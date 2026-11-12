using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace VirusInside
{
    /// <summary>Главный контроллер: мир, состояние игры, здоровье, финал.</summary>
    public class VirusGame : MonoBehaviour
    {
        public const int InteriorLayer = 8;
        public const float MaxHealth = 100f;
        public const int CrystalsNeeded = 10;
        public const int NotesNeeded = 10;

        public enum GameState { Menu, Intro, Playing, Note, QTE, Choice, Paused, Transition, Ending, Dead }

        public static VirusGame I;

        public VirusUI UI;
        public VirusAudio Audio;
        public Camera Cam2D;
        public Camera Cam3D;
        public Player2D Player;
        public Monster2D Monster;
        public Altar2D AltarObj;
        public KnifePickup Knife;
        public readonly List<Room3D> Rooms = new List<Room3D>();
        public readonly List<Medkit2D> Medkits = new List<Medkit2D>();
        public readonly List<HouseDoor> Houses = new List<HouseDoor>();

        public GameState State { get; private set; } = GameState.Menu;
        public bool InInterior { get; private set; }
        public Room3D CurrentRoom { get; private set; }
        public bool FinaleMode { get; private set; }
        public Vector2 AltarPos => new Vector2(0f, 15f);
        public float Health { get; private set; } = MaxHealth;
        public int Crystals { get; private set; }
        public int NotesCount { get; private set; }
        public bool HasKnife { get; private set; }
        public float PlayerInvulnerable { get; private set; }
        public bool CanPlay => State == GameState.Playing;
        public Vector2 PlayerPos => Player != null ? Player.Pos : Vector2.zero;
        public int IntroPage => introPage;

        readonly bool[] notesRead = new bool[NoteData.NoteCount];
        Transform worldRoot;
        Transform roomRoot;
        float shakeTime;
        float shakeAmount;
        float inputLockUntil;
        int introPage;
        string pendingToast;
        float qteElapsed;
        int qtePresses;
        int qteNeed;
        const float QteTime = 6f;
        int rendererCount = -1;

        // ------------------------------------------------------------------ build
        void Awake()
        {
            I = this;
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 1;

            SetupPipeline();
            BuildCameras();
            BuildLighting();

            Audio = gameObject.AddComponent<VirusAudio>();

            var uiGo = new GameObject("UI");
            uiGo.transform.SetParent(transform, false);
            UI = uiGo.AddComponent<VirusUI>();
            UI.Init(this);

            BuildRooms();
            BuildOverworld();

            Health = MaxHealth;
            State = GameState.Menu;
            UI.ShowMenu();
            UI.SetHudVisible(false);
            UI.UpdateHealth(Health);
            UI.UpdateCounters(Health, Crystals, NotesCount, HasKnife);
            UI.SetObjective(ObjectiveText());
            Snap2DCamera(0f);
        }

        void OnDestroy()
        {
            if (I == this) I = null;
        }

        void SetupPipeline()
        {
            rendererCount = ReadRendererCount();
            if (GraphicsSettings.currentRenderPipeline != null)
            {
                if (rendererCount == 1)
                {
                    // Проект использует только 2D Renderer. Переключаемся на встроенный пайплайн,
                    // иначе трёхмерные комнаты не получат освещение.
                    try
                    {
                        QualitySettings.renderPipeline = null;
                        GraphicsSettings.defaultRenderPipeline = null;
                        VirusArt.Pipeline3D = VirusArt.Pipeline.Builtin;
                        return;
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning("[VirusInside] could not switch pipeline: " + e.Message);
                    }
                }
                VirusArt.Pipeline3D = VirusArt.Pipeline.Urp;
            }
            else
            {
                VirusArt.Pipeline3D = VirusArt.Pipeline.Builtin;
            }
        }

        int ReadRendererCount()
        {
            try
            {
                var asset = GraphicsSettings.currentRenderPipeline;
                if (asset == null) return 0;
                var f = asset.GetType().GetField("m_RendererDataList",
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                if (f != null && f.GetValue(asset) is Array arr) return arr.Length;
            }
            catch { }
            return -1;
        }

        void BuildCameras()
        {
            var cam2dGo = new GameObject("Camera2D");
            cam2dGo.transform.SetParent(transform, false);
            cam2dGo.transform.position = new Vector3(0f, 0f, -10f);
            Cam2D = cam2dGo.AddComponent<Camera>();
            Cam2D.orthographic = true;
            Cam2D.orthographicSize = 8.4f;
            Cam2D.nearClipPlane = 0.1f;
            Cam2D.farClipPlane = 100f;
            Cam2D.clearFlags = CameraClearFlags.SolidColor;
            Cam2D.backgroundColor = new Color(0.035f, 0.05f, 0.055f);
            Cam2D.cullingMask = ~(1 << InteriorLayer);
            Cam2D.depth = 0;
            cam2dGo.AddComponent<AudioListener>();

            var cam3dGo = new GameObject("Camera3D");
            cam3dGo.transform.SetParent(transform, false);
            Cam3D = cam3dGo.AddComponent<Camera>();
            Cam3D.orthographic = false;
            Cam3D.fieldOfView = 52f;
            Cam3D.nearClipPlane = 0.1f;
            Cam3D.farClipPlane = 80f;
            Cam3D.clearFlags = CameraClearFlags.SolidColor;
            Cam3D.backgroundColor = new Color(0.02f, 0.03f, 0.035f);
            Cam3D.cullingMask = 1 << InteriorLayer;
            Cam3D.depth = 1;
            Cam3D.enabled = false;

            if (GraphicsSettings.currentRenderPipeline != null && (rendererCount < 0 || rendererCount > 1))
            {
                try
                {
                    var data = Cam3D.GetUniversalAdditionalCameraData();
                    if (data != null) data.SetRenderer(1);
                }
                catch (Exception e)
                {
                    Debug.LogWarning("[VirusInside] 3D renderer assignment failed: " + e.Message);
                }
            }
        }

        void BuildLighting()
        {
            var go = new GameObject("Sun");
            go.transform.SetParent(transform, false);
            go.transform.rotation = Quaternion.Euler(48f, -28f, 0f);
            var l = go.AddComponent<Light>();
            l.type = LightType.Directional;
            l.color = new Color(1f, 0.93f, 0.82f);
            l.intensity = 1.15f;
            l.shadows = LightShadows.Soft;

            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.20f, 0.22f, 0.26f);
        }

        void BuildRooms()
        {
            roomRoot = new GameObject("Rooms").transform;
            roomRoot.SetParent(transform, false);

            Color[] walls =
            {
                new Color(0.20f, 0.25f, 0.24f),
                new Color(0.19f, 0.27f, 0.32f),
                new Color(0.29f, 0.23f, 0.16f),
                new Color(0.22f, 0.18f, 0.24f),
                new Color(0.18f, 0.24f, 0.19f)
            };
            Color[] floors =
            {
                new Color(0.31f, 0.24f, 0.17f),
                new Color(0.20f, 0.29f, 0.33f),
                new Color(0.36f, 0.29f, 0.19f),
                new Color(0.27f, 0.21f, 0.26f),
                new Color(0.22f, 0.28f, 0.21f)
            };
            Color[] accents =
            {
                new Color(0.38f, 0.78f, 0.68f),
                new Color(0.42f, 0.80f, 0.90f),
                new Color(0.90f, 0.70f, 0.36f),
                new Color(0.78f, 0.48f, 0.78f),
                new Color(0.48f, 0.82f, 0.50f)
            };

            for (int i = 0; i < 5; i++)
            {
                var go = new GameObject("Room_" + i);
                go.transform.SetParent(roomRoot, false);
                go.transform.position = new Vector3(6000f + i * 90f, 6000f, 0f);
                var room = go.AddComponent<Room3D>();
                room.Build(this, i, NoteData.HouseTitles[i], i * 2, i * 2 + 1,
                    walls[i], floors[i], accents[i], Cam3D);
                go.SetActive(false);
                Rooms.Add(room);
            }
        }

        void BuildOverworld()
        {
            worldRoot = new GameObject("Overworld").transform;
            worldRoot.SetParent(transform, false);

            var sand = GroundSprite("ground_sand");
            var snow = GroundSprite("ground_snow");
            CreateGround("Sand", sand, new Vector2(0f, 0f), new Vector2(58f, 44f), -10000);
            CreateGround("Snow", snow, new Vector2(0f, 11f), new Vector2(58f, 26f), -9999);

            var rng = new System.Random(1337);
            for (int i = 0; i < 34; i++)
            {
                float x = -28f + (float)rng.NextDouble() * 56f;
                float y = -2.4f + (float)rng.NextDouble() * 3.2f;
                var patch = NewSprite("SnowPatch", VirusArt.SnowPatch, new Vector2(x, y),
                    new Vector3(3f + (float)rng.NextDouble() * 3.4f, 1.8f + (float)rng.NextDouble() * 1.6f, 1f),
                    -9998, 0.9f);
                patch.color = new Color(0.86f, 0.92f, 0.99f, 0.85f);
            }

            var defs = new[]
            {
                new HouseDef(-17f, 8f, true, 0),
                new HouseDef(17f, 8f, true, 1),
                new HouseDef(-17f, -8f, false, 2),
                new HouseDef(17f, -8f, false, 3),
                new HouseDef(0f, -15f, true, 4)
            };
            foreach (var d in defs)
            {
                var go = new GameObject("House");
                go.transform.SetParent(worldRoot, false);
                var house = go.AddComponent<HouseDoor>();
                house.Init(this, d.Room, NoteData.HouseTitles[d.Room], new Vector2(d.X, d.Y), d.Snow);
                Houses.Add(house);
            }

            var medkitPositions = new[]
            {
                new Vector2(-8f, 14f), new Vector2(8f, 14f), new Vector2(-24f, 0f),
                new Vector2(24f, 0f), new Vector2(-8f, -15f), new Vector2(8f, -15f),
                new Vector2(0f, 5f), new Vector2(13f, 3f)
            };
            for (int i = 0; i < medkitPositions.Length; i++)
            {
                var go = new GameObject("Medkit");
                go.transform.SetParent(worldRoot, false);
                var m = go.AddComponent<Medkit2D>();
                m.Init(this, medkitPositions[i], i * 0.9f);
                Medkits.Add(m);
            }

            var altarGo = new GameObject("Altar");
            altarGo.transform.SetParent(worldRoot, false);
            AltarObj = altarGo.AddComponent<Altar2D>();
            AltarObj.Init(this, AltarPos);

            var playerGo = new GameObject("Player2D");
            playerGo.transform.SetParent(worldRoot, false);
            Player = playerGo.AddComponent<Player2D>();
            Player.Init(this, new Vector2(0f, -1.5f));

            var monsterGo = new GameObject("Monster");
            monsterGo.transform.SetParent(worldRoot, false);
            Monster = monsterGo.AddComponent<Monster2D>();
            Monster.Init(this, new Vector2(12f, -3f));

            ScatterDecor(rng, defs);

            // рамка из деревьев по краям мира
            for (int i = 0; i < 46; i++)
            {
                float t = i / 46f;
                BorderTree(rng, Mathf.Lerp(-28f, 28f, t), -21.4f);
                BorderTree(rng, Mathf.Lerp(-28f, 28f, t), 21.4f);
            }
            for (int i = 0; i < 32; i++)
            {
                float t = i / 32f;
                BorderTree(rng, -27.6f, Mathf.Lerp(-21f, 21f, t));
                BorderTree(rng, 27.6f, Mathf.Lerp(-21f, 21f, t));
            }

            Snap2DCamera(0f);
        }

        Sprite GroundSprite(string name) => VirusArt.Ground(name);

        void CreateGround(string groundName, Sprite sprite, Vector2 center, Vector2 size, int order)
        {
            var go = new GameObject(groundName);
            go.transform.SetParent(worldRoot, false);
            go.transform.position = new Vector3(center.x, center.y, 0f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.drawMode = SpriteDrawMode.Tiled;
            sr.tileMode = SpriteTileMode.Continuous;
            sr.size = size;
            sr.sortingOrder = order;
        }

        SpriteRenderer NewSprite(string spriteName, Sprite sprite, Vector2 pos, Vector3 scale, int order, float alpha)
        {
            var go = new GameObject(spriteName);
            go.transform.SetParent(worldRoot, false);
            go.transform.position = new Vector3(pos.x, pos.y, 0f);
            go.transform.localScale = scale;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = new Color(1f, 1f, 1f, alpha);
            sr.sortingOrder = order;
            return sr;
        }

        void ScatterDecor(System.Random rng, HouseDef[] defs)
        {
            int placed = 0;
            int guard = 0;
            while (placed < 72 && guard < 900)
            {
                guard++;
                float x = -26f + (float)rng.NextDouble() * 52f;
                float y = -19f + (float)rng.NextDouble() * 38f;
                Vector2 p = new Vector2(x, y);

                bool blocked = false;
                foreach (var d in defs)
                    if (Vector2.Distance(p, new Vector2(d.X, d.Y)) < 9.5f) blocked = true;
                if (Vector2.Distance(p, AltarPos) < 6.5f) blocked = true;
                if (Vector2.Distance(p, new Vector2(0f, -1.5f)) < 5f) blocked = true;
                foreach (var m in Medkits)
                    if (Vector2.Distance(p, m.Pos) < 2.4f) blocked = true;
                if (blocked) continue;

                bool tree = rng.NextDouble() < (y > 0f ? 0.55 : 0.35);
                if (tree)
                {
                    var spr = y > 2f ? VirusArt.TreeSnow : VirusArt.TreeSand;
                    float s = 0.85f + (float)rng.NextDouble() * 0.55f;
                    NewSprite("Tree", spr, p, new Vector3(s, s, 1f), DecorOrder(y), 1f);
                }
                else
                {
                    float s = 0.7f + (float)rng.NextDouble() * 0.7f;
                    NewSprite("Rock", VirusArt.Rock, p, new Vector3(s, s, 1f), DecorOrder(y), 1f);
                }
                placed++;
            }
        }

        void BorderTree(System.Random rng, float x, float y)
        {
            var spr = y > 2f ? VirusArt.TreeSnow : VirusArt.TreeSand;
            float s = 1f + (float)rng.NextDouble() * 0.5f;
            NewSprite("BorderTree", spr, new Vector2(x, y), new Vector3(s, s, 1f), DecorOrder(y), 1f);
        }

        int DecorOrder(float y) => -Mathf.RoundToInt(y * 20f) - 4;

        // ------------------------------------------------------------------ loop
        void Update()
        {
            if (PlayerInvulnerable > 0f) PlayerInvulnerable -= Time.deltaTime;

            switch (State)
            {
                case GameState.Playing:
                    if (InInterior) CurrentRoom?.Tick();
                    else UpdateOverworld();
                    if (Input.GetKeyDown(KeyCode.Escape)) Pause();
                    break;
                case GameState.Note:
                    if (Time.unscaledTime > inputLockUntil &&
                        (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Escape)))
                        CloseNote();
                    break;
                case GameState.QTE:
                    UpdateQTE();
                    break;
                case GameState.Choice:
                    if (Input.GetKeyDown(KeyCode.Alpha1)) ChooseEnding(0);
                    else if (Input.GetKeyDown(KeyCode.Alpha2)) ChooseEnding(1);
                    else if (Input.GetKeyDown(KeyCode.Escape)) CloseChoices();
                    break;
                case GameState.Paused:
                    if (Input.GetKeyDown(KeyCode.Escape)) Resume();
                    break;
                case GameState.Intro:
                    if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)) AdvanceIntro();
                    break;
            }
        }

        void LateUpdate()
        {
            if (InInterior || Cam2D == null) return;

            Vector3 target;
            if (State == GameState.Menu)
            {
                float t = Time.unscaledTime;
                target = new Vector3(Mathf.Sin(t * 0.11f) * 7f, 2.4f + Mathf.Cos(t * 0.083f) * 3.2f, -10f);
            }
            else
            {
                target = new Vector3(PlayerPos.x, PlayerPos.y + 0.9f, -10f);
            }

            Vector3 pos = Vector3.Lerp(Cam2D.transform.position, target,
                1f - Mathf.Exp(-8f * Time.unscaledDeltaTime));

            float halfH = Cam2D.orthographicSize;
            float halfW = halfH * Cam2D.aspect;
            float minX = -26f + halfW, maxX = 26f - halfW;
            float minY = -20f + halfH, maxY = 20f - halfH;
            pos.x = minX > maxX ? 0f : Mathf.Clamp(pos.x, minX, maxX);
            pos.y = minY > maxY ? 0f : Mathf.Clamp(pos.y, minY, maxY);
            pos.z = -10f;

            if (shakeTime > 0f)
            {
                shakeTime -= Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(shakeTime / 0.35f);
                pos += (Vector3)(UnityEngine.Random.insideUnitCircle * shakeAmount * k);
            }
            Cam2D.transform.position = pos;
        }

        void Snap2DCamera(float yOffset)
        {
            if (Cam2D == null || Player == null) return;
            float halfH = Cam2D.orthographicSize;
            float halfW = halfH * Cam2D.aspect;
            float x = Mathf.Clamp(PlayerPos.x, -26f + halfW, 26f - halfW);
            float y = Mathf.Clamp(PlayerPos.y + yOffset, -20f + halfH, 20f - halfH);
            Cam2D.transform.position = new Vector3(x, y, -10f);
        }

        void UpdateOverworld()
        {
            string prompt = null;
            Action action = null;
            float best = 999f;
            Vector2 p = PlayerPos;

            foreach (var h in Houses)
            {
                float d = Vector2.Distance(p, h.DoorPoint);
                if (d < 3.1f && d < best)
                {
                    int idx = h.RoomIndex;
                    prompt = "E — войти: " + h.Title;
                    action = () => EnterHouse(idx);
                    best = d;
                }
            }

            foreach (var m in Medkits)
            {
                if (m.Taken) continue;
                float d = Vector2.Distance(p, m.Pos);
                if (d < 2.2f && d < best)
                {
                    prompt = "E — использовать аптечку";
                    action = () => UseMedkit(m);
                    best = d;
                }
            }

            if (Knife != null && !Knife.Taken)
            {
                float d = Vector2.Distance(p, Knife.Pos);
                if (d < 2.6f && d < best)
                {
                    prompt = "E — взять ритуальный нож";
                    action = TakeKnife;
                    best = d;
                }
            }

            float da = Vector2.Distance(p, AltarPos);
            if (da < 3.6f && da < best)
            {
                prompt = Crystals >= CrystalsNeeded ? "E — подойти к алтарю" : "Алтарь спит · нужно 10 кристаллов";
                action = UseAltar;
                best = da;
            }

            if (!string.IsNullOrEmpty(prompt))
            {
                UI.SetPrompt(prompt);
                if (Input.GetKeyDown(KeyCode.E) && action != null) action();
            }
            else UI.SetPrompt(null);
        }

        // ------------------------------------------------------------------ health / counters
        bool Damage(float amount)
        {
            Health = Mathf.Max(0f, Health - amount);
            UI.UpdateHealth(Health);
            UI.FlashDamage();
            Audio.Hurt();
            Shake(0.32f, 0.24f);
            if (Health <= 0f)
            {
                Die();
                return true;
            }
            return false;
        }

        void Shake(float time, float amount)
        {
            if (InInterior) return;
            shakeTime = time;
            shakeAmount = amount;
        }

        public void OnMonsterGrab(Monster2D monster)
        {
            if (State != GameState.Playing || InInterior) return;
            if (PlayerInvulnerable > 0f) return;

            State = GameState.QTE;
            qtePresses = 0;
            qteNeed = 12;
            qteElapsed = 0f;
            UI.SetPrompt(null);
            monster.BeginHold(PlayerPos);
            if (Damage(MaxHealth / 3f)) return;
            Audio.Growl();
            UI.ShowQTE(qteNeed);
            UI.UpdateQTE(0, qteNeed);
        }

        void UpdateQTE()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                qtePresses++;
                Audio.Click();
                UI.UpdateQTE(qtePresses, qteNeed);
                if (qtePresses >= qteNeed)
                {
                    EndGrab(true);
                    return;
                }
            }

            qteElapsed += Time.unscaledDeltaTime;
            UI.UpdateQTETimer(1f - Mathf.Clamp01(qteElapsed / QteTime));
            if (qteElapsed >= QteTime) EndGrab(false);
        }

        void EndGrab(bool escaped)
        {
            UI.HideQTE();
            State = GameState.Playing;
            PlayerInvulnerable = escaped ? 2.6f : 2.2f;

            if (escaped)
            {
                Monster.Release(true);
                Audio.Success();
                UI.Toast("Ты вырвался из хватки!");
            }
            else
            {
                Monster.Release(false);
                Audio.Fail();
                UI.Toast("Монстр отшвырнул тебя...");
                if (Damage(MaxHealth / 3f)) return;
            }
        }

        void Die()
        {
            State = GameState.Dead;
            UI.HideQTE();
            UI.HideNote();
            UI.HideChoices();
            UI.ShowDeath();
            Audio.Ending();
            UI.SetPrompt(null);
        }

        void UseMedkit(Medkit2D medkit)
        {
            if (medkit == null || medkit.Taken) return;
            if (Health >= MaxHealth - 0.01f)
            {
                UI.Toast("Здоровье и так полное");
                return;
            }
            Health = Mathf.Min(MaxHealth, Health + MaxHealth * 2f / 3f);
            medkit.Take();
            UI.UpdateHealth(Health);
            UI.FlashHeal();
            UI.Toast("Аптечка: восстановлено 2/3 здоровья");
        }

        // ------------------------------------------------------------------ collectibles
        public void CollectCrystal()
        {
            if (State != GameState.Playing) return;
            Crystals++;
            Audio.Pickup();
            if (Crystals >= CrystalsNeeded)
            {
                FinaleMode = true;
                AltarObj.SetAwake(true);
                if (Monster != null) Monster.FinaleMode = true;
                UI.Toast("Кристаллы собраны. Алтарь пробудился на севере.");
            }
            else UI.Toast("Кристалл памяти: " + Crystals + " / 10");
            RefreshHud();
        }

        public void CollectNote(int index)
        {
            if (State != GameState.Playing) return;
            if (index < 0 || index >= notesRead.Length || notesRead[index]) return;
            notesRead[index] = true;
            NotesCount++;
            Audio.Note();
            State = GameState.Note;
            inputLockUntil = Time.unscaledTime + 0.4f;
            UI.ShowNote(index);
            RefreshHud();

            if (NotesCount >= NotesNeeded)
            {
                SpawnKnife();
                pendingToast = "Все записки прочитаны. Ритуальный нож появился у алтаря.";
            }
        }

        void CloseNote()
        {
            if (State != GameState.Note) return;
            State = GameState.Playing;
            UI.HideNote();
            if (!string.IsNullOrEmpty(pendingToast))
            {
                UI.Toast(pendingToast);
                pendingToast = null;
            }
        }

        void SpawnKnife()
        {
            if (Knife != null) return;
            var go = new GameObject("RitualKnife");
            go.transform.SetParent(worldRoot, false);
            Knife = go.AddComponent<KnifePickup>();
            Knife.Init(this, new Vector2(2.8f, 15.2f));
        }

        void TakeKnife()
        {
            if (Knife == null || Knife.Taken) return;
            Knife.Take();
            HasKnife = true;
            Audio.Pickup();
            UI.Toast("Ритуальный нож у тебя. Алтарь ждёт.");
            RefreshHud();
        }

        // ------------------------------------------------------------------ altar / endings
        void UseAltar()
        {
            if (State != GameState.Playing) return;
            if (Crystals < CrystalsNeeded)
            {
                Audio.Click();
                UI.Toast("Алтарь спит. Ему нужно 10 кристаллов (" + Crystals + "/10).");
                return;
            }
            Audio.Click();
            State = GameState.Choice;
            UI.SetPrompt(null);
            UI.ShowChoices(NotesCount >= NotesNeeded && HasKnife,
                NotesCount, HasKnife);
        }

        void CloseChoices()
        {
            if (State != GameState.Choice) return;
            State = GameState.Playing;
            UI.HideChoices();
        }

        public void ChooseEnding(int which)
        {
            if (State != GameState.Choice) return;
            if (which == 1 && !(NotesCount >= NotesNeeded && HasKnife))
            {
                UI.Toast("Нужны все 10 записок и ритуальный нож.");
                return;
            }

            State = GameState.Ending;
            UI.HideChoices();
            Audio.Ending();
            UI.SetPrompt(null);
            if (which == 0) UI.ShowEnding("ВИРУС ПОБЕДИЛ", NoteData.EndingVirus, false);
            else UI.ShowEnding("ЧЕЛОВЕК ПРОСНУЛСЯ", NoteData.EndingSacrifice, true);
        }

        // ------------------------------------------------------------------ house transitions
        void EnterHouse(int index)
        {
            if (State != GameState.Playing || InInterior) return;
            if (index < 0 || index >= Rooms.Count) return;
            StartCoroutine(EnterHouseRoutine(index));
        }

        IEnumerator EnterHouseRoutine(int index)
        {
            State = GameState.Transition;
            UI.SetPrompt(null);
            Audio.Door();
            UI.FadeTo(1f, 0.25f);
            yield return new WaitForSecondsRealtime(0.3f);

            Player.gameObject.SetActive(false);
            Cam2D.enabled = false;
            Cam3D.enabled = true;

            var room = Rooms[index];
            room.Enter();
            InInterior = true;
            CurrentRoom = room;
            State = GameState.Playing;
            UI.SetHudVisible(true);
            UI.FadeTo(0f, 0.4f);
            UI.Toast("Дом: " + room.Title);
        }

        public void ExitHouse()
        {
            if (State != GameState.Playing || !InInterior) return;
            StartCoroutine(ExitHouseRoutine());
        }

        IEnumerator ExitHouseRoutine()
        {
            State = GameState.Transition;
            UI.SetPrompt(null);
            Audio.Door();
            UI.FadeTo(1f, 0.25f);
            yield return new WaitForSecondsRealtime(0.3f);

            var room = CurrentRoom;
            if (room != null) room.Leave();
            Cam3D.enabled = false;
            Cam2D.enabled = true;
            Player.gameObject.SetActive(true);

            var house = Houses.Find(h => h.RoomIndex == (room != null ? room.Index : 0));
            Vector2 exitPos = house != null ? house.DoorPoint + new Vector2(0f, -1.35f) : new Vector2(0f, -1.5f);
            Player.Teleport(exitPos);
            InInterior = false;
            CurrentRoom = null;
            PlayerInvulnerable = 2f;
            State = GameState.Playing;
            Snap2DCamera(0.9f);
            UI.FadeTo(0f, 0.4f);
            UI.Toast("Ты снова в пустоши");
        }

        // ------------------------------------------------------------------ menu / state
        public void StartNewGame()
        {
            introPage = 0;
            State = GameState.Intro;
            UI.HideMenu();
            UI.SetHudVisible(false);
            UI.ShowIntro(0);
        }

        public void AdvanceIntro()
        {
            if (State != GameState.Intro) return;
            introPage++;
            Audio.Click();
            if (introPage >= NoteData.IntroPages.Length)
            {
                BeginRun();
            }
            else UI.ShowIntro(introPage);
        }

        public void BeginRun()
        {
            State = GameState.Playing;
            UI.HideIntro();
            UI.SetHudVisible(true);
            RefreshHud();
            UI.Toast("Собери 10 кристаллов и 10 записок");
        }

        public void Pause()
        {
            if (State != GameState.Playing) return;
            State = GameState.Paused;
            UI.ShowPause();
        }

        public void Resume()
        {
            if (State != GameState.Paused) return;
            State = GameState.Playing;
            UI.HidePause();
        }

        public void RestartRun()
        {
            if (VirusBootstrap.Instance != null) VirusBootstrap.Instance.RestartGame(true);
        }

        public void ReturnToMenu()
        {
            if (VirusBootstrap.Instance != null) VirusBootstrap.Instance.RestartGame(false);
        }

        public void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        void RefreshHud()
        {
            UI.UpdateHealth(Health);
            UI.UpdateCounters(Health, Crystals, NotesCount, HasKnife);
            UI.SetObjective(ObjectiveText());
        }

        string ObjectiveText()
        {
            if (Crystals < CrystalsNeeded) return "Собери кристаллы памяти: " + Crystals + " / 10";
            if (NotesCount < NotesNeeded) return "Алтарь пробуждён. Осталось записок: " + (NotesNeeded - NotesCount);
            if (!HasKnife) return "Все записки собраны. Забери нож у алтаря";
            return "Иди к алтарю на севере и сделай выбор";
        }

        public void ShowPrompt(string text) => UI.SetPrompt(text);

        // ------------------------------------------------------------------ collision
        public void ResolveWorld(ref Vector2 p, float radius)
        {
            p.x = Mathf.Clamp(p.x, -25.6f, 25.6f);
            p.y = Mathf.Clamp(p.y, -19.6f, 19.6f);

            foreach (var h in Houses)
            {
                Rect r = h.Body;
                float minX = r.xMin - radius, maxX = r.xMax + radius;
                float minY = r.yMin - radius, maxY = r.yMax + radius;
                if (p.x > minX && p.x < maxX && p.y > minY && p.y < maxY)
                {
                    float dl = p.x - minX, dr = maxX - p.x;
                    float db = p.y - minY, dt = maxY - p.y;
                    float m = Mathf.Min(Mathf.Min(dl, dr), Mathf.Min(db, dt));
                    if (m == dl) p.x = minX;
                    else if (m == dr) p.x = maxX;
                    else if (m == db) p.y = minY;
                    else p.y = maxY;
                }
            }
        }

        // ------------------------------------------------------------------ data
        struct HouseDef
        {
            public float X, Y;
            public bool Snow;
            public int Room;
            public HouseDef(float x, float y, bool snow, int room) { X = x; Y = y; Snow = snow; Room = room; }
        }
    }
}
