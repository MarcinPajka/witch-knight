using System;
using System.Collections;
using System.Reflection;
using Modding;
using UnityEngine;

namespace WitchKnight
{
    /// <summary>One reskinned creature: which game objects it covers and how it maps their animation clips.</summary>
    internal sealed class Creature
    {
        public Creature(string key, float height, Func<string, string> choose, params string[] objectNames)
        {
            Key = key;
            Height = height;
            Choose = choose;
            ObjectNames = objectNames;
        }

        public string Key { get; }

        /// <summary>Figure height in game units. Hex Moth is 1.95.</summary>
        public float Height { get; }

        /// <summary>Maps the enemy's current tk2d clip name to one of our animations.</summary>
        public Func<string, string> Choose { get; }

        /// <summary>Game object names this creature replaces, matched as prefixes.</summary>
        public string[] ObjectNames { get; }
    }

    /// <summary>
    /// Puts the Greek bestiary art over Hallownest's enemies as they spawn, and over their corpses
    /// when they die.
    /// </summary>
    internal static class EnemySkins
    {
        private static bool Has(string clip, params string[] parts)
        {
            foreach (var p in parts) if (clip.IndexOf(p, StringComparison.OrdinalIgnoreCase) >= 0) return true;
            return false;
        }

        private static readonly Creature[] creatures =
        {
            new("hopliteHusk", 2.0f, clip =>
                Has(clip, "Death", "Dead") ? "death"
                : Has(clip, "Antic") ? "attackAntic"
                : Has(clip, "Attack", "Lunge", "Leap", "Jump", "Slash") ? "attack"
                : Has(clip, "Run", "Charge", "Barge") ? "run"
                : Has(clip, "Walk", "Turn") ? "walk"
                : "idle",
                "Zombie Runner", "Zombie Hornhead", "Zombie Leaper", "Zombie Barger"),

            new("harpyFly", 1.3f, clip =>
                Has(clip, "Death", "Dead") ? "death"
                : Has(clip, "Startle", "Chase", "Attack", "Charge") ? "attack"
                : "fly",
                "Buzzer"),

            new("bronzeCrawler", 0.8f, clip =>
                Has(clip, "Death", "Dead") ? "death" : "walk",
                "Crawler", "Climber"),

            new("minotaur", 5.0f, clip =>
                Has(clip, "Death", "Dead") ? "death"
                : Has(clip, "Stun", "Open", "Head", "Hit") ? "stun"
                : Has(clip, "Roar") ? "roar"
                : Has(clip, "Jump") && Has(clip, "Antic") ? "jumpAntic"
                : Has(clip, "Jump") ? "jump"
                : Has(clip, "Land") ? "land"
                : Has(clip, "Recover") ? "attackRecover"
                : Has(clip, "Antic") ? "attackAntic"
                : Has(clip, "Attack", "Slam", "Smash") ? "attack"
                : Has(clip, "Run", "Walk", "Turn") ? "run"
                : "idle",
                "False Knight"),
        };

        public static void Hook()
        {
            ModHooks.OnEnableEnemyHook += OnEnableEnemy;
        }

        private static bool OnEnableEnemy(GameObject enemy, bool isAlreadyDead)
        {
            if (WitchKnightMod.Settings.GreekEnemyArt && !isAlreadyDead && enemy.GetComponent<EnemySkin>() == null)
            {
                var creature = Find(enemy.name);
                if (creature != null && enemy.GetComponent<tk2dSprite>() != null)
                {
                    enemy.AddComponent<EnemySkin>().Init(creature, corpse: false);
                }
            }

            return isAlreadyDead;
        }

        private static Creature? Find(string objectName)
        {
            foreach (var c in creatures)
            {
                foreach (var n in c.ObjectNames)
                {
                    if (objectName.StartsWith(n, StringComparison.Ordinal)) return c;
                }
            }

            return null;
        }

        private static readonly string[] corpseFields = { "preInstantiatedCorpse", "corpse" };

        /// <summary>Called when a skinned enemy dies; skins the corpse the game drops in its place.</summary>
        internal static void SkinCorpse(GameObject enemy, Creature creature)
        {
            var effects = enemy.GetComponent<EnemyDeathEffects>();
            GameObject? corpse = null;
            if (effects != null)
            {
                foreach (var name in corpseFields)
                {
                    var field = typeof(EnemyDeathEffects).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                    if (field?.GetValue(effects) is GameObject go) { corpse = go; break; }
                }
            }

            if (corpse != null)
            {
                Attach(corpse, creature);
            }
            else if (GameManager.instance != null)
            {
                // No pre-made corpse to grab: look for the one the game spawns at the death spot.
                GameManager.instance.StartCoroutine(FindCorpse(enemy.transform.position, creature));
            }
        }

        private static IEnumerator FindCorpse(Vector3 at, Creature creature)
        {
            for (int tries = 0; tries < 3; tries++)
            {
                yield return null;
                foreach (var sprite in UnityEngine.Object.FindObjectsOfType<tk2dSprite>())
                {
                    var go = sprite.gameObject;
                    if (go.name.StartsWith("Corpse", StringComparison.Ordinal) && go.GetComponent<EnemySkin>() == null
                        && (go.transform.position - at).sqrMagnitude < 9f)
                    {
                        Attach(go, creature);
                        yield break;
                    }
                }
            }
        }

