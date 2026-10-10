using System;
using System.Collections;
using GlobalEnums;
using UnityEngine;

namespace WitchKnight
{
    /// <summary>
    /// Gives the Greek bosses a move of their own on top of their usual attacks.
    /// Each twist is a component added to the boss when its HealthManager starts, so the
    /// boss's own fight runs untouched underneath and the twist dies with it.
    /// </summary>
    internal static class BossTwists
    {
        public static void Hook()
        {
            On.HealthManager.Start += OnHealthManagerStart;
        }

        private static void OnHealthManagerStart(On.HealthManager.orig_Start orig, HealthManager self)
        {
            orig(self);
            if (!WitchKnightMod.Settings.BossTwists) return;

            var name = self.gameObject.name;
            if (name.StartsWith("False Knight")) Add<MinotaurCharge>(self);
            else if (name.StartsWith("Mantis Lord")) Add<GorgonGaze>(self);
            else if (name.StartsWith("Hornet Boss")) Add<AmazonDisc>(self);
            else if (name.StartsWith("Giant Fly")) Add<StymphalianFeathers>(self);
        }

        private static void Add<T>(HealthManager hm) where T : BossTwist
        {
            if (hm.GetComponent<T>() == null) hm.gameObject.AddComponent<T>();
        }

        /// <summary>Hurts Hex Moth if <paramref name="at"/> is within <paramref name="radius"/> of her body.
        /// Uses hazard type 1, so her bronze parry can turn it aside.</summary>
        public static bool HitHero(GameObject source, Vector3 at, float radius, int damage)
        {
            var hero = HeroController.instance;
            if (hero == null || hero.cState.dead || hero.cState.transitioning) return false;

            var box = hero.GetComponent<Collider2D>();
            var near = box != null
                ? box.bounds.SqrDistance(new Vector3(at.x, at.y, box.bounds.center.z)) <= radius * radius
                : ((Vector2)(hero.transform.position - at)).sqrMagnitude <= radius * radius;
            if (!near) return false;

            var side = at.x >= hero.transform.position.x ? CollisionSide.right : CollisionSide.left;
            hero.TakeDamage(source, side, damage, 1);
            return true;
        }

        // ---- Procedural sprites for the twists. One world unit per sprite unless scaled. ----

        private static Sprite? disc, horn, feather, eye;

        public static Sprite Disc => disc ??= Make(64, new Vector2(0.5f, 0.5f), (x, y) =>
        {
            var r = Mathf.Sqrt(x * x + y * y);
            if (r > 0.95f) return Color.clear;
            if (r < 0.28f) return Color.clear;
            // A bronze ring with a bright rim and notched teeth.
            var teeth = Mathf.Repeat(Mathf.Atan2(y, x) * 8f / Mathf.PI, 2f) < 1f && r > 0.85f;
            if (teeth) return Color.clear;
            var rim = r > 0.75f || r < 0.36f;
            return rim ? new Color(1f, 0.85f, 0.5f) : new Color(0.75f, 0.5f, 0.22f);
        });

        // Points right; flip the x scale to point left.
        public static Sprite Horn => horn ??= Make(64, new Vector2(0f, 0.5f), (x, y) =>
        {
            var u = (x + 1f) / 2f;                      // 0 at the base, 1 at the tip
            var curve = 0.35f * u * u;                  // tip sweeps upward
            var half = 0.55f * (1f - u);
            var d = Mathf.Abs(y - curve);
            if (d > half) return Color.clear;
            return d > half - 0.12f ? new Color(0.35f, 0.22f, 0.1f) : new Color(0.93f, 0.86f, 0.7f);
        });

        // Points down.
        public static Sprite Feather => feather ??= Make(48, new Vector2(0.5f, 0.5f), (x, y) =>
        {
            var u = (1f - y) / 2f;                      // 0 at the top, 1 at the tip
            var half = 0.32f * Mathf.Sin(Mathf.PI * Mathf.Min(1f, u * 1.15f));
            if (Mathf.Abs(x) > half) return Color.clear;
            if (Mathf.Abs(x) < 0.05f) return new Color(1f, 0.9f, 0.6f);
            return new Color(0.8f, 0.55f, 0.25f);
        });

