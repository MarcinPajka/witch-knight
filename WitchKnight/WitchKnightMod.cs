using Modding;

namespace WitchKnight
{
    /// <summary>
    /// Entry point loaded by the Hollow Knight Modding API.
    /// </summary>
    public class WitchKnightMod : Mod
    {
        internal static WitchKnightMod? Instance { get; private set; }

        public WitchKnightMod() : base("Witch Knight") { }

        public override string GetVersion() => GetType().Assembly.GetName().Version.ToString();

        public override void Initialize()
        {
            Instance = this;
            ModHooks.HeroUpdateHook += OnHeroUpdate;
            Log("The moth witch stirs.");
        }

        // Placeholder for M1 systems (Moonlight meter, wing-dash, staff moveset).
        private void OnHeroUpdate()
        {
        }
    }
}
