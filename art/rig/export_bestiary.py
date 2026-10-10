"""Render the bestiary rig to PNG sprite sheets plus a JSON manifest.

Usage: python3 art/rig/export_bestiary.py [chrome-binary]
Writes WitchKnight/Resources/Enemies/<creature>.<anim>.png and enemies.json.
"""
import base64, html, json, pathlib, re, subprocess, sys

root = pathlib.Path(__file__).resolve().parents[2]
chrome = sys.argv[1] if len(sys.argv) > 1 else "chromium"
page = (root / "art/rig/export_bestiary.html").as_uri()
dom = subprocess.run([chrome, "--headless", "--no-sandbox", "--disable-gpu", "--allow-file-access-from-files",
                      "--dump-dom", page], capture_output=True, text=True, check=True).stdout
data = json.loads(html.unescape(re.search(r'<pre id="out">(.*?)</pre>', dom, re.S).group(1)))
dest = root / "WitchKnight/Resources/Enemies"
dest.mkdir(parents=True, exist_ok=True)
manifest = {}
for key, cr in data.items():
    entry = {"frame": cr["frame"], "figure": cr["figure"], "anchor": cr["anchor"], "anims": {}}
    for name, a in cr["anims"].items():
        (dest / f"{key}.{name}.png").write_bytes(base64.b64decode(a["png"].split(",", 1)[1]))
        entry["anims"][name] = {k: a[k] for k in ("fps", "loop", "frames")}
    manifest[key] = entry
(dest / "enemies.json").write_text(json.dumps(manifest, indent=2) + "\n")
print("wrote", ", ".join(sorted(manifest)), "to", dest.relative_to(root))
