using UnityEngine;

namespace VirusInside
{
    /// <summary>Маленький страж: бродит, преследует, делает выпад и хватает за горло.</summary>
    public class Monster2D : MonoBehaviour
    {
        public Vector2 Pos;
        public Vector2 Home = new Vector2(12f, -3f);
        public bool FinaleMode;

        enum Mode { Wander, Chase, Hold, Throw, Stunned }

        VirusGame game;
        SpriteRenderer sr;
        Transform visual;
        Transform shadow;
        SpriteRenderer aura;

        Sprite downIdle, downA, downB;
        Sprite upIdle, upA, upB;
        Sprite leftIdle, leftA, leftB;
        Sprite rightIdle, rightA, rightB;
        Sprite grabLeft, holdLeft, throwLeft, grabRight, holdRight, throwRight;

        Mode mode = Mode.Wander;
        Vector2 wanderTarget;
        Vector2 lastDir = Vector2.down;
        float wanderTimer = 2f;
        float animTime;
        float lungeCooldown = 4f;
        float lungeTimer;
        float telegraphTimer;
        float stunTimer;
        float throwTimer;
        float holdAnimTimer;
        float grabSide = -1f;
        bool growlPlayed;

        const float ChaseRange = 11f;
        const float GrabRange = 1.55f;

        public void Init(VirusGame g, Vector2 start)
        {
            game = g;
            Pos = start;
            Home = start;
            transform.position = new Vector3(Pos.x, Pos.y, 0f);

            downIdle = VirusArt.Load("monster_down_idle", 2.7f);
            downA = VirusArt.Load("monster_down_walk0", 2.7f);
            downB = VirusArt.Load("monster_down_walk1", 2.7f);
            upIdle = VirusArt.Load("monster_up_idle", 2.7f);
            upA = VirusArt.Load("monster_up_walk0", 2.7f);
            upB = VirusArt.Load("monster_up_walk1", 2.7f);
            leftIdle = VirusArt.Load("monster_left_idle", 2.7f);
            leftA = VirusArt.Load("monster_left_walk0", 2.7f);
            leftB = VirusArt.Load("monster_left_walk1", 2.7f);
            rightIdle = VirusArt.Load("monster_right_idle", 2.7f);
            rightA = VirusArt.Load("monster_right_walk0", 2.7f);
            rightB = VirusArt.Load("monster_right_walk1", 2.7f);
            grabLeft = VirusArt.Load("monster_grab_left", 2.7f);
            holdLeft = VirusArt.Load("monster_hold_left", 2.7f);
            throwLeft = VirusArt.Load("monster_throw_left", 2.7f);
            grabRight = VirusArt.Load("monster_grab_right", 2.7f);
            holdRight = VirusArt.Load("monster_hold_right", 2.7f);
            throwRight = VirusArt.Load("monster_throw_right", 2.7f);

            var vis = new GameObject("Visual");
            vis.transform.SetParent(transform, false);
            visual = vis.transform;
            sr = vis.AddComponent<SpriteRenderer>();
            sr.sprite = downIdle;
            sr.sortingOrder = 120;

            var sh = new GameObject("Shadow");
            sh.transform.SetParent(transform, false);
            sh.transform.localPosition = new Vector3(0f, 0.05f, 0f);
            sh.transform.localScale = new Vector3(1.9f, 1.15f, 1f);
            shadow = sh.transform;
            var shsr = sh.AddComponent<SpriteRenderer>();
            shsr.sprite = VirusArt.Shadow;
            shsr.color = new Color(0f, 0f, 0f, 0.7f);
            shsr.sortingOrder = 119;

            var au = new GameObject("Aura");
            au.transform.SetParent(transform, false);
            au.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            au.transform.localScale = new Vector3(3.2f, 3.2f, 1f);
            aura = au.AddComponent<SpriteRenderer>();
            aura.sprite = VirusArt.Glow;
            aura.color = new Color(0.75f, 0.08f, 0.10f, 0.13f);
            aura.sortingOrder = 118;

            PickWanderTarget();
        }

        public void SetHome(Vector2 home)
        {
            Home = home;
            PickWanderTarget();
        }

        void Update()
        {
            if (game == null) return;

            if (mode == Mode.Hold)
            {
                holdAnimTimer += Time.deltaTime;
                sr.sprite = (grabSide < 0f ? (holdAnimTimer < 0.22f ? grabLeft : holdLeft)
                                           : (holdAnimTimer < 0.22f ? grabRight : holdRight));
                visual.localPosition = new Vector3(Mathf.Sin(Time.unscaledTime * 55f) * 0.02f, 0f, 0f);
                sr.color = new Color(1f, 1f, 1f, 1f);
                UpdateOrder();
                return;
            }

            if (!game.CanPlay)
            {
                SetIdleSprite();
                UpdateOrder();
                return;
            }

            aura.color = new Color(0.75f, 0.08f, 0.10f,
                0.10f + Mathf.Abs(Mathf.Sin(Time.time * 1.7f)) * 0.06f);

            if (mode == Mode.Throw)
            {
                throwTimer -= Time.deltaTime;
                sr.sprite = grabSide < 0f ? throwRight : throwLeft;
                if (throwTimer <= 0f) mode = Mode.Stunned;
                UpdateOrder();
                return;
            }

            if (mode == Mode.Stunned)
            {
                stunTimer -= Time.deltaTime;
                sr.color = new Color(1f, 1f, 1f, 0.45f + Mathf.Abs(Mathf.Sin(Time.time * 18f)) * 0.55f);
                if (stunTimer <= 0f)
                {
                    mode = Mode.Wander;
                    sr.color = Color.white;
                    lungeCooldown = 2.5f;
                }
                UpdateOrder();
                return;
            }

            sr.color = Color.white;

            if (game.InInterior)
            {
                sr.sprite = leftIdle;
                UpdateOrder();
                return;
            }

            Vector2 player = game.PlayerPos;
            float dist = Vector2.Distance(Pos, player);
            bool wantChase = dist < ChaseRange;
            if (FinaleMode && !wantChase) wantChase = false;

            if (wantChase)
            {
                mode = Mode.Chase;
                ChasePlayer(player, dist);
            }
            else
            {
                mode = Mode.Wander;
                Wander();
            }

            UpdateOrder();
        }

