"""Tests for acceptance oracles and automation safety, without launching the game."""
import copy
import importlib.util
from pathlib import Path
import sys
import subprocess
import tempfile
import unittest
from unittest.mock import patch

TOOLS = Path(__file__).resolve().parents[1] / 'tools/game-testing'
sys.path.insert(0, str(TOOLS))
import bootstrap
import client
import runner
import session
import tasks


def qi_states():
    qi = {'id': 'HEAVYSILENT_QI', 'instance_id': 1, 'upgraded': False}
    defend = {'id': 'HEAVYSILENT_DEFEND', 'instance_id': 2, 'upgraded': False}
    before = {'profile': 3, 'energy': 3, 'deck': [qi.copy()],
              'piles': {'hand': [qi], 'draw': [defend], 'discard': [], 'exhaust': [], 'play': []}}
    after = copy.deepcopy(before)
    after['piles'] = {'hand': [defend.copy()], 'draw': [], 'discard': [], 'exhaust': [qi.copy()], 'play': []}
    return before, after


class GameTestingTests(unittest.TestCase):
    def test_action_returns_live_selection_without_resending_play(self):
        game = client.GameClient({'directory': '.'})
        choice = {'busy': True, 'ui': {'state_type': 'hand_select'}}
        states = [{'busy': True, 'ui': {'state_type': 'monster'}}, choice, choice, choice]
        with patch.object(game, 'request') as request, patch.object(game, 'state', side_effect=states), patch('client.time.sleep'):
            self.assertEqual(game.action('play_card', card_index=0), choice)
        request.assert_called_once_with('/api/v1/singleplayer', {'action': 'play_card', 'card_index': 0})

    def test_worktrees_share_controller_and_preserve_evidence(self):
        # Real Git metadata exercises both directory and linked-worktree .git forms.
        with tempfile.TemporaryDirectory() as temp:
            primary = Path(temp) / 'primary repo'
            task = Path(temp) / 'task tree'
            primary.mkdir()

            def git(*args):
                return subprocess.run(
                    ['git', '-C', str(primary), *args],
                    check=True, capture_output=True,
                )

            git('init', '--template=')
            git('-c', 'user.name=Test', '-c', 'user.email=test@example.invalid',
                '-c', 'core.hooksPath=', 'commit', '--allow-empty', '-m', 'fixture')
            git('worktree', 'add', '--detach', str(task))
            artifacts = primary / 'artifacts/game-tests'
            self.assertEqual(client.shared_artifacts(primary), artifacts)
            self.assertEqual(client.shared_artifacts(task), artifacts)
            with patch.object(client, 'ARTIFACTS', client.shared_artifacts(primary)):
                with client.control_lock():
                    with patch.object(client, 'ARTIFACTS', client.shared_artifacts(task)):
                        with self.assertRaisesRegex(RuntimeError, 'owns the game controls'):
                            with client.control_lock():
                                self.fail('Another worktree acquired the same game')
            # Evidence survives removal of the disposable checkout.
            evidence = artifacts / 'report.json'
            evidence.write_text('{}')
            git('worktree', 'remove', str(task))
            self.assertTrue(evidence.exists())

    def test_source_export_uses_local_game_state(self):
        with tempfile.TemporaryDirectory() as temp:
            root = Path(temp)
            self.assertEqual(client.shared_artifacts(root), root / 'artifacts/game-tests')

    def test_state_identifies_source_and_controller_worktrees(self):
        game = client.GameClient({'directory': '.', 'source_root': 'another-worktree'})
        with patch.object(game, 'request', return_value={'profile': 3}):
            state = game.state()
        self.assertEqual(state['profile'], 3)
        self.assertEqual(state['source_root'], 'another-worktree')
        self.assertEqual(state['controller_root'], str(client.ROOT))

    def test_git_discovery_failure_does_not_create_a_second_controller(self):
        with tempfile.TemporaryDirectory() as temp:
            root = Path(temp)
            (root / '.git').mkdir()
            with patch('client.subprocess.run', side_effect=subprocess.CalledProcessError(1, 'git')):
                with self.assertRaises(subprocess.CalledProcessError):
                    client.shared_artifacts(root)

    def test_patch_context_drift_is_rejected(self):
        with self.assertRaisesRegex(RuntimeError, 'context changed'):
            bootstrap.replace_exact('new upstream', 'old upstream', 'patched')
        with self.assertRaises(RuntimeError):
            bootstrap.replace_exact('old old', 'old', 'patched')

    def test_no_process_powershell_null(self):
        with patch('session.subprocess.run') as command:
            command.return_value.stdout = 'null'
            self.assertEqual(session.game_pids(), [])
            command.return_value.stdout = '123'
            self.assertEqual(session.game_pids(), [123])

    def test_qi_oracle_accepts_real_pile_transition(self):
        runner.verify_card('qi', *qi_states())

    def test_qi_oracle_rejects_wrong_energy(self):
        before, after = qi_states()
        after['energy'] = 2
        with self.assertRaisesRegex(AssertionError, 'energy'):
            runner.verify_card('qi', before, after)

    def test_qi_oracle_rejects_discard_instead_of_exhaust(self):
        before, after = qi_states()
        after['piles']['discard'] = after['piles']['exhaust']
        after['piles']['exhaust'] = []
        with self.assertRaisesRegex(AssertionError, 'exhausted Qi'):
            runner.verify_card('qi', before, after)

    def test_qi_oracle_rejects_card_duplication(self):
        before, after = qi_states()
        after['piles']['exhaust'].append({'id': 'ILLEGAL', 'instance_id': 99})
        with self.assertRaisesRegex(AssertionError, 'conservation'):
            runner.verify_card('qi', before, after)

    def test_personal_hashes_detect_creation_edit_and_deletion(self):
        with tempfile.TemporaryDirectory() as temp:
            root = Path(temp)
            save = root / 'modded/profile1/progress.save'
            save.parent.mkdir(parents=True)
            save.write_text('first')
            first = session.personal_hashes(root)
            save.write_text('second')
            self.assertNotEqual(first, session.personal_hashes(root))
            save.unlink()
            self.assertEqual(session.personal_hashes(root), {})

    def test_hash_scope_does_not_change_for_existing_sessions(self):
        with tempfile.TemporaryDirectory() as temp:
            root = Path(temp)
            original = root / 'profile1/progress.save'
            original.parent.mkdir(parents=True)
            original.write_text('personal')
            self.assertEqual(session.personal_hashes(root, False), {})
            self.assertEqual(len(session.personal_hashes(root, True)), 1)

    def test_task_cannot_report_unexecuted_check_as_passed(self):
        with tempfile.TemporaryDirectory() as temp, patch.object(tasks, 'ARTIFACTS', Path(temp)):
            game = client.GameClient({'directory': temp, 'token': 'fake', 'id': 'test', 'package': {}, 'bridge_sha256': 'fake'})
            with patch.object(game, 'state', return_value={'profile': 3}):
                tasks.begin(game, 'change', 'reason', [{'id': 'a', 'question': 'what', 'expected': 'why', 'reason': 'risk'}])
                result = tasks.finish(game, 'Not executed')
            self.assertEqual(result['status'], 'incomplete')

    def test_task_rejects_evidence_outside_session(self):
        with tempfile.TemporaryDirectory() as temp, patch.object(tasks, 'ARTIFACTS', Path(temp)):
            root = Path(temp)
            directory = root / 'session'
            directory.mkdir()
            secret = root / 'unrelated.txt'
            secret.write_text('unrelated')
            game = client.GameClient({'directory': str(directory), 'token': 'fake', 'id': 'test', 'package': {}, 'bridge_sha256': 'fake'})
            with patch.object(game, 'state', return_value={'profile': 3}):
                tasks.begin(game, 'change', 'reason', [{'id': 'a', 'question': 'what', 'expected': 'why', 'reason': 'risk'}])
                with self.assertRaisesRegex(ValueError, 'session directory'):
                    tasks.observe(game, 'a', 'passed', 'seen', 'because', ['../unrelated.txt'])

    def test_wait_requires_repeated_observation(self):
        game = client.GameClient({'directory': '.', 'token': 'fake'})
        with patch.object(game, 'state', side_effect=[{'busy': True}, {'busy': False}, {'busy': False}, {'busy': False}]) as state:
            with patch('client.time.sleep'):
                self.assertEqual(game.wait(), {'busy': False})
            self.assertEqual(state.call_count, 4)

    def test_controller_lock_excludes_second_writer(self):
        with tempfile.TemporaryDirectory() as temp, patch.object(client, 'ARTIFACTS', Path(temp)):
            with client.control_lock():
                with self.assertRaisesRegex(RuntimeError, 'owns the game controls'):
                    with client.control_lock():
                        self.fail('A second writer entered the game controls')
            with client.control_lock():
                pass  # released even when the inner acquisition failed


if __name__ == '__main__':
    unittest.main()
