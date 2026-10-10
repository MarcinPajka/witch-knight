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

        // Swap the Knight for Hex Moth as soon as the hero exists. Later M1 systems
        // (Moonlight meter, wing-dash, spear moveset) hook in here too.
        private void OnHeroUpdate()
        {
            var hero = HeroController.instance;
            if (hero != null && hero.GetComponent<HexMothSkin>() == null)
            {
                hero.gameObject.AddComponent<HexMothSkin>();
            }
        }
    }
}
