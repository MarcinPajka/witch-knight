using System;
using System.Collections.Generic;
using Modding;

namespace WitchKnight
{
    /// <summary>
    /// Renames creatures to their Greek-myth selves wherever the game shows the name
    /// (Hunter's Journal, boss title cards). Matches on the English text, whole strings only.
    /// </summary>
    internal static class Bestiary
    {
        private static readonly Dictionary<string, string> names = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Wandering Husk"] = "Hoplite Husk",
            ["Husk Hornhead"] = "Hoplite Husk Captain",
            ["Husk Guard"] = "Phalanx Guard",
            ["Husk Sentry"] = "Phalanx Guard",
            ["Heavy Sentry"] = "Phalanx Shieldbearer",
            ["Great Husk Sentry"] = "Spartan Champion",
            ["Vengefly"] = "Harpy Fly",
            ["Vengefly King"] = "Harpy Queen",
            ["Gruzzer"] = "Stymphalian Chick",
            ["Gruz Mother"] = "Stymphalian Mother",
            ["Aspid Hunter"] = "Hydra Hatchling",
            ["Primal Aspid"] = "Elder Hydra Hatchling",
            ["Crawlid"] = "Bronze Crawler",
            ["Tiktik"] = "Bronze Ticker",
            ["Mosscreep"] = "Dryad Sprout",
            ["Mosskin"] = "Dryad Kin",
            ["Mistake"] = "Shade",
            ["Folly"] = "Wandering Shade",
            ["Crystal Hunter"] = "Cyclops Flier",
            ["Crystal Crawler"] = "Cyclops Miner",
            ["Weaver"] = "Arachne's Weaver",
            ["Hiveling"] = "Melissa Larva",
            ["Hive Soldier"] = "Melissa Soldier",
            ["False Knight"] = "The Minotaur",
            ["Failed Champion"] = "The Fallen Minotaur",
            ["Brooding Mawlek"] = "Chimera",
            ["Hornet"] = "The Amazon",
            ["Mantis Lords"] = "The Gorgon Sisters",
            ["Soul Master"] = "The Fallen Oracle",
            ["Dung Defender"] = "The Augean Keeper",
            ["Broken Vessel"] = "The Hydra",
            ["Crystal Guardian"] = "Talos",
            ["Watcher Knight"] = "Argus's Eye-Knight",
            ["Watcher Knights"] = "Argus Hundred-Eyes",
            ["Uumuu"] = "Scylla",
            ["Nosk"] = "Empusa",
            ["Hive Knight"] = "The Hive Warden",
            ["Traitor Lord"] = "Lycaon",
        };

        public static void Hook()
        {
            ModHooks.LanguageGetHook += OnLanguageGet;
        }

        private static string OnLanguageGet(string key, string sheet, string orig)
        {
            if (orig == null || orig.Length > 24 || !WitchKnightMod.Settings.GreekNames) return orig!;
            return names.TryGetValue(orig.Trim(), out var greek) ? greek : orig;
        }
    }
}
