using System.Collections.Generic;
using System.Linq;
using Modding;
using UnityEngine;

namespace WitchKnight
{
    /// <summary>
    /// Text barks and the run HUD: who said what at the bottom of the screen, the Favours
    /// held in the top right, and the Favour offer when the patrons make one.
    /// Drawn with Unity's immediate-mode GUI so it needs no assets.
    /// </summary>
    internal class Barks : MonoBehaviour
    {
        public static readonly Color MothColour = new(0.95f, 0.85f, 0.55f);

        private const float LineSeconds = 4.5f;
        private const int MaxLines = 3;
        private const float SceneBarkCooldown = 25f;

        private struct Line
        {
            public string Speaker;
            public string Text;
            public Color Colour;
            public float Born;
        }

        private static readonly List<Line> lines = new();
        private static float lastSceneBark = -999f;
        private static float lastHurtBark = -999f;
        private static readonly System.Random rng = new();

        private static readonly string[] arrivals =
        {
            "Deeper, then. The moon is patient.",
            "Smells of old bronze and older grudges.",
            "Shield up. Spear out. Wings shut.",
            "Something here remembers the Gods.",
            "Another hall of the dead. Lovely.",
            "If I hum, they hear me. I shall not hum.",
            "The dark tastes of iron here.",
            "Keep moving, little moth.",
        };

        private static readonly string[] hurts =
        {
            "Ngh! Bronze holds.",
            "That one stung.",
            "Careful, moth.",
            "My wings!",
            "Is that all?",
        };

        private static Barks? runner;

        public static void Hook()
        {
            var go = new GameObject("WitchKnight Barks");
            DontDestroyOnLoad(go);
            runner = go.AddComponent<Barks>();

            ModHooks.SceneChanged += OnSceneChanged;
        }

        // Coroutines for effects that outlive a scene object, such as burns and pulses.
        public static void Run(System.Collections.IEnumerator routine)
        {
            if (runner != null) runner.StartCoroutine(routine);
        }

        public static void Say(string speaker, string text, Color colour)
        {
            lines.Add(new Line { Speaker = speaker, Text = text, Colour = colour, Born = Time.unscaledTime });
            while (lines.Count > MaxLines) lines.RemoveAt(0);
        }

        public static void OnHurt()
        {
            if (Time.unscaledTime - lastHurtBark < 8f || Random.value > 0.35f) return;
            lastHurtBark = Time.unscaledTime;
            Say("Hex Moth", hurts[rng.Next(hurts.Length)], MothColour);
        }

        private static void OnSceneChanged(string scene)
        {
            if (!InGame()) return;
            if (Time.unscaledTime - lastSceneBark < SceneBarkCooldown || Random.value > 0.4f) return;
            lastSceneBark = Time.unscaledTime;
            Say("Hex Moth", arrivals[rng.Next(arrivals.Length)], MothColour);
        }

        private static bool InGame()
        {
            var gm = GameManager.instance;
            return gm != null && HeroController.instance != null && !gm.IsNonGameplayScene();
        }

