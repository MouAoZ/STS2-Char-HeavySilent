"""Visible-game acceptance. Expectations are design constants, independent of C# fixtures."""
from __future__ import annotations
import argparse
from collections import Counter
from datetime import datetime, timezone
from pathlib import Path
import time
import traceback
from client import ARTIFACTS, HERE, SESSION_FILE, GameClient, control_lock, digest, read_json, write_json
import session

SEED = "HEAVYSILENTTEST01"
CASES = {
    "strike": {"damage": 6, "energy": 1},
    "strike_plus": {"damage": 9, "energy": 1},
    "defend": {"block": 5, "energy": 1},
    "defend_plus": {"block": 8, "energy": 1},
    "qi": {"draw": 1, "energy": 0},
    "qi_plus": {"draw": 2, "energy": 0},
    "qi_shuffle": {"draw": 1, "energy": 0},
    "qi_empty": {"draw": 0, "energy": 0},
    "qi_full": {"draw": 1, "energy": 0},
    "qi_near_full": {"draw": 2, "energy": 0},
    "qi_no_draw": {"draw": 0, "energy": 0},
    "strength": {"damage": 8, "energy": 1},
    "weak": {"damage": 4, "energy": 1},
    "dexterity": {"block": 7, "energy": 1},
    "enemy_block": {"damage": 3, "enemy_block": 0, "energy": 1},
}


class RegressionClient(GameClient):
    def wait(self, predicate=lambda state: not state['busy'], timeout=30):
        def observe(state):
            # Fresh profiles can show several first-use tutorials even after unlock all.
            # Acknowledgements use the ordinary UI buttons, and only fixed runs do this.
            screen = state['ui'].get('menu_screen')
            if screen in ('tutorial', 'tutorial_prompt'):
                self.request('/api/v1/singleplayer', {'action': 'menu_select',
                             'option': 'no' if screen == 'tutorial_prompt' else 'advance'})
                time.sleep(.5)
                return False
            return predicate(state)
        return super().wait(observe, timeout)


def require(actual, expected, label):
    if actual != expected:
        raise AssertionError(f"{label}: expected {expected!r}, observed {actual!r}")


def deck_signature(state):
    return sorted((card['id'], card['upgraded']) for card in state['deck'])


def verify_card(name, before, after):
    expected = CASES[name]
    card = before['piles']['hand'][0]
    require(before['profile'], 3, 'test profile')
    require(after['energy'], before['energy'] - expected['energy'], 'energy')
    require(after['piles']['play'], [], 'action queue finished moving card')
    if 'damage' in expected:
        require(before['enemies'][0]['hp'] - after['enemies'][0]['hp'], expected['damage'], 'damage')
    if 'enemy_block' in expected:
        require(after['enemies'][0]['block'], expected['enemy_block'], 'enemy block')
    if 'block' in expected:
        require(after['block'] - before['block'], expected['block'], 'gained block')
    if 'draw' in expected:
        require(len(after['piles']['hand']), len(before['piles']['hand']) - 1 + expected['draw'], 'hand size')
        destination = 'exhaust'
        require(sum(c['instance_id'] == card['instance_id'] for c in after['piles']['exhaust']), 1, 'exhausted Qi')
        require(len(after['piles']['discard']), 0, 'Qi never enters discard')
        source_count = len(before['piles']['draw']) + len(before['piles']['discard'])
        require(len(after['piles']['draw']), source_count - expected['draw'], 'remaining draw pile')
    else:
        destination = 'discard'
    require(any(c['instance_id'] == card['instance_id'] for c in after['piles'][destination]), True, 'played card destination')
    # No cards may disappear or be duplicated during play, draw, shuffle, or exhaust.
    require(Counter(c['instance_id'] for pile in after['piles'].values() for c in pile),
            Counter(c['instance_id'] for pile in before['piles'].values() for c in pile), 'combat card conservation')
    require(deck_signature(after), deck_signature(before), 'permanent deck unchanged')


def options(state):
    return [o if isinstance(o, str) else o['name'] for o in state['ui'].get('options', [])
            if isinstance(o, str) or o.get('enabled', True)]


