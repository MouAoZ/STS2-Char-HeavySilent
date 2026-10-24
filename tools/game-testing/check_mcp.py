"""Exercise the actual stdio MCP protocol against a running managed game."""
import asyncio
import sys
import urllib.error
import urllib.request
from mcp import ClientSession, StdioServerParameters
from mcp.client.stdio import stdio_client
from client import HERE, GameClient, write_json


async def main():
    game = GameClient()
    params = StdioServerParameters(command=sys.executable, args=[str(HERE / 'mcp_server.py')])
    async with stdio_client(params) as (reader, writer):
        async with ClientSession(reader, writer) as connection:
            initialization = await connection.initialize()
            listed = await connection.list_tools()
            state = await connection.call_tool('get_state', {})
            shot = await connection.call_tool('capture_screenshot', {'name': 'mcp-protocol'})
            assert not state.isError and not shot.isError
            assert any(item.type == 'image' for item in shot.content)
            result = {'status': 'passed', 'server': initialization.serverInfo.name,
                      'tools': [tool.name for tool in listed.tools], 'state_call': 'passed', 'image_call': 'passed'}
    # An unauthenticated client must not even be able to inspect the managed state.
    opener = urllib.request.build_opener(urllib.request.ProxyHandler({}))
    try:
        opener.open('http://127.0.0.1:15526/hs/state', timeout=5)
        raise AssertionError('Unauthenticated state access was accepted')
    except urllib.error.HTTPError as error:
        assert error.code == 403
        result['unauthenticated_access'] = 'rejected_403'
    write_json(game.directory / 'mcp-protocol.json', result)
    print(result)


if __name__ == '__main__':
    asyncio.run(main())
