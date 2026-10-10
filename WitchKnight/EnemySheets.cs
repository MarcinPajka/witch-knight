using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace WitchKnight
{
    /// <summary>One creature's animations, cut from the sheets that art/rig/export_bestiary.py writes.</summary>
    internal sealed class CreatureSheets
    {
        public CreatureSheets(Dictionary<string, SheetAnimation> anims, bool centered)
        {
            Anims = anims;
            Centered = centered;
        }

        public Dictionary<string, SheetAnimation> Anims { get; }

        /// <summary>True for fliers, whose pivot is the middle of the body instead of the feet.</summary>
        public bool Centered { get; }
    }

    /// <summary>
    /// Loads enemy sprite sheets from the mod's embedded resources, one creature at a time,
    /// the first time that creature appears.
    /// </summary>
    internal static class EnemySheets
    {
        private const string Prefix = "WitchKnight.Resources.Enemies.";

        private static JObject? manifest;
        private static readonly Dictionary<string, CreatureSheets> cache = new();

        /// <param name="key">The creature key from enemies.json, e.g. "hopliteHusk".</param>
        /// <param name="heightInUnits">How tall the figure stands in game units.</param>
        public static CreatureSheets Get(string key, float heightInUnits)
        {
            if (cache.TryGetValue(key, out var hit)) return hit;

            var asm = Assembly.GetExecutingAssembly();
            manifest ??= JObject.Parse(ReadText(asm, Prefix + "enemies.json"));
            var entry = manifest[key] ?? throw new KeyNotFoundException("No sprites for creature " + key);
            var frame = entry["frame"]!;
            int w = (int)frame["w"]!, h = (int)frame["h"]!;
            var pivot = new Vector2((float)frame["pivotX"]! / w, 1f - (float)frame["pivotY"]! / h);
            float pixelsPerUnit = (float)entry["figure"]! / heightInUnits;

            var anims = new Dictionary<string, SheetAnimation>();
            foreach (var prop in ((JObject)entry["anims"]!).Properties())
            {
                var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                tex.LoadImage(ReadBytes(asm, $"{Prefix}{key}.{prop.Name}.png"));
                tex.filterMode = FilterMode.Bilinear;
                tex.wrapMode = TextureWrapMode.Clamp;

                int count = (int)prop.Value["frames"]!;
                var sprites = new Sprite[count];
                for (int i = 0; i < count; i++)
                {
                    sprites[i] = Sprite.Create(tex, new Rect(i * w, 0, w, h), pivot, pixelsPerUnit);
                    sprites[i].name = $"{key}_{prop.Name}_{i}";
                }

                anims[prop.Name] = new SheetAnimation(sprites, (float)prop.Value["fps"]!, (bool)prop.Value["loop"]!);
            }

            var sheets = new CreatureSheets(anims, (string?)entry["anchor"] == "center");
            cache[key] = sheets;
            return sheets;
        }

        private static byte[] ReadBytes(Assembly asm, string name)
        {
            using var stream = asm.GetManifestResourceStream(name)
                ?? throw new FileNotFoundException("Missing embedded resource " + name);
            using var ms = new MemoryStream();
            stream.CopyTo(ms);
            return ms.ToArray();
        }

        private static string ReadText(Assembly asm, string name) =>
            System.Text.Encoding.UTF8.GetString(ReadBytes(asm, name));
    }
}