def enter_run(client, prefix, seed=SEED, upgrade_deck=True):
    state = client.wait(lambda s: not s['busy'] and bool(s.get('character_id') or options(s)))
    while not state.get('character_id') and state['ui'].get('menu_screen') in ('character_select', 'singleplayer'):
        state = client.action('menu_select', option='back')
    if state.get('character_id'):
        client.request('/hs/save-quit', {})
        state = client.wait(lambda s: not s.get('character_id'))
    client.request('/hs/profile-ready', {})
    state = client.wait()
    # Only this registered profile's previous automation run is abandoned.
    if 'abandon_run' in options(state):
        client.action('menu_select', option='abandon_run')
        state = client.wait(lambda s: s['ui'].get('menu_screen') == 'popup')
        client.action('menu_select', option=options(state)[0])
        client.wait(lambda s: s['ui'].get('menu_screen') == 'main')
        client.request('/hs/profile-ready', {})
        state = client.wait(lambda s: 'singleplayer' in options(s))
    client.action('menu_select', option='singleplayer')
    state = client.wait(lambda s: s['ui'].get('menu_screen') in ('singleplayer', 'character_select'))
    if state['ui']['menu_screen'] == 'singleplayer':
        client.action('menu_select', option='standard')
        client.wait(lambda s: s['ui'].get('menu_screen') == 'character_select')
    client.action('menu_select', option='HEAVYSILENT_CHARACTER')
    client.screenshot(prefix + '-character')
    client.action('menu_select', option='embark', seed=seed)
    state = client.wait(lambda s: s.get('character_id') or s['ui'].get('menu_screen') == 'tutorial_prompt')
    if state['ui'].get('menu_screen') == 'tutorial_prompt':
        client.action('menu_select', option='no')
    state = client.wait(lambda s: bool(s.get('character_id')))
    require(state['character_id'], 'HEAVYSILENT_CHARACTER', 'selected character')
    require(state['seed'], seed, 'actual run seed')
    if upgrade_deck:
        require(Counter(c['id'] for c in state['deck']),
                Counter(HEAVYSILENT_STRIKE=5, HEAVYSILENT_DEFEND=5,
                        HEAVYSILENT_GATHER_BREATH=1, HEAVYSILENT_OBLIQUE_STRIKE=1), 'starting deck')
        client.request('/hs/prepare', {'case_id': 'save_upgrade'})
    # Normal room navigation. A fresh profile skips Neow; an existing one may show it.
    for _ in range(12):
        state = client.state()
        if state['in_combat']:
            return client.wait(lambda s: s['play_phase'] and not s['busy'])
        kind = state['ui']['state_type']
        if kind == 'map':
            client.action('choose_map_node', index=state['ui']['map']['next_options'][0]['index'])
        elif kind == 'bundle_select':
            if not state['ui']['bundle_select']['preview_showing']:
                client.action('select_bundle', index=0)
            client.action('confirm_bundle_selection')
        elif kind in ('event', 'ancient'):
            state = client.wait(lambda s: not s['busy'] and s.get('event_ui_ready', False))
            if state['ui']['event'].get('in_dialogue'):
                client.action('advance_dialogue')
            else:
                client.action('choose_event_option', index=0)
        elif state['ui'].get('menu_screen') == 'tutorial':
            client.action('menu_select', option='advance')
        else:
            raise RuntimeError(f"Unrecognized run-entry UI: {state['ui']}")
        time.sleep(.5)
    raise TimeoutError('No first combat after 12 room transitions')


class Report:
    def __init__(self, client, suite):
        self.client = client
        self.path = client.directory / 'report.json'
        self.data = {'status': 'running', 'suite': suite, 'run_id': client.session['id'],
                     'seed': SEED, 'started_utc': datetime.now(timezone.utc).isoformat(),
                     'package': client.session['package'], 'bridge_sha256': client.session['bridge_sha256'],
                     'test_sources': {str(p.relative_to(HERE)): digest(p) for p in
                                      [HERE / 'runner.py', HERE / 'client.py', HERE / 'bridge/HsFixtures.cs', HERE / 'upstream.json']},
                     'visual_review': 'pending', 'cases': [], 'not_covered': [
                         '完整爬塔与奖励池', '联机', '磨顶与征召', '正式美术']}
        selected = list(CASES) if suite == 'regression' else ['strike', 'defend', 'qi']
        for number in range(1, 3 if suite == 'regression' else 2):
            for name in ['start', 'save-continue', *selected, 'text-' + client.state()['language'], 'end-turn']:
                self.data['cases'].append({'name': f'round{number}-{name}', 'status': 'unexecuted'})
        write_json(ARTIFACTS / 'latest.json', {'directory': str(client.directory)})
        self.save()

    def save(self):
        write_json(self.path, self.data)

    def case(self, name, action):
        entry = next(c for c in self.data['cases'] if c['name'] == name)
        entry['status'] = 'running'
        self.save()
        try:
            entry['evidence'] = action()
            entry['status'] = 'passed'
            print(f'PASS {name}', flush=True)
        except Exception as error:
            entry.update(status='failed', error=str(error), traceback=traceback.format_exc())
            try:
                entry['screenshot'] = str(self.client.screenshot(name + '-failed'))
                entry['state'] = self.client.state()
            except Exception as capture_error:
                entry['capture_error'] = str(capture_error)
            raise
        finally:
            self.save()


