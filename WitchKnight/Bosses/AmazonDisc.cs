using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WitchKnight
{
    /// <summary>
    /// The Amazon throws a spinning bronze disc at Hex Moth. It slows, hangs, then flies back
    /// to her hand, so it can catch Hex Moth on the way out and again on the way back.
    /// In her second fight she throws two at once. The bronze parry turns a disc aside.
    /// </summary>
    internal sealed class AmazonDisc : BossTwist
    {
        private const float OutSpeed = 21f;
        private const float OutTime = 0.75f;
        private const float HangTime = 0.2f;
        private const float BackSpeed = 24f;
        private const float HitRadius = 0.75f;

        private static readonly Color Colour = new(0.95f, 0.6f, 0.35f);
        private static readonly string[] lines = { "Catch!", "Run, little moth!", "Bronze bites twice!" };

        private readonly List<GameObject> discs = new();

        private bool SecondFight => gameObject.name.EndsWith("2");

        protected override IEnumerator Loop()
        {
            while (true)
            {
                yield return new WaitForSeconds(Random.Range(5.5f, 8f) * (Enraged ? 0.8f : 1f));
                while (!CanAct(26f)) yield return null;

                Barks.Say("The Amazon", lines[Random.Range(0, lines.Length)], Colour);
                if (SecondFight)
                {
                    StartCoroutine(Throw(12f));
                    StartCoroutine(Throw(-12f));
                }
                else
                {
                    StartCoroutine(Throw(0f));
                }
            }
        }

        private IEnumerator Throw(float spreadDegrees)
        {
            var hero = HeroController.instance;
            if (hero == null) yield break;

            var sr = BossTwists.Spawn("Amazon Disc", BossTwists.Disc, Centre, 1.3f);
            var disc = sr.transform;
            discs.Add(disc.gameObject);

            var aim = (Vector2)(hero.transform.position - Centre);
            if (aim.sqrMagnitude < 0.01f) aim = Vector2.right;
            var dir = (Vector2)(Quaternion.Euler(0f, 0f, spreadDegrees) * aim.normalized);
            var hitOut = false;
            var hitBack = false;

            // Out: fast, easing to a stop.
            for (var t = 0f; t < OutTime + HangTime; t += Time.deltaTime)
            {
                if (disc == null || Health == null || Health.GetIsDead()) break;
                var speed = OutSpeed * Mathf.Clamp01(1f - t / OutTime);
                disc.position += (Vector3)(dir * speed * Time.deltaTime);
                disc.Rotate(0f, 0f, -900f * Time.deltaTime);
                if (!hitOut && speed > 2f) hitOut = BossTwists.HitHero(gameObject, disc.position, HitRadius, 1);
                yield return null;
            }

            // Back: home in on her hand.
            for (var t = 0f; t < 2.5f; t += Time.deltaTime)
            {
                if (disc == null || Health == null || Health.GetIsDead()) break;
                var to = Centre - disc.position;
                to.z = 0f;
                if (to.sqrMagnitude < 0.8f) break;
                var speed = BackSpeed * Mathf.Clamp01(0.3f + t * 2f);
                disc.position += to.normalized * Mathf.Min(speed * Time.deltaTime, to.magnitude);
                disc.Rotate(0f, 0f, 900f * Time.deltaTime);
                if (!hitBack) hitBack = BossTwists.HitHero(gameObject, disc.position, HitRadius, 1);
                yield return null;
            }

            if (disc != null)
            {
                discs.Remove(disc.gameObject);
                Destroy(disc.gameObject);
            }
        }

        private void OnDisable()
        {
            foreach (var d in discs)
            {
                if (d != null) Destroy(d);
            }
            discs.Clear();
        }
    }
}
