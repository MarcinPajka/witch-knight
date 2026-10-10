using GlobalEnums;
using Modding;
using UnityEngine;

namespace WitchKnight
{
    /// <summary>
    /// Hex Moth's M1 moveset on top of the Knight's controls:
    /// a longer spear reach, Moonlight that slowly recharges, and a wing-glide.
    /// </summary>
    internal static class Moveset
    {
        // Spear thrusts reach further than the nail; up and down thrusts a little less so.
        private const float SideReach = 1.35f;
        private const float VerticalReach = 1.2f;

        // Moonlight (Soul) trickles back on its own: this much every interval while not full.
        private const int MoonlightPerTick = 3;
        private const float MoonlightInterval = 1.5f;

        // Holding jump while falling opens the wing-cloak and caps the fall speed.
        private const float GlideFallSpeed = 4f;

        private static float moonlightTimer;

        public static void Hook()
        {
            ModHooks.AfterAttackHook += OnAfterAttack;
            ModHooks.HeroUpdateHook += OnHeroUpdate;
        }

        public static void Unhook()
        {
            ModHooks.AfterAttackHook -= OnAfterAttack;
            ModHooks.HeroUpdateHook -= OnHeroUpdate;
        }

        private static void OnAfterAttack(AttackDirection dir)
        {
            var hero = HeroController.instance;
            if (hero == null) return;

            // Attack() has just called StartSlash on this component, which reset its scale,
            // so stretching it here applies once per swing.
            var slash = ReflectionHelper.GetField<HeroController, NailSlash>(hero, "slashComponent");
            if (slash == null) return;

            var t = slash.transform;
            var s = t.localScale;
            t.localScale = dir == AttackDirection.normal
                ? new Vector3(s.x * SideReach, s.y, s.z)
                : new Vector3(s.x * VerticalReach, s.y * VerticalReach, s.z);
        }

        private static void OnHeroUpdate()
        {
            var hero = HeroController.instance;
            if (hero == null || hero.cState.dead || hero.cState.transitioning) return;

            RegenerateMoonlight(hero);
            Glide(hero);
        }

        private static void RegenerateMoonlight(HeroController hero)
        {
            var pd = PlayerData.instance;
            if (pd.GetInt("MPCharge") >= pd.GetInt("maxMP"))
            {
                moonlightTimer = 0f;
                return;
            }

            moonlightTimer += Time.deltaTime;
            if (moonlightTimer < MoonlightInterval) return;
            moonlightTimer = 0f;

            hero.AddMPCharge(MoonlightPerTick);
            GameCameras.instance?.soulOrbFSM?.SendEvent("MP GAIN");
        }

        private static void Glide(HeroController hero)
        {
            var cs = hero.cState;
            if (cs.onGround || cs.dashing || cs.downAttacking || cs.swimming || cs.wallSliding) return;
            if (!InputHandler.Instance.inputActions.jump.IsPressed) return;

            var body = hero.GetComponent<Rigidbody2D>();
            if (body.velocity.y < -GlideFallSpeed)
            {
                body.velocity = new Vector2(body.velocity.x, -GlideFallSpeed);
            }
        }
    }
}
