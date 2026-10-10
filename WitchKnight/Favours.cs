using System.Collections.Generic;
using System.Linq;
using Modding;
using UnityEngine;

namespace WitchKnight
{
    internal enum Patron
    {
        PaleTide,
        EmberChoir,
        SilkMatron,
        HollowBell,
        LastBloom,
    }

    /// <summary>
    /// The run's Favours. Every few kills the patrons offer three Favours; the player
    /// picks one with 1, 2 or 3. Favours stack per patron and are lost on death.
    /// </summary>
    internal static class Favours
    {
        public const int MaxLevel = 5;

        private static readonly Dictionary<Patron, int> levels = new();

        public static int KillsTowardNext { get; private set; }
        public static int KillsNeeded { get; private set; } = FirstKillsNeeded;
        public static List<Patron>? Offer { get; private set; }
        public static List<int>? OfferRarity { get; private set; }
        public static bool RerollUsed { get; private set; }

        private const int FirstKillsNeeded = 4;
        private const int KillsStep = 2;
        private const int MaxKillsNeeded = 12;

        private static readonly System.Random rng = new();

        public static int Level(Patron p) => levels.TryGetValue(p, out var l) ? l : 0;
        public static IEnumerable<KeyValuePair<Patron, int>> All => levels.Where(kv => kv.Value > 0);

        public static string Name(Patron p) => p switch
        {
            Patron.PaleTide => "The Pale Tide",
            Patron.EmberChoir => "The Ember Choir",
            Patron.SilkMatron => "The Silk Matron",
            Patron.HollowBell => "The Hollow Bell",
            _ => "The Last Bloom",
        };

        public static string FavourName(Patron p) => p switch
        {
            Patron.PaleTide => "Undertow",
            Patron.EmberChoir => "Cinder Edge",
            Patron.SilkMatron => "Silkstep",
            Patron.HollowBell => "Tolling Moon",
            _ => "Graveblossom",
        };

        public static string Describe(Patron p, int level) => p switch
        {
            Patron.PaleTide => $"Thrusts knock foes {40 * level}% further; {Mathf.Min(10 * level, 40)}% chance a blow washes off you",
            Patron.EmberChoir => $"Spear +{20 * level}% damage, spells +{25 * level}%; hits burn for {3 * level}",
            Patron.SilkMatron => $"Run {8 * level}% faster; glide falls more gently",
            Patron.HollowBell => $"Moonlight returns {50 * level}% faster; every 6th hit tolls for {4 * level} around the foe",
            _ => $"{Mathf.Min(12 * level, 60)}% chance a kill mends one mask",
        };

        public static Color Colour(Patron p) => p switch
        {
            Patron.PaleTide => new Color(0.55f, 0.85f, 1f),
            Patron.EmberChoir => new Color(1f, 0.55f, 0.25f),
            Patron.SilkMatron => new Color(0.92f, 0.86f, 0.7f),
            Patron.HollowBell => new Color(0.75f, 0.65f, 1f),
            _ => new Color(0.6f, 1f, 0.6f),
        };

        private static readonly Dictionary<Patron, string[]> greetings = new()
        {
            [Patron.PaleTide] = new[] { "The tide comes in, little moth. Take what it brings.", "Let them break on you like waves on stone." },
            [Patron.EmberChoir] = new[] { "Sing, spear. Sing in fire.", "We burn for you tonight, moth. Do not waste it." },
            [Patron.SilkMatron] = new[] { "Light feet, small one. The web remembers.", "Step where they cannot follow." },
            [Patron.HollowBell] = new[] { "Hear it? The moon rings for you.", "Each toll a little more light." },
            [Patron.LastBloom] = new[] { "What falls feeds the garden.", "Even down here, something blooms." },
        };

        public static void Hook()
        {
            ModHooks.RecordKillForJournalHook += (_, _, _, _, _) => OnKill();
            ModHooks.HitInstanceHook += OnHit;
            ModHooks.AfterTakeDamageHook += OnTakeDamage;
            ModHooks.AfterPlayerDeadHook += OnDeath;
            ModHooks.HeroUpdateHook += OnHeroUpdate;
        }

        private static void OnKill()
        {
            var bloom = Level(Patron.LastBloom);
            var hero = HeroController.instance;
            if (bloom > 0 && hero != null && Random.value < Mathf.Min(0.12f * bloom, 0.6f)
                && PlayerData.instance.GetInt("health") < PlayerData.instance.GetInt("maxHealth"))
            {
                hero.AddHealth(1);
                Barks.Say(Name(Patron.LastBloom), "A mask, mended.", Colour(Patron.LastBloom));
            }

            if (Offer != null) return;

            KillsTowardNext++;
            if (KillsTowardNext < KillsNeeded) return;

            KillsTowardNext = 0;
            KillsNeeded = Mathf.Min(KillsNeeded + KillsStep, MaxKillsNeeded);
            MakeOffer();
        }

