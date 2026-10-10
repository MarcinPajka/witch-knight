using UnityEngine;

namespace WitchKnight
{
    /// <summary>
    /// Bronze parry: raise the shield for a moment. An enemy hit that lands while it is up
    /// is turned aside, staggers foes close by and feeds the moon.
    /// </summary>
    internal static class Parry
    {
        private const float Window = 0.35f;
        private const float Cooldown = 2f;
        private const float RiposteRadius = 3f;
        private const int RiposteDamage = 12;

        private static float raisedAt = -999f;

        public static bool Active => Time.time - raisedAt < Window;
        public static bool ShowPose => Time.time - raisedAt < Window + 0.1f;

        public static void TryRaise()
        {
            var hero = HeroController.instance;
            if (hero == null || hero.cState.dead || hero.cState.transitioning) return;
            if (Time.time - raisedAt < Cooldown) return;
            raisedAt = Time.time;
        }

        // Called from the damage hook; true when the hit was parried.
        public static bool TryTurnAside()
        {
            if (!Active) return false;
            // Close the window, but leave only a short wait before she can raise it again.
            raisedAt = Time.time - Cooldown + 0.4f;

            var hero = HeroController.instance;
            Hex.Riposte(hero.transform.position, RiposteRadius, RiposteDamage);
            Barks.Say("Hex Moth", Random.value < 0.5f ? "Bronze holds!" : "Turned aside!", Barks.MothColour);
            return true;
        }
    }
}
