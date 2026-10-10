using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WitchKnight
{
    /// <summary>
    /// The Stymphalian Mother sheds bronze feathers that rain from the ceiling around
    /// Hex Moth. Each feather hangs for a breath before it drops, so the gaps can be read.
    /// Below half health she sheds more of them.
    /// </summary>
    internal sealed class StymphalianFeathers : BossTwist
    {
        private const float Hang = 0.6f;
        private const float MaxFall = 22f;
        private const float HitRadius = 0.45f;

        private static readonly Color Colour = new(0.85f, 0.65f, 0.3f);

        private readonly List<GameObject> feathers = new();

        protected override IEnumerator Loop()
        {
            while (true)
            {
                yield return new WaitForSeconds(Random.Range(5f, 7f) * (Enraged ? 0.8f : 1f));
                while (!CanAct(24f)) yield return null;

                if (Random.value < 0.5f) Barks.Say("Stymphalian Mother", "SKREEEE!", Colour);
                var hero = HeroController.instance;
                var count = Enraged ? 7 : 5;
                var gap = 8f / (count - 1);
                var start = hero.transform.position.x - 4f + Random.Range(-gap / 2f, gap / 2f);
                for (var i = 0; i < count; i++)
                {
                    StartCoroutine(Drop(new Vector2(start + i * gap, hero.transform.position.y), i * 0.08f));
                }
            }
        }

        private IEnumerator Drop(Vector2 column, float delay)
        {
            yield return new WaitForSeconds(delay);

            // Hang just below the ceiling, or high above her if there is none.
            var up = Physics2D.Raycast(column + Vector2.up, Vector2.up, 14f, 1 << 8);
            var y = up.collider != null ? up.point.y - 0.6f : column.y + 10f;

            var sr = BossTwists.Spawn("Stymphalian Feather", BossTwists.Feather, new Vector3(column.x, y, 0f), 1.4f);
            var feather = sr.transform;
            feathers.Add(feather.gameObject);

            for (var t = 0f; t < Hang; t += Time.deltaTime)
            {
                if (feather == null) yield break;
                sr.color = new Color(1f, 1f, 1f, Mathf.Clamp01(t / Hang * 1.5f));
                feather.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(t * 30f) * 8f);
                yield return null;
            }

            var speed = 4f;
            for (var t = 0f; t < 3f; t += Time.deltaTime)
            {
                if (feather == null) yield break;
                feather.localRotation = Quaternion.identity;
                speed = Mathf.Min(MaxFall, speed + 45f * Time.deltaTime);
                var step = speed * Time.deltaTime;
                var from = (Vector2)feather.position;
                if (Physics2D.Raycast(from, Vector2.down, step + 0.5f, 1 << 8).collider != null) break;

                feather.position += Vector3.down * step;
                if (BossTwists.HitHero(gameObject, feather.position + Vector3.down * 0.4f, HitRadius, 1)) break;
                yield return null;
            }

            if (feather != null)
            {
                feathers.Remove(feather.gameObject);
                Destroy(feather.gameObject);
            }
        }

        private void OnDisable()
        {
            foreach (var f in feathers)
            {
                if (f != null) Destroy(f);
            }
            feathers.Clear();
        }
    }
}
