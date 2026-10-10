using System.Collections;
using UnityEngine;

namespace WitchKnight
{
    /// <summary>
    /// The Gorgon Sisters' petrifying gaze. A sister opens a great eye above her; when it
    /// flashes, Hex Moth must be facing away from her. Caught facing her, she turns to stone
    /// for a moment and cannot move or strike. The sisters take turns, and gaze more often
    /// once two of them fight together.
    /// </summary>
    internal sealed class GorgonGaze : BossTwist
    {
        private const float Warning = 1.4f;
        private const float StoneTime = 1.1f;

        private static readonly Color Colour = new(0.55f, 0.85f, 0.5f);
        private static readonly Color StoneTint = new(0.55f, 0.55f, 0.55f);

        // One gaze at a time across all the sisters.
        private static float nextGazeAt;
        private static GorgonGaze? gazing;

        private SpriteRenderer? eye;
        private bool petrifying;

        private string SisterName => gameObject.name.EndsWith("S1") ? "Euryale"
            : gameObject.name.EndsWith("S2") ? "Medusa"
            : "Stheno";

        private bool Duet => gameObject.name.EndsWith("S1") || gameObject.name.EndsWith("S2");

        protected override IEnumerator Loop()
        {
            // First gaze a few seconds after the fight starts, not straight away.
            while (!CanAct()) yield return null;
            if (Time.time > nextGazeAt + 30f) nextGazeAt = Time.time + 4f;

            while (true)
            {
                yield return null;
                if (gazing != null || Time.time < nextGazeAt || !CanAct(30f)) continue;

                gazing = this;
                nextGazeAt = Time.time + Random.Range(8f, 11f) * (Duet ? 0.7f : 1f);
                yield return Gaze();
                if (gazing == this) gazing = null;
            }
        }

        private IEnumerator Gaze()
        {
            Barks.Say(SisterName, Random.value < 0.5f ? "Look upon me, little moth." : "Meet my eyes...", Colour);
            var e = BossTwists.Spawn("Gorgon Eye", BossTwists.Eye, Centre + Vector3.up * 3f, 0.2f);
            eye = e;

            for (var t = 0f; t < Warning; t += Time.deltaTime)
            {
                if (Health == null || Health.GetIsDead()) { CloseEye(); yield break; }
                var k = t / Warning;
                e.transform.position = Centre + Vector3.up * 3f;
                e.transform.localScale = new Vector3(2.2f, 2.2f * Mathf.Clamp01(k * 1.5f), 1f);
                // The eye throbs faster as the gaze nears.
                e.color = Color.Lerp(Color.white, Colour, Mathf.PingPong(t * (2f + 6f * k), 1f));
                yield return null;
            }

            Hex.Pulse(Centre + Vector3.up * 3f, 14f, Colour);
            CloseEye();

            var hero = HeroController.instance;
            if (hero == null || hero.cState.dead || hero.cState.transitioning) yield break;

            var sheIsRight = Centre.x >= hero.transform.position.x;
            if (hero.cState.facingRight == sheIsRight)
            {
                yield return Petrify(hero);
            }
            else if (Random.value < 0.4f)
            {
                Barks.Say("Hex Moth", "Not today, sister.", Barks.MothColour);
            }
        }

        private IEnumerator Petrify(HeroController hero)
        {
            petrifying = true;
            Barks.Say("Hex Moth", "Stone... creeping...!", Barks.MothColour);
            Hex.Pulse(hero.transform.position, 2f, StoneTint);

            var body = hero.GetComponent<Rigidbody2D>();
            var skin = hero.transform.Find("Hex Moth Sprite")?.GetComponent<SpriteRenderer>();
            hero.RelinquishControl();
            if (skin != null) skin.color = StoneTint;

            for (var t = 0f; t < StoneTime; t += Time.deltaTime)
            {
                if (hero.cState.dead || hero.cState.transitioning) break;
                if (body != null) body.velocity = new Vector2(0f, Mathf.Min(0f, body.velocity.y));
                yield return null;
            }

            Release(hero);
        }

        private void Release(HeroController? hero)
        {
            if (!petrifying) return;
            petrifying = false;
            if (hero == null) return;

            var skin = hero.transform.Find("Hex Moth Sprite")?.GetComponent<SpriteRenderer>();
            if (skin != null) skin.color = Color.white;
            if (!hero.cState.dead) hero.RegainControl();
        }

        private void CloseEye()
        {
            if (eye != null) Destroy(eye.gameObject);
            eye = null;
        }

        private void OnDisable()
        {
            // Never leave her stuck in stone if the sister vanishes mid-gaze.
            Release(HeroController.instance);
            CloseEye();
            if (gazing == this) gazing = null;
        }
    }
}
