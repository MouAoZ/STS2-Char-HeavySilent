"""An Agent's test intent, observations and reasoning, separate from optional regressions."""
from datetime import datetime, timezone
from pathlib import Path
import argparse
import json
import re
import secrets
from client import ARTIFACTS, SESSION_FILE, GameClient, control_lock, read_json, write_json


def validate_check(check):
    if not re.fullmatch(r'[a-zA-Z0-9_-]+', check.get('id', '')):
        raise ValueError('Check id must contain letters, digits, underscore or hyphen')
    for field in ('question', 'expected', 'reason'):
        if not isinstance(check.get(field), str) or not check[field].strip():
            raise ValueError(f'Check requires a nonempty {field}')


def task_path(client):
    pointer = client.directory / 'active-task.json'
    return client.directory / read_json(pointer)['path']


def save_task(path, data):
    write_json(path, data)
    lines = [f"# {data['title']}", '', data['change_summary'], '',
             f"状态：{data['status']}；会话：{data['session_id']}", '']
    for check in data['checks']:
        lines += [f"## {check['id']} — {check['status']}", '',
                  f"问题：{check['question']}", '', f"预期：{check['expected']}", '', f"为何检查：{check['reason']}", '']
        for observation in check['observations']:
            lines += [f"判断：{observation['verdict']}。{observation['reasoning']}", '',
                      f"观察：{observation['observed']}", '', '证据：' + ', '.join(observation['evidence']), '']
    if data.get('summary'):
        lines += ['## Review 说明', '', data['summary'], '']
    path.with_suffix('.md').write_text('\n'.join(lines), encoding='utf-8')


def begin(client, title, change_summary, checks):
    if not title.strip() or not change_summary.strip() or not checks:
        raise ValueError('Provide the change being tested and at least one reasoned check')
    if (client.directory / 'active-task.json').exists() and read_json(task_path(client))['status'] == 'running':
        raise RuntimeError('Finish the active test task before beginning another')
    for check in checks:
        validate_check(check)
    if len({c['id'] for c in checks}) != len(checks):
        raise ValueError('Check IDs must be unique')
    name = datetime.now(timezone.utc).strftime('%H%M%S') + '-' + secrets.token_hex(3)
    path = client.directory / 'tasks' / (name + '.json')
    data = {'title': title, 'change_summary': change_summary, 'session_id': client.session['id'],
            'status': 'running', 'started_utc': datetime.now(timezone.utc).isoformat(),
            'package': client.session['package'], 'bridge_sha256': client.session['bridge_sha256'],
            'initial_state': client.state(),
            'checks': [{**c, 'status': 'unexecuted', 'observations': []} for c in checks]}
    save_task(path, data)
    write_json(client.directory / 'active-task.json', {'path': str(path.relative_to(client.directory))})
    write_json(ARTIFACTS / 'latest.json', {'directory': str(client.directory)})
    return {'task': str(path), 'review': str(path.with_suffix('.md')), 'status': 'running'}


def add_check(client, check):
    validate_check(check)
    path = task_path(client); data = read_json(path)
    if data['status'] != 'running' or any(c['id'] == check['id'] for c in data['checks']):
        raise ValueError('Task must be running and check ID must be new')
    data['checks'].append({**check, 'status': 'unexecuted', 'observations': []})
    save_task(path, data)
    return check


def observe(client, check_id, verdict, observed, reasoning, evidence):
    if verdict not in ('passed', 'failed', 'blocked') or not reasoning.strip() or not observed.strip():
        raise ValueError('Provide observed behavior and reasoning for passed/failed/blocked')
    path = task_path(client); data = read_json(path)
    if data['status'] != 'running':
        raise ValueError('Task is already finished')
    check = next(c for c in data['checks'] if c['id'] == check_id)
    files = []
    for name in evidence:
        candidate = Path(name) if Path(name).is_absolute() else client.directory / name
        candidate = candidate.resolve()
        if not candidate.is_relative_to(client.directory.resolve()) or not candidate.is_file():
            raise ValueError('Evidence must be an existing file in this session directory')
        files.append(str(candidate.relative_to(client.directory)))
    snapshot = path.parent / f'{path.stem}-{check_id}-{len(check["observations"]) + 1}.json'
    write_json(snapshot, client.state())
    files.append(str(snapshot.relative_to(client.directory)))
    check['observations'].append({'verdict': verdict, 'observed': observed, 'reasoning': reasoning,
                                  'evidence': files, 'time_utc': datetime.now(timezone.utc).isoformat()})
    check['status'] = verdict
    save_task(path, data)
    return check


def finish(client, summary):
    if not summary.strip():
        raise ValueError('Summarize findings and what remains untested')
    path = task_path(client); data = read_json(path)
    statuses = [c['status'] for c in data['checks']]
    data['status'] = 'failed' if 'failed' in statuses else 'passed' if all(s == 'passed' for s in statuses) else 'incomplete'
    data['summary'] = summary
    data['finished_utc'] = datetime.now(timezone.utc).isoformat()
    save_task(path, data)
    return {'status': data['status'], 'task': str(path), 'review': str(path.with_suffix('.md'))}


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description='Launch a game for an Agent-authored test task; does not auto-run regressions')
    parser.add_argument('--file', required=True)
    parser.add_argument('--attach', action='store_true')
    parser.add_argument('--configuration', default='Debug')
    parser.add_argument('--local-config')
    parser.add_argument('--language', choices=('eng', 'zhs'))
    args = parser.parse_args()
    plan = read_json(args.file)
    for item in plan['checks']:
        validate_check(item)
    import session
    with control_lock():
        data = read_json(SESSION_FILE) if args.attach else session.start(args.configuration, args.local_config, args.language)
        print(json.dumps(begin(GameClient(data), plan['title'], plan['change_summary'], plan['checks']), ensure_ascii=False))