        private static void Attach(GameObject corpse, Creature creature)
        {
            if (corpse.GetComponent<EnemySkin>() == null && corpse.GetComponent<tk2dSprite>() != null)
            {
                corpse.AddComponent<EnemySkin>().Init(creature, corpse: true);
            }
        }
    }

    /// <summary>
    /// Hides an enemy's own sprite and draws our creature in its place, following the enemy's
    /// animation clips. On a corpse it plays the death animation once and holds the last frame.
    /// </summary>
    internal sealed class EnemySkin : MonoBehaviour
    {
        private const float HurtTime = 0.2f;

        private Creature creature = null!;
        private bool isCorpse;
        private CreatureSheets sheets = null!;
        private tk2dSprite original = null!;
        private tk2dSpriteAnimator? animator;
        private MeshRenderer originalRenderer = null!;
        private HealthManager? health;
        private SpriteRenderer sprite = null!;

        private string current = "";
        private float time;
        private int lastHp;
        private float hurt;
        private bool ready;

        public void Init(Creature c, bool corpse)
        {
            creature = c;
            isCorpse = corpse;
            original = GetComponent<tk2dSprite>();
            animator = GetComponent<tk2dSpriteAnimator>();
            originalRenderer = GetComponent<MeshRenderer>();
            health = GetComponent<HealthManager>();
            lastHp = health != null ? health.hp : 0;

            try
            {
                sheets = EnemySheets.Get(c.Key, c.Height);
            }
            catch (Exception e)
            {
                WitchKnightMod.Instance?.LogError($"Could not load sprites for {c.Key}: {e}");
                Destroy(this);
                return;
            }

            var go = new GameObject("Witch Knight " + c.Key);
            go.transform.SetParent(transform, false);
            go.layer = gameObject.layer;
            sprite = go.AddComponent<SpriteRenderer>();
            Place(go.transform);

            Play(isCorpse ? "death" : creature.Choose(""));
            ready = true;
        }

        // Feet on the bottom of the collider, or the body's middle for fliers, at a fixed world size.
        private void Place(Transform t)
        {
            var s = transform.lossyScale;
            t.localScale = new Vector3(1f / Mathf.Max(0.01f, Mathf.Abs(s.x)), 1f / Mathf.Max(0.01f, Mathf.Abs(s.y)), 1f);

            Vector2 centre, bottom;
            var box = GetComponent<BoxCollider2D>();
            if (box != null)
            {
                centre = box.offset;
                bottom = new Vector2(box.offset.x, box.offset.y - box.size.y / 2f);
            }
            else
            {
                var col = GetComponent<Collider2D>();
                if (col != null)
                {
                    var b = col.bounds;
                    centre = transform.InverseTransformPoint(b.center);
                    bottom = transform.InverseTransformPoint(new Vector3(b.center.x, b.min.y, b.center.z));
                }
                else
                {
                    var b = original.GetBounds();
                    centre = b.center;
                    bottom = new Vector2(b.center.x, b.min.y);
                }
            }

            var at = sheets.Centered ? centre : bottom;
            t.localPosition = new Vector3(at.x, at.y, -0.001f);
        }

        private void LateUpdate()
        {
            if (!ready) return;

            // Keep the original invisible; the game resets its colour during flashes and fades.
            if (original.color.a > 0f)
            {
                var c = original.color;
                original.color = new Color(c.r, c.g, c.b, 0f);
            }

            sprite.enabled = originalRenderer.enabled;
            sprite.sortingLayerID = originalRenderer.sortingLayerID;
            sprite.sortingOrder = originalRenderer.sortingOrder;

            // Hallownest's enemy sprites face left; ours face right.
            sprite.flipX = !original.FlipX;

            if (!isCorpse)
            {
                if (health != null)
                {
                    if (health.hp < lastHp) hurt = HurtTime;
                    lastHp = health.hp;
                }

                hurt -= Time.deltaTime;
                string next = creature.Choose(animator?.CurrentClip?.name ?? "");
                if (hurt > 0f && next != "death" && next != "stun" && sheets.Anims.ContainsKey("hurt")) next = "hurt";
                if (next != current) Play(next);
            }

            Step(Time.deltaTime);
        }

        private void OnDisable()
        {
            if (ready && !isCorpse && health != null && health.GetIsDead())
            {
                EnemySkins.SkinCorpse(gameObject, creature);
            }
        }

        private void Play(string name)
        {
            if (!sheets.Anims.ContainsKey(name)) name = sheets.Anims.ContainsKey("idle") ? "idle" : FirstAnim();
            current = name;
            time = 0f;
            sprite.sprite = sheets.Anims[name].Frames[0];
        }

        private string FirstAnim()
        {
            foreach (var k in sheets.Anims.Keys) return k;
            return "";
        }

        private void Step(float dt)
        {
            var a = sheets.Anims[current];
            time += dt;
            int i = (int)(time * a.Fps);
            i = a.Loop ? i % a.Frames.Length : Mathf.Min(i, a.Frames.Length - 1);
            sprite.sprite = a.Frames[i];
        }
    }
}