        private void Update()
        {
            var keys = WitchKnightMod.Settings;
            if (!InGame()) return;
            if (Input.GetKeyDown(keys.MoonHexKey)) Hex.TryCast();
            if (Input.GetKeyDown(keys.ParryKey)) Parry.TryRaise();

            if (Route.Choosing)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)) Route.Choose(RouteKind.Descent);
                else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2)) Route.Choose(RouteKind.Ascent);
                return;
            }

            if (Favours.Offer == null) return;
            if (Input.GetKeyDown(keys.RedrawKey)) { Favours.Reroll(); return; }
            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)) Favours.Choose(0);
            else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2)) Favours.Choose(1);
            else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3)) Favours.Choose(2);
        }

        private GUIStyle? text;
        private GUIStyle? small;

        private void OnGUI()
        {
            if (!InGame() || GameManager.instance.IsGamePaused()) return;

            var scale = Screen.height / 1080f;
            text ??= new GUIStyle(GUI.skin.label) { richText = true, wordWrap = true };
            small ??= new GUIStyle(GUI.skin.label) { richText = true, alignment = TextAnchor.UpperRight };
            text.fontSize = Mathf.RoundToInt(26 * scale);
            small.fontSize = Mathf.RoundToInt(22 * scale);

            if (WitchKnightMod.Settings.ShowHud) DrawFavours(scale);
            if (Route.Choosing) DrawRouteChoice(scale);
            else DrawOffer(scale);
            DrawLines(scale);
        }

        private void DrawFavours(float scale)
        {
            var w = 460 * scale;
            var y = 20 * scale;
            var x = Screen.width - w - 24 * scale;
            var lh = 30 * scale;

            var pct = Mathf.RoundToInt(Hex.Charge / Hex.MaxCharge * 100f);
            var moon = Hex.Ready ? $"<b>Moon Hex READY [{WitchKnightMod.Settings.MoonHexKey}]</b>" : $"Moon Hex {pct}%";
            Shadowed(new Rect(x, y, w, lh), moon, Hex.Ready ? new Color(0.85f, 0.9f, 1f) : new Color(1f, 1f, 1f, 0.6f), small!);
            y += lh;
            Shadowed(new Rect(x, y, w, lh), $"Next Favour  {Favours.KillsTowardNext}/{Favours.KillsNeeded}", new Color(1f, 1f, 1f, 0.75f), small!);
            y += lh;

            foreach (var kv in Favours.All.OrderByDescending(kv => kv.Value))
            {
                Shadowed(new Rect(x, y, w, lh), $"{Favours.FavourName(kv.Key)} {Roman(kv.Value)}", Favours.Colour(kv.Key), small!);
                y += lh;
            }
        }

        private void DrawRouteChoice(float scale)
        {
            var routes = new[] { RouteKind.Descent, RouteKind.Ascent };
            var cardW = 460 * scale;
            var cardH = 190 * scale;
            var gap = 30 * scale;
            var x = (Screen.width - (2 * cardW + gap)) / 2f;
            var y = Screen.height * 0.18f;

            for (var i = 0; i < routes.Length; i++)
            {
                var r = new Rect(x + i * (cardW + gap), y, cardW, cardH);
                var c = Route.Colour(routes[i]);
                Card(r, c, scale,
                    $"<b>[{i + 1}]  <color=#{ColorUtility.ToHtmlStringRGB(c)}>{Route.Name(routes[i])}</color></b>\n" +
                    $"<size={Mathf.RoundToInt(19 * scale)}>{Route.Describe(routes[i])}</size>");
            }
        }

        private void Card(Rect r, Color c, float scale, string body)
        {
            GUI.color = new Color(0f, 0f, 0f, 0.7f);
            GUI.DrawTexture(r, Texture2D.whiteTexture);
            GUI.color = c;
            GUI.DrawTexture(new Rect(r.x, r.y, r.width, 4 * scale), Texture2D.whiteTexture);
            GUI.color = Color.white;
            var pad = 14 * scale;
            GUI.Label(new Rect(r.x + pad, r.y + pad, r.width - 2 * pad, r.height - 2 * pad), body, text!);
        }

        private void DrawOffer(float scale)
        {
            var offer = Favours.Offer;
            if (offer == null) return;

            var cardW = 420 * scale;
            var cardH = 210 * scale;
            var gap = 24 * scale;
            var total = offer.Count * cardW + (offer.Count - 1) * gap;
            var x = (Screen.width - total) / 2f;
            var y = Screen.height * 0.18f;

            for (var i = 0; i < offer.Count; i++)
            {
                var p = offer[i];
                var r = new Rect(x + i * (cardW + gap), y, cardW, cardH);
                var c = Favours.Colour(p);

                GUI.color = new Color(0f, 0f, 0f, 0.7f);
                GUI.DrawTexture(r, Texture2D.whiteTexture);
                GUI.color = c;
                GUI.DrawTexture(new Rect(r.x, r.y, r.width, 4 * scale), Texture2D.whiteTexture);
                GUI.color = Color.white;

                var rarity = Favours.OfferRarity?[i] ?? 1;
                var next = Mathf.Min(Favours.Level(p) + rarity, Favours.MaxLevel);
                var rhex = ColorUtility.ToHtmlStringRGB(Favours.RarityColour(rarity));
                var pad = 14 * scale;
                var inner = new Rect(r.x + pad, r.y + pad, r.width - 2 * pad, r.height - 2 * pad);
                var hex = ColorUtility.ToHtmlStringRGB(c);
                GUI.Label(inner,
                    $"<b>[{i + 1}]  <color=#{hex}>{Favours.FavourName(p)} {Roman(next)}</color></b>\n" +
                    $"<size={Mathf.RoundToInt(19 * scale)}><color=#{rhex}>{Favours.RarityName(rarity)}</color> · <i>{Favours.Name(p)}</i>\n{Favours.Describe(p, next)}</size>",
                    text!);
            }
        }

        private void DrawLines(float scale)
        {
            var now = Time.unscaledTime;
            lines.RemoveAll(l => now - l.Born > LineSeconds);

            var w = Mathf.Min(1100 * scale, Screen.width - 40 * scale);
            var lh = 40 * scale;
            var y = Screen.height - 150 * scale - lines.Count * lh;
            foreach (var l in lines)
            {
                var age = now - l.Born;
                var alpha = Mathf.Clamp01((LineSeconds - age) / 0.8f) * Mathf.Clamp01(age / 0.2f);
                var hex = ColorUtility.ToHtmlStringRGB(l.Colour);
                var c = new Color(1f, 1f, 1f, alpha);
                Shadowed(new Rect((Screen.width - w) / 2f, y, w, lh), $"<b><color=#{hex}>{l.Speaker}:</color></b> {l.Text}", c, text!);
                y += lh;
            }
        }

        private static void Shadowed(Rect r, string s, Color c, GUIStyle style)
        {
            GUI.color = new Color(0f, 0f, 0f, c.a * 0.8f);
            GUI.Label(new Rect(r.x + 2, r.y + 2, r.width, r.height), StripColour(s), style);
            GUI.color = c;
            GUI.Label(r, s, style);
            GUI.color = Color.white;
        }

        // The shadow pass must be plain black, so drop inline colour tags from it.
        private static string StripColour(string s) =>
            System.Text.RegularExpressions.Regex.Replace(s, "<color=[^>]*>|</color>", "");

        private static string Roman(int n) => n switch { 1 => "I", 2 => "II", 3 => "III", 4 => "IV", _ => "V" };
    }
}