        void ChasePlayer(Vector2 player, float dist)
        {
            lungeCooldown -= Time.deltaTime;
            if (telegraphTimer > 0f)
            {
                telegraphTimer -= Time.deltaTime;
                sr.color = Color.Lerp(Color.white, new Color(1f, 0.25f, 0.25f),
                    0.4f + Mathf.Abs(Mathf.Sin(Time.time * 30f)) * 0.6f);
                if (!growlPlayed)
                {
                    game.Audio.Growl();
                    growlPlayed = true;
                }
                if (telegraphTimer <= 0f)
                {
                    lungeTimer = 1.15f;
                    lungeCooldown = 6.5f;
                }
                SetIdleSprite();
                return;
            }

            if (lungeTimer > 0f) lungeTimer -= Time.deltaTime;
            else if (lungeCooldown <= 0f && dist < 7.5f)
            {
                telegraphTimer = 0.35f;
                growlPlayed = false;
                SetIdleSprite();
                return;
            }

            float speed = lungeTimer > 0f ? 8.4f : 4.9f;
            Vector2 dir = (player - Pos).normalized;
            if (dir.sqrMagnitude > 0.01f) lastDir = dir;
            Pos += dir * speed * Time.deltaTime;
            game.ResolveWorld(ref Pos, 0.65f);
            transform.position = new Vector3(Pos.x, Pos.y, 0f);

            animTime += Time.deltaTime * (lungeTimer > 0f ? 11f : 6f);
            sr.sprite = WalkSprite(lastDir, animTime);

            if (dist < GrabRange && game.PlayerInvulnerable <= 0f)
            {
                grabSide = player.x >= Pos.x ? -1f : 1f;
                holdAnimTimer = 0f;
                mode = Mode.Hold;
                game.OnMonsterGrab(this);
            }
        }

        void Wander()
        {
            wanderTimer -= Time.deltaTime;
            if (wanderTimer <= 0f)
            {
                PickWanderTarget();
            }

            if (FinaleMode)
            {
                wanderTarget = game.AltarPos;
                if (Vector2.Distance(Pos, wanderTarget) < 1.6f)
                {
                    SetIdleSprite();
                    return;
                }
            }

            Vector2 delta = wanderTarget - Pos;
            if (delta.sqrMagnitude < 0.08f)
            {
                SetIdleSprite();
                return;
            }
            Vector2 dir = delta.normalized;
            lastDir = dir;
            Pos += dir * 2.3f * Time.deltaTime;
            game.ResolveWorld(ref Pos, 0.65f);
            transform.position = new Vector3(Pos.x, Pos.y, 0f);
            animTime += Time.deltaTime * 3.2f;
            sr.sprite = WalkSprite(lastDir, animTime);
        }

        void PickWanderTarget()
        {
            wanderTimer = Random.Range(2.6f, 5.5f);
            Vector2 offset = Random.insideUnitCircle * 6.5f;
            wanderTarget = FinaleMode ? game.AltarPos : Home + offset;
        }

        void SetIdleSprite()
        {
            sr.sprite = IdleSprite(lastDir);
        }

        Sprite IdleSprite(Vector2 dir)
        {
            if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y)) return dir.x >= 0f ? rightIdle : leftIdle;
            return dir.y >= 0f ? upIdle : downIdle;
        }

        Sprite WalkSprite(Vector2 dir, float t)
        {
            bool flip = Mathf.FloorToInt(t) % 2 == 0;
            if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
            {
                if (dir.x >= 0f) return flip ? rightA : rightB;
                return flip ? leftA : leftB;
            }
            if (dir.y >= 0f) return flip ? upA : upB;
            return flip ? downA : downB;
        }

        public void BeginHold(Vector2 playerPos)
        {
            Pos = playerPos + new Vector2(grabSide * 1.15f, 0.02f);
            transform.position = new Vector3(Pos.x, Pos.y, 0f);
            sr.sprite = grabSide < 0f ? grabLeft : grabRight;
            holdAnimTimer = 0f;
            mode = Mode.Hold;
            UpdateOrder();
        }

        public void Release(bool escaped)
        {
            mode = Mode.Throw;
            throwTimer = 0.45f;
            stunTimer = escaped ? 4f : 2.2f;
            Vector2 away = grabSide < 0f ? new Vector2(-1f, 0.35f) : new Vector2(1f, 0.35f);
            Pos += away.normalized * 2.8f;
            game.ResolveWorld(ref Pos, 0.65f);
            transform.position = new Vector3(Pos.x, Pos.y, 0f);
            sr.sprite = grabSide < 0f ? throwRight : throwLeft;
        }

        void UpdateOrder()
        {
            int order = -Mathf.RoundToInt(Pos.y * 20f) + 20;
            sr.sortingOrder = order;
            var shsr = shadow.GetComponent<SpriteRenderer>();
            if (shsr != null) shsr.sortingOrder = order - 1;
            if (aura != null) aura.sortingOrder = order - 2;
            shadow.position = new Vector3(Pos.x, Pos.y + 0.04f, 0f);
        }
    }
}
