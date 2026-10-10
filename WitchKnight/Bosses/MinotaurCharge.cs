using System.Collections;
using UnityEngine;

namespace WitchKnight
{
    /// <summary>
    /// The Minotaur lowers his horns and charges across the arena. Bronze horns jut from his
    /// leading edge for the charge and gore for two masks; a wall stops him with a crash.
    /// Below half health he turns and charges straight back.
    /// </summary>
    internal sealed class MinotaurCharge : BossTwist
    {
        private const float WindUp = 0.65f;
        private const float Speed = 24f;
        private const float MaxDuration = 1.1f;
        private const int GoreDamage = 2;

        private static readonly Color Colour = new(0.9f, 0.45f, 0.3f);
        private static readonly string[] lines =
        {
            "The Minotaur lowers his horns!",
            "The labyrinth's lord paws the ground...",
            "HRRAAAGH!",
        };

        private bool charging;
        private SpriteRenderer? horns;

        protected override IEnumerator Loop()
        {
            while (true)
            {
                yield return new WaitForSeconds(Random.Range(6f, 9f) * (Enraged ? 0.75f : 1f));
                while (!CanAct() || !Grounded()) yield return null;

                yield return Charge(towardHero: true);
                if (Enraged && CanAct())
                {
                    yield return new WaitForSeconds(0.35f);
                    yield return Charge(towardHero: false);
                }
            }
        }

        private IEnumerator Charge(bool towardHero)
        {
            var hero = HeroController.instance;
            if (hero == null || Body == null) yield break;

            float dir;
            if (towardHero) dir = hero.transform.position.x >= Centre.x ? 1f : -1f;
            else dir = Body.velocity.x >= 0f ? -1f : 1f;

            charging = true;
            horns = BossTwists.Spawn("Minotaur Horns", BossTwists.Horn, Centre, 1f);
            Barks.Say("The Minotaur", lines[Random.Range(0, lines.Length)], Colour);
            Hex.Pulse(Centre, 3.5f, Colour);

            // Wind-up: the horns grow out and he holds still.
            for (var t = 0f; t < WindUp; t += Time.deltaTime)
            {
                if (!Alive()) { Finish(); yield break; }
                Body.velocity = new Vector2(0f, Body.velocity.y);
                PlaceHorns(horns!, dir, Mathf.SmoothStep(0.3f, 1f, t / WindUp));
                yield return null;
            }

            var gored = false;
            var crashed = false;
            for (var t = 0f; t < MaxDuration; t += Time.fixedDeltaTime)
            {
                if (!Alive()) break;
                if (WallAhead(dir)) { crashed = true; break; }

                Body.velocity = new Vector2(dir * Speed, Body.velocity.y);
                var tip = PlaceHorns(horns!, dir, 1f);
                if (!gored) gored = BossTwists.HitHero(gameObject, tip, 1.1f, GoreDamage);
                yield return new WaitForFixedUpdate();
            }

            if (Body != null) Body.velocity = new Vector2(0f, Body.velocity.y);
            if (crashed)
            {
                GameCameras.instance?.cameraShakeFSM?.SendEvent("AverageShake");
                Hex.Pulse(Centre + new Vector3(dir * 1.5f, 0f), 2.5f, new Color(0.8f, 0.7f, 0.55f));
            }
            Finish();
        }

        // Puts the horns on his leading edge and returns the tip's position.
        private Vector3 PlaceHorns(SpriteRenderer horns, float dir, float grow)
        {
            var b = Box != null ? Box.bounds : new Bounds(transform.position, Vector3.one * 2f);
            var root = new Vector3(dir > 0f ? b.max.x - 0.3f : b.min.x + 0.3f, b.center.y + b.extents.y * 0.25f, b.center.z - 0.3f);
            var length = 2.4f * grow;
            horns.transform.position = root;
            horns.transform.localScale = new Vector3(dir * length, 1.6f * grow, 1f);
            return root + new Vector3(dir * length * 0.85f, 0.25f * grow, 0f);
        }

        private bool WallAhead(float dir)
        {
            if (Box == null) return false;
            var b = Box.bounds;
            var from = new Vector2(b.center.x, b.center.y);
            return Physics2D.Raycast(from, new Vector2(dir, 0f), b.extents.x + 0.7f, 1 << 8).collider != null;
        }

        private bool Grounded()
        {
            if (Box == null) return true;
            var b = Box.bounds;
            return Physics2D.Raycast(new Vector2(b.center.x, b.min.y + 0.1f), Vector2.down, 0.6f, 1 << 8).collider != null;
        }

        private bool Alive() => Health != null && !Health.GetIsDead() && HeroController.instance != null && !HeroController.instance.cState.dead;

        private void Finish()
        {
            charging = false;
            if (horns != null) Destroy(horns.gameObject);
            horns = null;
        }

        private void OnDisable()
        {
            if (charging && Body != null) Body.velocity = new Vector2(0f, Body.velocity.y);
            Finish();
        }
    }
}
