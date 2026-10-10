using System.Collections;
using Modding;
using UnityEngine;

namespace WitchKnight
{
    /// <summary>
    /// Hex Moth's combat magic on top of the moveset:
    /// the Moon Hex (press F when charged), the Ember Choir's burn,
    /// the Hollow Bell's toll, and the run's tally.
    /// </summary>
    internal static class Hex
    {
        public const float MaxCharge = 100f;
        private const float ChargePerHit = 9f;
        private const float HexRadius = 9f;
        private const int HexDamage = 30;

        private const int TollEvery = 6;
        private const float TollRadius = 4.5f;

        public static float Charge { get; private set; }
        public static bool Ready => Charge >= MaxCharge;

        // Run tally, shown when she dies.
        public static int Kills;
        public static int HexesCast;
        public static int FavoursTaken;

        private static int nailHits;
        private static Sprite? ring;

        public static void Hook()
        {
            ModHooks.HitInstanceHook += OnHit;
            ModHooks.RecordKillForJournalHook += (_, _, _, _, _) => Kills++;
        }

        public static void ResetRun()
        {
            Charge = 0f;
            nailHits = 0;
            Kills = 0;
            HexesCast = 0;
            FavoursTaken = 0;
        }

        public static string Tally() =>
            $"{Kills} foes felled, {FavoursTaken} Favours, {HexesCast} Moon Hexes.";

        private static HitInstance OnHit(HutongGames.PlayMaker.Fsm owner, HitInstance hit)
        {
            if (hit.AttackType != AttackTypes.Nail || hit.IsExtraDamage) return hit;

            var target = owner?.GameObject != null ? owner.GameObject.GetComponent<HealthManager>() : null;
            if (target == null) return hit;

            var wasReady = Ready;
            Charge = Mathf.Min(MaxCharge, Charge + ChargePerHit * (Route.Current == RouteKind.Ascent ? 2f : 1f));
            if (!wasReady && Ready) Barks.Say("Hex Moth", $"The moon is full. [{WitchKnightMod.Settings.MoonHexKey}]", Barks.MothColour);

            var ember = Favours.Level(Patron.EmberChoir);
            if (ember > 0) Barks.Run(Burn(target, ember));

            var bell = Favours.Level(Patron.HollowBell);
            if (bell > 0 && ++nailHits % TollEvery == 0)
            {
                Toll(target.transform.position, bell);
            }
            return hit;
        }

        // Ember Choir: three ticks of fire after each spear hit.
        private static IEnumerator Burn(HealthManager target, int level)
        {
            for (var i = 0; i < 3; i++)
            {
                yield return new WaitForSeconds(0.5f);
                if (target == null || target.GetIsDead()) yield break;
                target.ApplyExtraDamage(level);
            }
        }

        // Hollow Bell: every sixth hit tolls, striking everything close by.
        private static void Toll(Vector3 at, int level)
        {
            Strike(at, TollRadius, 4 * level);
            Pulse(at, TollRadius, Favours.Colour(Patron.HollowBell));
        }

        public static void Riposte(Vector3 at, float radius, int damage)
        {
            Charge = Mathf.Min(MaxCharge, Charge + 20f);
            Strike(at, radius, damage);
            Pulse(at, radius, new Color(0.95f, 0.75f, 0.35f));
        }

        public static void TryCast()
        {
            var hero = HeroController.instance;
            if (hero == null || !Ready || hero.cState.dead || hero.cState.transitioning) return;

            Charge = 0f;
            HexesCast++;
            var at = hero.transform.position;
            var hitCount = Strike(at, HexRadius, HexDamage);
            Pulse(at, HexRadius, new Color(0.8f, 0.85f, 1f));
            GameCameras.instance?.cameraShakeFSM?.SendEvent("AverageShake");
            Barks.Say("Hex Moth", hitCount > 0 ? "By moonlight, be undone!" : "...the moon hears no one here.", Barks.MothColour);
        }

        private static int Strike(Vector3 at, float radius, int damage)
        {
            var count = 0;
            foreach (var hm in Object.FindObjectsOfType<HealthManager>())
            {
                if (hm == null || hm.GetIsDead() || !hm.gameObject.activeInHierarchy) continue;
                if ((hm.transform.position - at).sqrMagnitude > radius * radius) continue;

                var dir = hm.transform.position.x >= at.x ? 0f : 180f;
                hm.Hit(new HitInstance
                {
                    Source = HeroController.instance.gameObject,
                    AttackType = AttackTypes.Spell,
                    DamageDealt = damage,
                    Direction = dir,
                    Multiplier = 1f,
                    MagnitudeMultiplier = 1f,
                    IgnoreInvulnerable = true,
                });
                count++;
            }
            return count;
        }

        // A ring of light that grows to the strike radius and fades.
        internal static void Pulse(Vector3 at, float radius, Color colour)
        {
            ring ??= MakeRing();
            var go = new GameObject("WitchKnight Pulse");
            go.transform.position = new Vector3(at.x, at.y, at.z - 0.5f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = ring;
            sr.color = colour;
            sr.sortingOrder = 50;
            Barks.Run(Grow(go, sr, radius, colour));
        }

        private static IEnumerator Grow(GameObject go, SpriteRenderer sr, float radius, Color colour)
        {
            const float duration = 0.45f;
            for (var t = 0f; t < duration; t += Time.deltaTime)
            {
                if (go == null) yield break;
                var k = t / duration;
                var size = Mathf.Lerp(0.3f, 1f, 1f - (1f - k) * (1f - k)) * radius * 2f;
                go.transform.localScale = new Vector3(size, size, 1f);
                sr.color = new Color(colour.r, colour.g, colour.b, 1f - k);
                yield return null;
            }
            if (go != null) Object.Destroy(go);
        }

        private static Sprite MakeRing()
        {
            const int n = 128;
            var tex = new Texture2D(n, n, TextureFormat.RGBA32, false);
            var px = new Color32[n * n];
            for (var y = 0; y < n; y++)
            {
                for (var x = 0; x < n; x++)
                {
                    var dx = (x + 0.5f) / n * 2f - 1f;
                    var dy = (y + 0.5f) / n * 2f - 1f;
                    var r = Mathf.Sqrt(dx * dx + dy * dy);
                    var edge = Mathf.Clamp01(1f - Mathf.Abs(r - 0.9f) / 0.08f);
                    var fill = r < 0.9f ? 0.12f * r : 0f;
                    var a = Mathf.Clamp01(edge + fill);
                    px[y * n + x] = new Color(1f, 1f, 1f, a);
                }
            }
            tex.SetPixels32(px);
            tex.Apply();
            // One unit wide, so localScale is the diameter in world units.
            return Sprite.Create(tex, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), n);
        }
    }
}
