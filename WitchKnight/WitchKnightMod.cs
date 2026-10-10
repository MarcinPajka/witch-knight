using Modding;

namespace WitchKnight
{
    /// <summary>
    /// Entry point loaded by the Hollow Knight Modding API.
    /// </summary>
    public class WitchKnightMod : Mod, IGlobalSettings<GlobalSettings>
    {
        public static GlobalSettings Settings { get; private set; } = new();

        public void OnLoadGlobal(GlobalSettings s) => Settings = s;
        public GlobalSettings OnSaveGlobal() => Settings;

        internal static WitchKnightMod? Instance { get; private set; }

        public WitchKnightMod() : base("Witch Knight") { }

        public override string GetVersion() => GetType().Assembly.GetName().Version.ToString();

        public override void Initialize()
        {
            Instance = this;
            ModHooks.HeroUpdateHook += OnHeroUpdate;
            Moveset.Hook();
            Favours.Hook();
            Barks.Hook();
            Hex.Hook();
            Bestiary.Hook();
            RunLoop.Hook();
            BossTwists.Hook();
            Log("The moth witch stirs.");
        }

        // Swap the Knight for Hex Moth as soon as the hero exists. The moveset lives in Moveset.
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