def play_case(client, name, prefix):
    before = client.prepare(name)
    arguments = {'card_index': 0}
    if 'damage' in CASES[name]:
        arguments['target'] = before['ui']['battle']['enemies'][0]['entity_id']
    client.action('play_card', **arguments)
    played = before['piles']['hand'][0]['instance_id']
    after = client.wait(lambda s: not s['busy'] and not s['piles']['play'] and
                        all(c['instance_id'] != played for c in s['piles']['hand']))
    evidence = {'before': before, 'after': after, 'expected': CASES[name]}
    write_json(client.directory / f'{prefix}-{name}.json', evidence)
    verify_card(name, before, after)
    return {'states': f'{prefix}-{name}.json', 'expected': CASES[name]}


def save_continue(client):
    before = client.state()
    require(sum(c['upgraded'] for c in before['deck']), 3, 'one upgraded copy of each sample')
    client.request('/hs/save-quit', {})
    client.wait(lambda s: not s.get('character_id') and 'continue' in options(s))
    client.action('menu_select', option='continue')
    after = client.wait(lambda s: s['play_phase'] and not s['busy'])
    require(deck_signature(after), deck_signature(before), 'restored permanent IDs and upgrades')
    require(after['seed'], SEED, 'restored seed')
    return {'deck_before': deck_signature(before), 'deck_after': deck_signature(after)}


def visual_cards(client, language, prefix):
    state = client.prepare('visual_cards')
    require([c['title'] for c in state['piles']['hand']],
            ['打击', '防御', '气'] if language == 'zhs' else ['Strike', 'Defend', 'Qi'], 'localized card titles')
    # Screenshot is evidence, never an automatic assertion of visual correctness.
    screenshots = [str(client.screenshot(f'{prefix}-cards-{language}'))]
    for index, name in enumerate(('strike', 'defend', 'qi')):
        client.request('/hs/inspect-card', {'index': index})
        time.sleep(.6)
        screenshots.append(str(client.screenshot(f'{prefix}-{name}-{language}')))
        client.request('/hs/inspect-card', {'close': True})
        time.sleep(.3)
    return {'screenshots': screenshots, 'visual_review': 'pending'}


def run(suite='regression', attach=False, keep_open=False, configuration='Debug', config=None, language=None):
    report = None
    owns = False
    try:
        data = read_json(SESSION_FILE) if attach else session.start(configuration, config, language)
        owns = True
        client = RegressionClient(data)
        report = Report(client, suite)
        rounds = 2 if suite == 'regression' else 1
        for number in range(1, rounds + 1):
            prefix = f'round{number}'
            report.case(prefix + '-start', lambda: {'state': enter_run(client, prefix)})
            report.case(prefix + '-save-continue', lambda: save_continue(client))
            selected = CASES if suite == 'regression' else ['strike', 'defend', 'qi']
            for name in selected:
                report.case(prefix + '-' + name, lambda n=name: play_case(client, n, prefix))
            for language in (client.state()['language'],):
                report.case(prefix + '-text-' + language, lambda lang=language: visual_cards(client, lang, prefix))
            # Check enemy AI and next-turn flow through EndTurn, without killing enemies.
            def next_turn():
                client.prepare('defend')
                before = client.state()
                client.action('end_turn')
                after = client.wait(lambda s: s['play_phase'] and not s['busy'] and
                                    s['ui']['battle']['round'] > before['ui']['battle']['round'])
                return {'round_before': before['ui']['battle']['round'], 'round_after': after['ui']['battle']['round']}
            report.case(prefix + '-end-turn', next_turn)
        report.data['status'] = 'passed'
    except Exception:
        if report:
            report.data.update(status='failed', error=traceback.format_exc())
        raise
    finally:
        cleanup = None
        # A failed startup may have a recoverable session record and owned process.
        if not keep_open and (owns or not attach and SESSION_FILE.exists()):
            try:
                cleanup = session.stop()
            except Exception:
                cleanup = {'status': 'failed', 'error': traceback.format_exc()}
        if report:
            report.data['cleanup'] = cleanup or {'status': 'kept_open'}
            if cleanup and cleanup['status'] == 'failed':
                report.data['status'] = 'failed'
            report.data['finished_utc'] = datetime.now(timezone.utc).isoformat()
            report.save()
            print(f'Report: {report.path}', flush=True)
        if cleanup and cleanup['status'] == 'failed':
            raise RuntimeError(cleanup['error'])


if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('--suite', choices=('smoke', 'regression'), default='regression')
    parser.add_argument('--attach', action='store_true')
    parser.add_argument('--keep-open', action='store_true')
    parser.add_argument('--configuration', default='Debug', choices=('Debug', 'Release'))
    parser.add_argument('--local-config')
    parser.add_argument('--language', choices=('eng', 'zhs'))
    args = parser.parse_args()
    with control_lock():
        run(args.suite, args.attach, args.keep_open, args.configuration, args.local_config, args.language)
