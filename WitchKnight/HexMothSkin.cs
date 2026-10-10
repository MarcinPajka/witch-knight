using System.Collections.Generic;
using UnityEngine;

namespace WitchKnight
{
    /// <summary>
    /// Hides the Knight's own sprite and draws Hex Moth in its place, picking her animation
    /// from whichever animation clip the Knight is currently playing.
    /// </summary>
    internal sealed class HexMothSkin : MonoBehaviour
    {
        private HeroController hero = null!;
        private tk2dSprite knightSprite = null!;
        private tk2dSpriteAnimator knightAnimator = null!;
        private MeshRenderer knightRenderer = null!;
        private Rigidbody2D body = null!;
        private SpriteRenderer sprite = null!;
        private Dictionary<string, SheetAnimation> anims = null!;

        private string current = "";
        private float time;

        private void Awake()
        {
            hero = GetComponent<HeroController>();
            knightSprite = GetComponent<tk2dSprite>();
            knightAnimator = GetComponent<tk2dSpriteAnimator>();
            knightRenderer = GetComponent<MeshRenderer>();
            body = GetComponent<Rigidbody2D>();
            anims = SpriteSheets.Load();

            var go = new GameObject("Hex Moth Sprite");
            go.transform.SetParent(transform, false);
            go.layer = gameObject.layer;
            sprite = go.AddComponent<SpriteRenderer>();

            // Stand her feet on the bottom of the Knight's collider.
            var box = GetComponent<BoxCollider2D>();
            float feet = box != null ? box.offset.y - box.size.y / 2f : -0.64f;
            go.transform.localPosition = new Vector3(0f, feet, -0.001f);

            Play("idle");
        }

        private void LateUpdate()
        {
            // Keep the Knight invisible; the game resets his colour during flashes and cutscenes.
            if (knightSprite.color.a > 0f)
            {
                var c = knightSprite.color;
                knightSprite.color = new Color(c.r, c.g, c.b, 0f);
            }

            sprite.enabled = knightRenderer.enabled;
            sprite.sortingLayerID = knightRenderer.sortingLayerID;
            sprite.sortingOrder = knightRenderer.sortingOrder;

            // Our art faces right; undo whatever flip the Knight's transform carries.
            bool parentFlipped = transform.lossyScale.x < 0f;
            sprite.flipX = parentFlipped == hero.cState.facingRight;

            string next = Choose();
            if (next != current) Play(next);
            Step(Time.deltaTime);
        }

        private string Choose()
        {
            if (hero.cState.dead) return "death";
            if (Parry.ShowPose) return "block";

            string clip = knightAnimator.CurrentClip?.name ?? "";
            if (clip.Contains("Death")) return "death";
            if (clip.Contains("Recoil") || clip.Contains("Hurt") || clip.Contains("Stun")) return "hurt";
            if (clip.Contains("UpSlash")) return "attackUp";
            if (clip.Contains("DownSlash")) return "attackDown";
            if (clip.Contains("Slash")) return "attack";
            if (clip.Contains("Fireball") || clip.Contains("Scream") || clip.Contains("Quake") || clip.Contains("Focus")) return "cast";
            if (clip.Contains("Dash")) return "dash";
            if (clip.Contains("Land")) return "land";
            if (clip.Contains("Run") || clip.Contains("Walk")) return "run";

            if (!hero.cState.onGround)
            {
                // Keep the jump rising pose until she starts to fall.
                return body.velocity.y > 0.1f ? "jump" : "fall";
            }

            return "idle";
        }

        private void Play(string name)
        {
            if (!anims.ContainsKey(name)) name = "idle";
            current = name;
            time = 0f;
            sprite.sprite = anims[name].Frames[0];
        }

        private void Step(float dt)
        {
            var a = anims[current];
            time += dt;
            int i = (int)(time * a.Fps);
            i = a.Loop ? i % a.Frames.Length : Mathf.Min(i, a.Frames.Length - 1);
            sprite.sprite = a.Frames[i];
        }
    }
}