        private static void MakeOffer()
        {
            var open = System.Enum.GetValues(typeof(Patron)).Cast<Patron>().Where(p => Level(p) < MaxLevel).ToList();
            if (open.Count == 0) return;

            Offer = open.OrderBy(_ => rng.Next()).Take(3).ToList();
            OfferRarity = Offer.Select(p => Mathf.Min(RollRarity(), MaxLevel - Level(p))).ToList();
            Barks.Say("Hex Moth", RerollUsed ? "The patrons are watching. Choose: 1, 2 or 3." : "The patrons are watching. Choose 1, 2 or 3, or R to redraw.", Barks.MothColour);
        }

        public static void Choose(int index)
        {
            if (Offer == null || index < 0 || index >= Offer.Count) return;

            var p = Offer[index];
            var gain = OfferRarity?[index] ?? 1;
            levels[p] = Mathf.Min(Level(p) + gain, MaxLevel);
            Offer = null;
            OfferRarity = null;
            Hex.FavoursTaken++;

            var lines = greetings[p];
            Barks.Say(Name(p), lines[rng.Next(lines.Length)], Colour(p));
            WitchKnightMod.Instance?.Log($"Favour taken: {FavourName(p)} {Level(p)}");
        }

        // 1 = Common, 2 = Rare (+2 levels), 3 = Heroic (+3 levels).
        private static int RollRarity()
        {
            var r = rng.NextDouble();
            return r < 0.06 ? 3 : r < 0.24 ? 2 : 1;
        }

        public static string RarityName(int r) => r switch { 3 => "Heroic", 2 => "Rare", _ => "Common" };

        public static Color RarityColour(int r) => r switch
        {
            3 => new Color(1f, 0.45f, 0.35f),
            2 => new Color(0.45f, 0.7f, 1f),
            _ => new Color(0.8f, 0.8f, 0.8f),
        };

        // Fate's Thread: once per run, press R to redraw an offer.
        public static void Reroll()
        {
            if (Offer == null || RerollUsed) return;
            RerollUsed = true;
            Offer = null;
            MakeOffer();
            Barks.Say("Hex Moth", "Fate, spin again.", Barks.MothColour);
        }

        public static void Reset()
        {
            levels.Clear();
            Offer = null;
            OfferRarity = null;
            RerollUsed = false;
            KillsTowardNext = 0;
            KillsNeeded = FirstKillsNeeded;
        }

        private static HitInstance OnHit(HutongGames.PlayMaker.Fsm owner, HitInstance hit)
        {
            var ember = Level(Patron.EmberChoir);
            var tide = Level(Patron.PaleTide);

            if (hit.AttackType == AttackTypes.Nail)
            {
                if (ember > 0) hit.DamageDealt = Mathf.RoundToInt(hit.DamageDealt * (1f + 0.2f * ember));
                if (tide > 0) hit.MagnitudeMultiplier *= 1f + 0.4f * tide;
            }
            else if (hit.AttackType == AttackTypes.Spell && ember > 0)
            {
                hit.DamageDealt = Mathf.RoundToInt(hit.DamageDealt * (1f + 0.25f * ember));
            }
            return hit;
        }

        private static int OnTakeDamage(int hazardType, int damage)
        {
            // hazardType 1 is an ordinary enemy hit; spikes and acid always land.
            var tide = Level(Patron.PaleTide);
            if (damage > 0 && hazardType == 1 && tide > 0 && Random.value < Mathf.Min(0.1f * tide, 0.4f))
            {
                Barks.Say(Name(Patron.PaleTide), "Washed away.", Colour(Patron.PaleTide));
                return 0;
            }
            if (damage > 0) Barks.OnHurt();
            return damage;
        }

        private static void OnDeath()
        {
            var had = levels.Values.Sum();
            Barks.Say("Run", Hex.Tally(), new Color(0.85f, 0.85f, 0.85f));
            Reset();
            Hex.ResetRun();
            Barks.Say("Hex Moth", had > 0 ? "The Favours fade... I will climb again." : "Again. From the start.", Barks.MothColour);
        }

        // Silk Matron: run speed, applied against the hero's own base speed.
        private static float baseRun = -1f;

        private static void OnHeroUpdate()
        {
            var hero = HeroController.instance;
            if (hero == null) return;
            if (baseRun < 0f) baseRun = hero.RUN_SPEED;
            hero.RUN_SPEED = baseRun * (1f + 0.08f * Level(Patron.SilkMatron));
        }

        public static float MoonlightRate => 1f + 0.5f * Level(Patron.HollowBell);
        public static float GlideFallSpeed(float baseSpeed) => baseSpeed * Mathf.Pow(0.85f, Level(Patron.SilkMatron));
    }
}
