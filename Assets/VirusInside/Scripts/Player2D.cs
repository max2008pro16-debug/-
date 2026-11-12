using UnityEngine;

namespace VirusInside
{
    /// <summary>Двумерный персонаж: движение по пустоши, анимация и коллизии с домами.</summary>
    public class Player2D : MonoBehaviour
    {
        public Vector2 Pos;

        VirusGame game;
        SpriteRenderer sr;
        Transform visual;
        Transform shadow;
        Sprite idleDown, idleUp, walkA, walkB, runA, runB;
        float animTime;
        float bobTime;
        bool moving;

        public void Init(VirusGame g, Vector2 start)
        {
            game = g;
            Pos = start;
            transform.position = new Vector3(Pos.x, Pos.y, 0f);

            idleDown = VirusArt.Load("hero_down", 2.35f);
            idleUp = VirusArt.Load("hero_up", 2.35f);
            walkA = VirusArt.Load("hero_side_a", 2.35f);
            walkB = VirusArt.Load("hero_side_b", 2.35f);
            runA = VirusArt.Load("hero_run_a", 2.35f);
            runB = VirusArt.Load("hero_run_b", 2.35f);

            var vis = new GameObject("Visual");
            vis.transform.SetParent(transform, false);
            visual = vis.transform;
            sr = vis.AddComponent<SpriteRenderer>();
            sr.sprite = idleDown;
            sr.sortingOrder = 120;

            var sh = new GameObject("Shadow");
            sh.transform.SetParent(transform, false);
            sh.transform.localPosition = new Vector3(0f, 0.05f, 0f);
            sh.transform.localScale = new Vector3(1.6f, 1f, 1f);
            shadow = sh.transform;
            var shsr = sh.AddComponent<SpriteRenderer>();
            shsr.sprite = VirusArt.Shadow;
            shsr.color = new Color(1f, 1f, 1f, 0.8f);
            shsr.sortingOrder = 119;
        }

        public void Teleport(Vector2 p)
        {
            Pos = p;
            transform.position = new Vector3(p.x, p.y, 0f);
        }

        void Update()
        {
            if (game == null) return;
            bool active = game.CanPlay;
            Vector2 input = Vector2.zero;
            bool sprint = false;

            if (active)
            {
                if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) input.y += 1f;
                if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) input.y -= 1f;
                if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) input.x -= 1f;
                if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) input.x += 1f;
                sprint = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            }

            moving = input.sqrMagnitude > 0.001f;
            if (moving)
            {
                input.Normalize();
                float speed = sprint ? 9.2f : 6.2f;
                Pos += input * speed * Time.deltaTime;
                game.ResolveWorld(ref Pos, 0.55f);
                transform.position = new Vector3(Pos.x, Pos.y, 0f);
                animTime += Time.deltaTime * (sprint ? 1.7f : 1f);
                bobTime += Time.deltaTime * (sprint ? 18f : 11f);
                game.Audio.Step();
            }
            else
            {
                bobTime = Mathf.Lerp(bobTime, 0f, Time.deltaTime * 8f);
            }

            UpdateSprite(input, sprint);
            UpdateSorting();

            int order = SortOrder();
            sr.sortingOrder = order;
            var shsr = shadow.GetComponent<SpriteRenderer>();
            if (shsr != null) shsr.sortingOrder = order - 1;
            shadow.transform.position = new Vector3(Pos.x, Pos.y + 0.04f, 0f);

            float alpha = game.PlayerInvulnerable > 0f && Mathf.Repeat(Time.unscaledTime * 12f, 2f) < 1f ? 0.35f : 1f;
            sr.color = new Color(1f, 1f, 1f, alpha);
        }

        void UpdateSprite(Vector2 input, bool sprint)
        {
            bool horizontal = Mathf.Abs(input.x) >= Mathf.Abs(input.y) && Mathf.Abs(input.x) > 0.01f;
            if (moving && horizontal)
            {
                sr.sprite = sprint ? (Mathf.FloorToInt(animTime * 11f) % 2 == 0 ? runA : runB)
                                   : (Mathf.FloorToInt(animTime * 7f) % 2 == 0 ? walkA : walkB);
                sr.flipX = input.x > 0f;
            }
            else if (moving)
            {
                sr.sprite = input.y > 0f ? idleUp : idleDown;
                sr.flipX = false;
            }
            else
            {
                sr.sprite = idleDown;
                sr.flipX = false;
            }

            float bob = moving ? Mathf.Abs(Mathf.Sin(bobTime)) * 0.06f : Mathf.Sin(Time.time * 2f) * 0.012f;
            visual.localPosition = new Vector3(0f, bob, 0f);
        }

        void UpdateSorting()
        {
            int order = SortOrder();
            sr.sortingOrder = order;
        }

        int SortOrder() => -Mathf.RoundToInt(Pos.y * 20f) + 20;
    }
}
