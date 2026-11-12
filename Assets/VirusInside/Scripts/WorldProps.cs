using System;
using System.Net.NetworkInformation;
using UnityEngine;

namespace VirusInside
{
    /// <summary>Дом в пустоши. Хранит прямоугольник коллизии и точку двери.</summary>
    public class HouseDoor : MonoBehaviour
    {
        public int RoomIndex;
        public string Title;
        public Vector2 DoorPoint;
        public Rect Body;

        Transform mark;
        SpriteRenderer markSr;
        float t;

        public void Init(VirusGame game, int roomIndex, string title, Vector2 basePos, bool snow)
        {
            RoomIndex = roomIndex;
            Title = title;
            name = "House_" + roomIndex;
            transform.position = new Vector3(basePos.x, basePos.y, 0f);

            var sprite = VirusArt.Load(snow ? "house_snow" : "house_sand", 6.8f, 0f);
            var sr = gameObject.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = -Mathf.RoundToInt(basePos.y * 20f);

            float w = sprite.bounds.size.x;
            float h = sprite.bounds.size.y;
            Body = new Rect(basePos.x - w * 0.42f, basePos.y + 0.55f, w * 0.84f, h * 0.66f);
            DoorPoint = new Vector2(basePos.x, basePos.y - 0.35f);

            var glow = new GameObject("PorchGlow");
            glow.transform.SetParent(transform, false);
            glow.transform.localPosition = new Vector3(0f, 1.3f, 0f);
            glow.transform.localScale = new Vector3(3f, 2f, 1f);
            var gsr = glow.AddComponent<SpriteRenderer>();
            gsr.sprite = VirusArt.Glow;
            gsr.color = new Color(0.95f, 0.72f, 0.35f, 0.14f);
            gsr.sortingOrder = sr.sortingOrder + 1;

            var m = new GameObject("DoorMark");
            m.transform.SetParent(transform, false);
            m.transform.localPosition = new Vector3(0f, 0.9f, 0f);
            m.transform.localScale = new Vector3(1.05f, 1.05f, 1f);
            mark = m.transform;
            markSr = m.AddComponent<SpriteRenderer>();
            markSr.sprite = VirusArt.DoorMark;
            markSr.color = new Color(1f, 1f, 1f, 0.75f);
            markSr.sortingOrder = sr.sortingOrder + 2;
        }

        void Update()
        {
            if (mark == null) return;
            t += Time.deltaTime;
            float s = 1f + Mathf.Sin(t * 2.4f) * 0.08f;
            mark.localScale = new Vector3(s, s, 1f);
            var c = markSr.color;
            c.a = 0.55f + Mathf.Abs(Mathf.Sin(t * 2.4f)) * 0.35f;
            markSr.color = c;
        }
    }

    /// <summary>Аптечка. Лечит 2/3 здоровья и возвращается через минуту.</summary>
    public class Medkit2D : MonoBehaviour
    {
        public Vector2 Pos;
        public bool Taken;

        VirusGame game;
        SpriteRenderer sr;
        SpriteRenderer glow;
        Transform visual;
        float timer;
        float phase;

        public void Init(VirusGame g, Vector2 pos, float phaseOffset)
        {
            game = g;
            Pos = pos;
            phase = phaseOffset;
            name = "Medkit";
            transform.position = new Vector3(pos.x, pos.y, 0f);

            var shadow = new GameObject("Shadow");
            shadow.transform.SetParent(transform, false);
            shadow.transform.localScale = new Vector3(0.8f, 0.6f, 1f);
            var shsr = shadow.AddComponent<SpriteRenderer>();
            shsr.sprite = VirusArt.Shadow;
            shsr.color = new Color(0f, 0f, 0f, 0.5f);

            var vis = new GameObject("Visual");
            vis.transform.SetParent(transform, false);
            visual = vis.transform;
            sr = vis.AddComponent<SpriteRenderer>();
            sr.sprite = VirusArt.MedkitIcon;
            sr.transform.localScale = new Vector3(0.85f, 0.85f, 1f);

            var gl = new GameObject("Glow");
            gl.transform.SetParent(transform, false);
            gl.transform.localScale = new Vector3(1.7f, 1.7f, 1f);
            glow = gl.AddComponent<SpriteRenderer>();
            glow.sprite = VirusArt.Glow;
            glow.color = new Color(0.35f, 0.95f, 0.55f, 0.12f);

            UpdateOrder();
        }

        public void Take()
        {
            if (Taken) return;
            Taken = true;
            timer = 75f;
            if (sr != null) sr.enabled = false;
            if (glow != null) glow.enabled = false;
            game.Audio.Heal();
        }

        void Update()
        {
            if (game == null) return;
            if (Taken)
            {
                timer -= Time.deltaTime;
                if (timer <= 0f) Respawn();
                return;
            }

            float bob = Mathf.Sin(Time.time * 2.2f + phase) * 0.09f;
            if (visual != null) visual.localPosition = new Vector3(0f, 0.75f + bob, 0f);
            if (sr != null) sr.sortingOrder = Order();
            if (glow != null)
            {
                glow.sortingOrder = Order() - 1;
                float a = 0.10f + Mathf.Abs(Mathf.Sin(Time.time * 2f + phase)) * 0.10f;
                glow.color = new Color(0.35f, 0.95f, 0.55f, a);
            }
            if (glow != null) glow.transform.position = new Vector3(Pos.x, Pos.y + 0.8f, 0f);
        }

        void Respawn()
        {
            Taken = false;
            timer = 0f;
            if (sr != null) sr.enabled = true;
            if (glow != null) glow.enabled = true;
        }

        int Order() => -Mathf.RoundToInt(Pos.y * 20f) + 4;

