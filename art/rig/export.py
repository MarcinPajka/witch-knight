"""Render the Hex Moth rig to PNG sprite sheets plus a JSON manifest.

Usage: python3 art/rig/export.py [chrome-binary]
Writes WitchKnight/Resources/Sprites/<anim>.png and anims.json.
"""
import base64, html, json, pathlib, re, subprocess, sys

root = pathlib.Path(__file__).resolve().parents[2]
chrome = sys.argv[1] if len(sys.argv) > 1 else "chromium"
page = (root / "art/rig/export.html").as_uri()
dom = subprocess.run([chrome, "--headless", "--no-sandbox", "--disable-gpu", "--allow-file-access-from-files",
                      "--dump-dom", page], capture_output=True, text=True, check=True).stdout
data = json.loads(html.unescape(re.search(r'<pre id="out">(.*?)</pre>', dom, re.S).group(1)))
dest = root / "WitchKnight/Resources/Sprites"
dest.mkdir(parents=True, exist_ok=True)
manifest = {"frame": data["frame"], "anims": {}}
for name, a in data["anims"].items():
    (dest / f"{name}.png").write_bytes(base64.b64decode(a["png"].split(",", 1)[1]))
    manifest["anims"][name] = {k: a[k] for k in ("fps", "loop", "frames")}
(dest / "anims.json").write_text(json.dumps(manifest, indent=2) + "\n")
print("wrote", ", ".join(sorted(manifest["anims"])), "to", dest.relative_to(root))
