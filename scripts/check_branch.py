"""Compare the installed Steam build with public-beta metadata; never update files."""
import json
import sys
import urllib.error
import urllib.request

URL = "https://api.steamcmd.net/v1/info/2868840"

if __name__ == "__main__":
    try:
        with urllib.request.urlopen(URL, timeout=20) as response:
            build = json.load(response)["data"]["2868840"]["depots"]["branches"]["public-beta"]["buildid"]
        installed = sys.argv[1] if len(sys.argv) > 1 else ""
        print(f"SteamCMD metadata mirror: public-beta={build}, installed={installed}")
        if str(build) != installed:
            print("Build differs. Let Steam update public-beta, then rerun doctor and compatibility checks.")
            sys.exit(1)
    except (OSError, ValueError, KeyError) as error:
        print(f"Could not confirm latest beta: {error}. Check the Steam client.", file=sys.stderr)
        sys.exit(2)
