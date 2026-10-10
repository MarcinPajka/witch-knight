using UnityEngine;

namespace WitchKnight
{
    internal enum RouteKind { None, Descent, Ascent }

    /// <summary>
    /// Each run starts with a choice of route. Descent goes down into the deep: foes are
    /// tougher but the patrons come sooner. Ascent climbs toward the moon: blows hurt twice
    /// as much, but the moon fills twice as fast and Favours are rarer and grander.
    /// </summary>
    internal static class Route
    {
        public static RouteKind Current { get; private set; } = RouteKind.None;
        public static bool Choosing => Current == RouteKind.None;

        public const float DescentToughness = 1.4f;

        public static void Choose(RouteKind r)
        {
            Current = r;
            Barks.Say("Hex Moth", r == RouteKind.Descent
                ? "Down, then. Into the dark where the old things sleep."
                : "Up. Toward the moon, whatever waits there.", Barks.MothColour);
        }

        public static void Reset() => Current = RouteKind.None;

        public static string Name(RouteKind r) => r == RouteKind.Descent ? "Descent" : "Ascent";

        public static string Describe(RouteKind r) => r == RouteKind.Descent
            ? "Down into the deep. Foes are 40% tougher; Favours come one kill sooner."
            : "Up toward the moon. Blows hurt twice as much; the moon fills twice as fast and Favours roll rarer.";

        public static Color Colour(RouteKind r) => r == RouteKind.Descent
            ? new Color(0.6f, 0.5f, 0.9f)
            : new Color(0.95f, 0.9f, 0.6f);
    }
}