        public static Sprite Eye => eye ??= Make(64, new Vector2(0.5f, 0.5f), (x, y) =>
        {
            // An almond eye with a slit pupil.
            var lid = 0.55f * (1f - x * x);
            if (Mathf.Abs(y) > lid) return Color.clear;
            if (Mathf.Abs(y) > lid - 0.1f) return new Color(0.2f, 0.35f, 0.25f);
            var r = Mathf.Sqrt(x * x + y * y);
            if (r < 0.3f) return Mathf.Abs(x) < 0.07f ? Color.black : new Color(0.55f, 0.9f, 0.45f);
            return new Color(0.95f, 0.95f, 0.85f);
        });

        public static SpriteRenderer Spawn(string name, Sprite sprite, Vector3 at, float size)
        {
            var go = new GameObject("WitchKnight " + name);
            go.transform.position = new Vector3(at.x, at.y, at.z - 0.3f);
            go.transform.localScale = new Vector3(size, size, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 40;
            return sr;
        }

        // x and y run from -1 to 1 across the texture.
        private static Sprite Make(int n, Vector2 pivot, Func<float, float, Color> paint)
        {
            var tex = new Texture2D(n, n, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
            var px = new Color32[n * n];
            for (var j = 0; j < n; j++)
            {
                for (var i = 0; i < n; i++)
                {
                    px[j * n + i] = paint((i + 0.5f) / n * 2f - 1f, (j + 0.5f) / n * 2f - 1f);
                }
            }
            tex.SetPixels32(px);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, n, n), pivot, n);
        }
    }

    /// <summary>
    /// Shared plumbing for a boss twist: the boss's health, and whether the fight is on.
    /// A twist starts only once Hex Moth has drawn blood, so sleeping and
    /// not-yet-arrived bosses stay quiet.
    /// </summary>
    internal abstract class BossTwist : MonoBehaviour
    {
        // Boss control states during which a twist would look wrong.
        private static readonly string[] busyStates =
            { "Stun", "Death", "Dead", "Intro", "Sleep", "Wake", "Roar", "Init", "Dormant", "Pause", "Open" };

        protected HealthManager Health = null!;
        protected Rigidbody2D? Body;
        protected Collider2D? Box;
        private int maxHp;
        private PlayMakerFSM[] fsms = null!;

        protected float Health01 => maxHp > 0 ? (float)Health.hp / maxHp : 1f;
        protected bool Enraged => Health01 <= 0.5f;

        protected virtual void Awake()
        {
            Health = GetComponent<HealthManager>();
            Body = GetComponent<Rigidbody2D>();
            Box = GetComponent<Collider2D>();
            fsms = GetComponents<PlayMakerFSM>();
            maxHp = Health.hp;
        }

        protected virtual void OnEnable() => StartCoroutine(Loop());

        protected abstract IEnumerator Loop();

        protected Vector3 Centre => Box != null ? Box.bounds.center : transform.position;

        /// <summary>True when the boss is awake, hurt, in reach of Hex Moth and not mid-stagger.</summary>
        protected bool CanAct(float reach = 22f)
        {
            var hero = HeroController.instance;
            if (hero == null || hero.cState.dead || hero.cState.transitioning) return false;
            if (Health == null || Health.GetIsDead()) return false;
            if (Health.hp > maxHp) maxHp = Health.hp;
            if (Health.hp >= maxHp) return false;
            if ((hero.transform.position - Centre).sqrMagnitude > reach * reach) return false;

            foreach (var fsm in fsms)
            {
                var state = fsm != null ? fsm.ActiveStateName : null;
                if (state == null) continue;
                foreach (var busy in busyStates)
                {
                    if (state.IndexOf(busy, StringComparison.OrdinalIgnoreCase) >= 0) return false;
                }
            }
            return true;
        }
    }
}
