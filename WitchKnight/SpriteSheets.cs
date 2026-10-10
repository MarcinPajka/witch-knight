using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace WitchKnight
{
    /// <summary>One animation cut from a horizontal sprite sheet.</summary>
    internal sealed class SheetAnimation
    {
        public SheetAnimation(Sprite[] frames, float fps, bool loop)
        {
            Frames = frames;
            Fps = fps;
            Loop = loop;
        }

        public Sprite[] Frames { get; }
        public float Fps { get; }
        public bool Loop { get; }
        public float Duration => Frames.Length / Fps;
    }

    /// <summary>
    /// Loads the sprite sheets exported by art/rig/export.py from the mod's embedded resources.
    /// </summary>
    internal static class SpriteSheets
    {
        private const string Prefix = "WitchKnight.Resources.Sprites.";

        // Hex Moth stands about 1.95 units tall in game, a little taller than the Knight with his horns.
        private const float HeightInUnits = 1.95f;
        private const float FigureHeightInPixels = 380f;

        private static Dictionary<string, SheetAnimation>? cache;

        public static Dictionary<string, SheetAnimation> Load()
        {
            if (cache != null) return cache;

            var asm = Assembly.GetExecutingAssembly();
            var manifest = JObject.Parse(ReadText(asm, Prefix + "anims.json"));
            var frame = manifest["frame"]!;
            int w = (int)frame["w"]!, h = (int)frame["h"]!;
            var pivot = new Vector2((float)frame["pivotX"]! / w, 1f - (float)frame["pivotY"]! / h);
            float pixelsPerUnit = FigureHeightInPixels / HeightInUnits;

            cache = new Dictionary<string, SheetAnimation>();
            foreach (var prop in ((JObject)manifest["anims"]!).Properties())
            {
                var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                tex.LoadImage(ReadBytes(asm, Prefix + prop.Name + ".png"));
                tex.filterMode = FilterMode.Bilinear;
                tex.wrapMode = TextureWrapMode.Clamp;

                int count = (int)prop.Value["frames"]!;
                var sprites = new Sprite[count];
                for (int i = 0; i < count; i++)
                {
                    sprites[i] = Sprite.Create(tex, new Rect(i * w, 0, w, h), pivot, pixelsPerUnit);
                    sprites[i].name = $"HexMoth_{prop.Name}_{i}";
                }

                cache[prop.Name] = new SheetAnimation(sprites, (float)prop.Value["fps"]!, (bool)prop.Value["loop"]!);
            }

            return cache;
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