        void UpdateOrder()
        {
            if (sr != null) sr.sortingOrder = Order();
            if (glow != null) glow.sortingOrder = Order() - 1;
        }
    }

    /// <summary>Алтарь. Просыпается от десяти кристаллов, здесь происходит финал.</summary>
    public class Altar2D : MonoBehaviour
    {
        public Vector2 Pos;
        public bool Awake;

        VirusGame game;
        SpriteRenderer sr;
        SpriteRenderer glow;
        SpriteRenderer ring;
        float t;

        public void Init(VirusGame g, Vector2 pos)
        {
            game = g;
            Pos = pos;
            name = "Altar";
            transform.position = new Vector3(pos.x, pos.y, 0f);

            var gl = new GameObject("Glow");
            gl.transform.SetParent(transform, false);
            gl.transform.localPosition = new Vector3(0f, 0.7f, 0f);
            gl.transform.localScale = new Vector3(5.2f, 4.2f, 1f);
            glow = gl.AddComponent<SpriteRenderer>();
            glow.sprite = VirusArt.Glow;
            glow.color = new Color(0.35f, 0.85f, 0.85f, 0.06f);

            var rg = new GameObject("RuneRing");
            rg.transform.SetParent(transform, false);
            rg.transform.localPosition = new Vector3(0f, 0.55f, 0f);
            rg.transform.localScale = new Vector3(4.0f, 3.0f, 1f);
            ring = rg.AddComponent<SpriteRenderer>();
            ring.sprite = VirusArt.Glow;
            ring.color = new Color(0.35f, 0.95f, 0.88f, 0.0f);

            var vis = new GameObject("Visual");
            vis.transform.SetParent(transform, false);
            vis.transform.localScale = new Vector3(2.1f, 2.1f, 1f);
            sr = vis.AddComponent<SpriteRenderer>();
            sr.sprite = VirusArt.Altar;
            sr.color = new Color(0.72f, 0.78f, 0.78f, 1f);

            UpdateOrder();
        }

        public void SetAwake(bool awake)
        {
            Awake = awake;
        }

        void Update()
        {
            if (game == null) return;
            t += Time.deltaTime;
            int order = Order();
            sr.sortingOrder = order;
            glow.sortingOrder = order - 1;
            ring.sortingOrder = order - 2;

            float target = Awake ? 0.9f : 0.25f;
            float pulse = target * (0.7f + Mathf.Abs(Mathf.Sin(t * 1.8f)) * 0.5f);
            sr.color = Color.Lerp(new Color(0.55f, 0.60f, 0.60f, 1f), new Color(0.85f, 1f, 0.95f, 1f), Awake ? 0.7f : 0f);
            glow.color = new Color(0.35f, 0.9f, 0.85f, Awake ? 0.10f + pulse * 0.12f : 0.03f);
            ring.color = new Color(0.35f, 0.95f, 0.88f, Awake ? 0.10f + Mathf.Abs(Mathf.Sin(t * 2.5f)) * 0.16f : 0f);
            ring.transform.localRotation = Quaternion.Euler(0f, 0f, t * (Awake ? 18f : 0f));
            glow.transform.localScale = new Vector3(5.2f + pulse, 4.2f + pulse * 0.7f, 1f);
        }
        void UpdateOrder()
        {
            int order = Order();
            if (sr != null) sr.sortingOrder = order;
            if (glow != null) glow.sortingOrder = order - 1;
            if (ring != null) ring.sortingOrder = order - 2;
        }

        int Order() => -Mathf.RoundToInt(Pos.y * 20f) + 2;
    }

    /// <summary>Ритуальный нож, появляется у алтаря после всех записок.</summary>
    public class KnifePickup : MonoBehaviour
    {
        public Vector2 Pos;
        public bool Taken;

        VirusGame game;
        SpriteRenderer sr;
        SpriteRenderer glow;
        Transform visual;
        float t;

        public void Init(VirusGame g, Vector2 pos)
        {
            game = g;
            Pos = pos;
            name = "RitualKnife";
            transform.position = new Vector3(pos.x, pos.y, 0f);

            var gl = new GameObject("Glow");
            gl.transform.SetParent(transform, false);
            gl.transform.localScale = new Vector3(3.2f, 2.4f, 1f);
            glow = gl.AddComponent<SpriteRenderer>();
            glow.sprite = VirusArt.Glow;
            glow.color = new Color(1f, 0.85f, 0.45f, 0.18f);

            var vis = new GameObject("Visual");
            vis.transform.SetParent(transform, false);
            visual = vis.transform;
            sr = vis.AddComponent<SpriteRenderer>();
            sr.sprite = VirusArt.Knife;
            sr.transform.localScale = new Vector3(1.5f, 1.5f, 1f);
            UpdateOrder();
        }

        public void Take()
        {
            Taken = true;
            gameObject.SetActive(false);
        }

        void Update()
        {
            if (game == null || Taken) return;
            t += Time.deltaTime;
            visual.localPosition = new Vector3(0f, 0.8f + Mathf.Sin(t * 2.2f) * 0.08f, 0f);
            visual.localRotation = Quaternion.Euler(0f, 0f, -18f + Mathf.Sin(t * 1.5f) * 8f);
            sr.sortingOrder = Order() + 2;
            glow.sortingOrder = Order() + 1;
            glow.transform.position = new Vector3(Pos.x, Pos.y + 0.8f, 0f);
            glow.color = new Color(1f, 0.85f, 0.45f, 0.12f + Mathf.Abs(Mathf.Sin(t * 2f)) * 0.12f);
        }

        int Order() => -Mathf.RoundToInt(Pos.y * 20f) + 2;

        void UpdateOrder()
        {
            if (sr != null) sr.sortingOrder = Order() + 2;
            if (glow != null) glow.sortingOrder = Order() + 1;
        }
    }
}
