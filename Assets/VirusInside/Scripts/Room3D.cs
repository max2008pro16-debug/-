using System.Collections.Generic;
using UnityEngine;

namespace VirusInside
{
    /// <summary>Трёхмерная комната внутри дома. У каждого дома своя тема и свой набор предметов.</summary>
    public class Room3D : MonoBehaviour
    {
        public int Index;
        public string Title;
        public Player3D Avatar;
        public Vector3 SpawnPoint;
        public Vector3 ExitPoint;
        public List<Collectible3D> Items = new List<Collectible3D>();

        VirusGame game;
        Camera cam;
        int noteA, noteB;
        float flicker;

        public void Build(VirusGame g, int index, string title, int firstNote, int secondNote,
            Color wallColor, Color floorColor, Color accent, Camera roomCamera)
        {
            game = g;
            cam = roomCamera;
            Index = index;
            Title = title;
            noteA = firstNote;
            noteB = secondNote;
            name = "Room_" + index;

            var wallMat = VirusArt.Mat3D(wallColor, WallTex(), 0.05f);
            var floorMat = VirusArt.Mat3D(floorColor, CrateTex(), 0.08f);
            var trimMat = VirusArt.Mat3D(accent * 0.55f, null, 0.2f);
            var darkMat = VirusArt.Mat3D(new Color(0.08f, 0.09f, 0.10f), null, 0.1f);
            var woodMat = VirusArt.Mat3D(new Color(0.55f, 0.42f, 0.30f), CrateTex(), 0.1f);
            var clothMat = VirusArt.Mat3D(accent * 0.35f, null, 0.05f);
            var glowMat = VirusArt.MatUnlit(accent);
            var warmMat = VirusArt.Mat3D(new Color(0.9f, 0.55f, 0.25f), null, 0.1f,
                new Color(1.4f, 0.55f, 0.15f));

            // -------- пол и стены --------
            Box("Floor", new Vector3(0f, -0.15f, 0f), new Vector3(16f, 0.3f, 12f), floorMat);
            Box("WallN", new Vector3(0f, 1.5f, 5.85f), new Vector3(16f, 3f, 0.3f), wallMat);
            Box("WallE", new Vector3(7.85f, 1.5f, 0f), new Vector3(0.3f, 3f, 12f), wallMat);
            Box("WallW", new Vector3(-7.85f, 1.5f, 0f), new Vector3(0.3f, 3f, 12f), wallMat);
            Box("WallS_L", new Vector3(-4.65f, 0.6f, -5.85f), new Vector3(6.7f, 1.2f, 0.3f), wallMat);
            Box("WallS_R", new Vector3(4.65f, 0.6f, -5.85f), new Vector3(6.7f, 1.2f, 0.3f), wallMat);

            // -------- плинтус и балки --------
            Box("TrimN", new Vector3(0f, 0.14f, 5.62f), new Vector3(15.4f, 0.28f, 0.18f), trimMat);
            Box("TrimE", new Vector3(7.62f, 0.14f, 0f), new Vector3(0.18f, 0.28f, 11.4f), trimMat);
            Box("TrimW", new Vector3(-7.62f, 0.14f, 0f), new Vector3(0.18f, 0.28f, 11.4f), trimMat);
            Box("BeamN", new Vector3(0f, 2.92f, 5.7f), new Vector3(15.6f, 0.18f, 0.3f), darkMat);
            Box("BeamE", new Vector3(7.7f, 2.92f, 0f), new Vector3(0.3f, 0.18f, 11.6f), darkMat);
            Box("BeamW", new Vector3(-7.7f, 2.92f, 0f), new Vector3(0.3f, 0.18f, 11.6f), darkMat);
            Box("PillarNE", new Vector3(7.5f, 1.5f, 5.5f), new Vector3(0.55f, 3f, 0.55f), trimMat);
            Box("PillarNW", new Vector3(-7.5f, 1.5f, 5.5f), new Vector3(0.55f, 3f, 0.55f), trimMat);

            // -------- дверной проём --------
            Box("DoorL", new Vector3(-1.62f, 1.5f, -5.85f), new Vector3(0.22f, 3f, 0.34f), darkMat);
            Box("DoorR", new Vector3(1.62f, 1.5f, -5.85f), new Vector3(0.22f, 3f, 0.34f), darkMat);
            Box("DoorTop", new Vector3(0f, 2.95f, -5.85f), new Vector3(3.5f, 0.2f, 0.34f), darkMat);
            var exitGlow = Box("ExitGlow", new Vector3(0f, 1.35f, -6.02f), new Vector3(2.5f, 2.6f, 0.05f), glowMat);
            exitGlow.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            // -------- ковёр, мебель --------
            Box("Rug", new Vector3(0f, 0.02f, -0.6f), new Vector3(6.4f, 0.04f, 4.2f), clothMat);
            Box("RugInner", new Vector3(0f, 0.035f, -0.6f), new Vector3(5.4f, 0.04f, 3.2f), VirusArt.Mat3D(accent * 0.5f, null, 0.05f));

            // стол
            Box("TableTop", new Vector3(0f, 0.92f, 3.7f), new Vector3(2.6f, 0.14f, 1.3f), woodMat);
            Box("TableLeg1", new Vector3(-1.05f, 0.45f, 3.2f), new Vector3(0.13f, 0.9f, 0.13f), darkMat);
            Box("TableLeg2", new Vector3(1.05f, 0.45f, 3.2f), new Vector3(0.13f, 0.9f, 0.13f), darkMat);
            Box("TableLeg3", new Vector3(-1.05f, 0.45f, 4.2f), new Vector3(0.13f, 0.9f, 0.13f), darkMat);
            Box("TableLeg4", new Vector3(1.05f, 0.45f, 4.2f), new Vector3(0.13f, 0.9f, 0.13f), darkMat);

            // стеллаж
            Box("Shelf", new Vector3(0f, 1.05f, 5.55f), new Vector3(3.4f, 2.1f, 0.4f), woodMat);
            for (int i = 0; i < 3; i++)
                Box("ShelfBoard" + i, new Vector3(0f, 0.55f + i * 0.55f, 5.3f), new Vector3(3.2f, 0.08f, 0.4f), darkMat);
            for (int i = 0; i < 4; i++)
                Box("Book" + i, new Vector3(-1.2f + i * 0.7f, 0.75f + (i % 3) * 0.55f, 5.3f),
                    new Vector3(0.26f, 0.42f, 0.28f), VirusArt.Mat3D(accent * (0.5f + i * 0.1f), null, 0.1f));

            // ящики
            Box("Crate1", new Vector3(-6.5f, 0.5f, -4.4f), Vector3.one * 1.0f, woodMat);
            Box("Crate2", new Vector3(-6.3f, 0.35f, -3.2f), Vector3.one * 0.7f, woodMat);
            Box("Crate3", new Vector3(-5.4f, 0.55f, -4.6f), Vector3.one * 0.55f, woodMat);

            // камин
            Box("Fireplace", new Vector3(7.55f, 0.9f, 2.6f), new Vector3(0.6f, 1.8f, 2.2f), darkMat);
            var fire = Box("Fire", new Vector3(7.2f, 0.75f, 2.6f), new Vector3(0.08f, 0.8f, 1.4f), warmMat);
            fire.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            // окна
            Box("WindowN1", new Vector3(-4.2f, 1.75f, 5.67f), new Vector3(1.8f, 1.1f, 0.06f), glowMat);
            Box("WindowN2", new Vector3(4.2f, 1.75f, 5.67f), new Vector3(1.8f, 1.1f, 0.06f), glowMat);
            Box("WindowFrame1", new Vector3(-4.2f, 1.75f, 5.64f), new Vector3(2.0f, 1.3f, 0.05f), darkMat);
            Box("WindowFrame2", new Vector3(4.2f, 1.75f, 5.64f), new Vector3(2.0f, 1.3f, 0.05f), darkMat);
            Box("WindowN1b", new Vector3(-4.2f, 1.75f, 5.66f), new Vector3(1.7f, 1.05f, 0.06f), glowMat);
            Box("WindowN2b", new Vector3(4.2f, 1.75f, 5.66f), new Vector3(1.7f, 1.05f, 0.06f), glowMat);

            // -------- свет --------
            var lightGo = new GameObject("RoomLight");
            lightGo.transform.SetParent(transform, false);
            lightGo.transform.localPosition = new Vector3(0f, 2.7f, 1.2f);
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.86f, 0.66f);
            light.range = 14f;
            light.intensity = 2.2f;
            light.shadows = LightShadows.None;

            var fireLightGo = new GameObject("FireLight");
            fireLightGo.transform.SetParent(transform, false);
            fireLightGo.transform.localPosition = new Vector3(6.6f, 1.2f, 2.6f);
            var fireLight = fireLightGo.AddComponent<Light>();
            fireLight.type = LightType.Point;
            fireLight.color = new Color(1f, 0.45f, 0.18f);
            fireLight.range = 5f;
            fireLight.intensity = 1.5f;
            fireLight.shadows = LightShadows.None;

            var exitLightGo = new GameObject("ExitLight");
            exitLightGo.transform.SetParent(transform, false);
            exitLightGo.transform.localPosition = new Vector3(0f, 1.7f, -5.4f);
            var exitLight = exitLightGo.AddComponent<Light>();
            exitLight.type = LightType.Point;
            exitLight.color = new Color(0.35f, 1f, 0.7f);
            exitLight.range = 4.5f;
            exitLight.intensity = 1.1f;
            exitLight.shadows = LightShadows.None;

            // -------- игрок --------
            var playerGo = new GameObject("Player3D");
            playerGo.transform.SetParent(transform, false);
            Avatar = playerGo.AddComponent<Player3D>();
            Avatar.Init(this, game, cam, new Vector3(0f, 0.1f, -4.3f));
            SpawnPoint = new Vector3(0f, 0.1f, -4.3f);
            ExitPoint = new Vector3(0f, 0f, -5.2f);

            // -------- предметы --------
            SpawnCrystal(new Vector3(-5.4f, 0f, 1.9f), floorMat, accent);
            SpawnCrystal(new Vector3(5.4f, 0f, 2.5f), floorMat, accent);
            SpawnNote(new Vector3(-4.6f, 0f, -3.7f), noteA, accent);
            SpawnNote(new Vector3(4.9f, 0f, -3.2f), noteB, accent);

            VirusArt.SetLayerRecursively(gameObject, VirusGame.InteriorLayer);
        }

        Texture2D WallTex()
        {
            var t = Resources.Load<Texture2D>("Art/tex_wall");
            if (t != null) { t.wrapMode = TextureWrapMode.Repeat; t.filterMode = FilterMode.Point; }
            return t;
        }

        Texture2D CrateTex()
        {
            var t = Resources.Load<Texture2D>("Art/tex_crate");
            if (t != null) { t.wrapMode = TextureWrapMode.Repeat; t.filterMode = FilterMode.Point; }
            return t;
        }

        GameObject Box(string boxName, Vector3 pos, Vector3 scale, Material mat)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = boxName;
            go.transform.SetParent(transform, false);
            go.transform.localPosition = pos;
            go.transform.localScale = scale;
            var mr = go.GetComponent<MeshRenderer>();
            mr.sharedMaterial = mat;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            return go;
        }

        void SpawnCrystal(Vector3 floorPos, Material pedestalMat, Color accent)
        {
            var pedestal = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pedestal.name = "Pedestal";
            pedestal.transform.SetParent(transform, false);
            pedestal.transform.localPosition = floorPos + new Vector3(0f, 0.2f, 0f);
            pedestal.transform.localScale = new Vector3(0.85f, 0.2f, 0.85f);
            var pr = pedestal.GetComponent<MeshRenderer>();
            pr.sharedMaterial = pedestalMat;

            var go = new GameObject("Crystal");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = floorPos + new Vector3(0f, 1.2f, 0f);
            var mf = go.AddComponent<MeshFilter>();
            mf.sharedMesh = VirusArt.Octahedron();
            var mr = go.AddComponent<MeshRenderer>();
            var crystalMat = VirusArt.Mat3D(new Color(0.35f, 0.95f, 0.9f), null, 0.7f,
                new Color(0.2f, 1.2f, 1.1f));
            mr.sharedMaterial = crystalMat;
            go.transform.localScale = new Vector3(0.42f, 0.7f, 0.42f);

            var lightGo = new GameObject("CrystalLight");
            lightGo.transform.SetParent(go.transform, false);
            var l = lightGo.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = new Color(0.4f, 1f, 0.95f);
            l.range = 4.5f;
            l.intensity = 1.6f;
            l.shadows = LightShadows.None;

            var c = go.AddComponent<Collectible3D>();
            c.Init(game, Avatar, false, -1, go.transform);
            Items.Add(c);
        }

        void SpawnNote(Vector3 floorPos, int noteIndex, Color accent)
        {
            var go = new GameObject("Note_" + noteIndex);
            go.transform.SetParent(transform, false);
            go.transform.localPosition = floorPos + new Vector3(0f, 1.15f, 0f);

            BoxOn(go, "Paper", Vector3.zero, new Vector3(0.55f, 0.78f, 0.045f),
                VirusArt.Mat3D(new Color(0.92f, 0.90f, 0.80f), null, 0.05f));
            BoxOn(go, "Seal", new Vector3(0.17f, -0.25f, 0.035f), new Vector3(0.16f, 0.16f, 0.03f),
                VirusArt.Mat3D(new Color(0.65f, 0.12f, 0.12f), null, 0.1f));
            BoxOn(go, "Ribbon", new Vector3(0f, 0.34f, 0.03f), new Vector3(0.58f, 0.06f, 0.02f),
                VirusArt.Mat3D(accent, null, 0.1f));
            go.transform.localRotation = Quaternion.Euler(0f, 18f, 0f);

            var c = go.AddComponent<Collectible3D>();
            c.Init(game, Avatar, true, noteIndex, go.transform);
            Items.Add(c);
        }

        void BoxOn(GameObject parent, string boxName, Vector3 localPos, Vector3 scale, Material mat)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = boxName;
            go.transform.SetParent(parent.transform, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = scale;
            var mr = go.GetComponent<MeshRenderer>();
            mr.sharedMaterial = mat;
            var col = go.GetComponent<Collider>();
            if (col != null) Object.Destroy(col);
        }

        public void Enter()
        {
            gameObject.SetActive(true);
            Avatar.Teleport(SpawnPoint);
            Avatar.SnapCamera();
        }

        public void Leave()
        {
            gameObject.SetActive(false);
        }

        public void Tick()
        {
            if (Avatar == null) return;
            Vector3 p = Avatar.transform.position;
            Vector2 flat = new Vector2(p.x, p.z);
            Vector2 exit = new Vector2(ExitPoint.x, ExitPoint.z);
            if (Vector2.Distance(flat, exit) < 2.3f)
            {
                game.ShowPrompt("E — выйти из дома");
                if (Input.GetKeyDown(KeyCode.E)) game.ExitHouse();
            }
        }
    }

    /// <summary>Герой в трёхмерной комнате. Простое управление и follow-камера.</summary>
    public class Player3D : MonoBehaviour
    {
        VirusGame game;
        Camera cam;
        CharacterController controller;
        Transform model;
        float bob;

        public void Init(Room3D room, VirusGame g, Camera roomCamera, Vector3 spawn)
        {
            game = g;
            cam = roomCamera;
            transform.localPosition = spawn;

            controller = gameObject.AddComponent<CharacterController>();
            controller.height = 1.75f;
            controller.radius = 0.34f;
            controller.center = new Vector3(0f, 0.9f, 0f);
            controller.slopeLimit = 50f;
            controller.stepOffset = 0.32f;

            var body = new GameObject("Model");
            body.transform.SetParent(transform, false);
            model = body.transform;

            Part(PrimitiveType.Capsule, "Body", new Vector3(0f, 0.85f, 0f), new Vector3(0.62f, 0.62f, 0.52f),
                new Color(0.20f, 0.42f, 0.40f));
            Part(PrimitiveType.Sphere, "Head", new Vector3(0f, 1.62f, 0f), new Vector3(0.46f, 0.46f, 0.46f),
                new Color(0.94f, 0.72f, 0.52f));
            Part(PrimitiveType.Sphere, "Hair", new Vector3(0f, 1.70f, -0.02f), new Vector3(0.50f, 0.40f, 0.50f),
                new Color(0.07f, 0.07f, 0.08f));
            Part(PrimitiveType.Cube, "Scarf", new Vector3(0f, 1.30f, 0f), new Vector3(0.46f, 0.12f, 0.34f),
                new Color(0.65f, 0.22f, 0.22f));
            Part(PrimitiveType.Cube, "Bag", new Vector3(-0.34f, 0.85f, -0.18f), new Vector3(0.18f, 0.34f, 0.22f),
                new Color(0.33f, 0.25f, 0.18f));

            var lightGo = new GameObject("PlayerLight");
            lightGo.transform.SetParent(transform, false);
            lightGo.transform.localPosition = new Vector3(0f, 1.6f, 0f);
            var l = lightGo.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = new Color(0.75f, 0.9f, 1f);
            l.range = 5f;
            l.intensity = 0.55f;
            l.shadows = LightShadows.None;

            VirusArt.SetLayerRecursively(gameObject, VirusGame.InteriorLayer);
        }

        void Part(PrimitiveType type, string partName, Vector3 localPos, Vector3 scale, Color color)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = partName;
            go.transform.SetParent(model, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = scale;
            var col = go.GetComponent<Collider>();
            if (col != null) Object.Destroy(col);
            go.GetComponent<MeshRenderer>().sharedMaterial =
                VirusArt.Mat3D(color, null, 0.1f);
        }

        public void Teleport(Vector3 pos)
        {
            if (controller != null) controller.enabled = false;
            transform.position = pos;
            if (controller != null) controller.enabled = true;
            if (cam != null) SnapCamera();
        }

        public void SnapCamera()
        {
            if (cam == null) return;
            cam.transform.position = transform.position + new Vector3(0f, 8.8f, -6.2f);
            cam.transform.LookAt(transform.position + new Vector3(0f, 0.9f, 0f));
        }

        void Update()
        {
            if (game == null || controller == null) return;
            if (!game.CanPlay)
            {
                bob = Mathf.Lerp(bob, 0f, Time.deltaTime * 6f);
                model.localPosition = new Vector3(0f, bob, 0f);
                return;
            }

            float x = 0f, z = 0f;
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) z += 1f;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) z -= 1f;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) x -= 1f;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) x += 1f;

            Vector3 dir = new Vector3(x, 0f, z);
            if (dir.sqrMagnitude > 0.01f)
            {
                dir.Normalize();
                bool sprint = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
                float speed = sprint ? 7.4f : 5.2f;
                controller.Move(dir * speed * Time.deltaTime);
                var look = Quaternion.LookRotation(dir, Vector3.up);
                model.rotation = Quaternion.Slerp(model.rotation, look, 1f - Mathf.Exp(-14f * Time.deltaTime));
                bob = Mathf.Abs(Mathf.Sin(Time.time * (sprint ? 14f : 10f))) * 0.055f;
                game.Audio.Step();
            }
            else
            {
                bob = Mathf.Lerp(bob, 0f, Time.deltaTime * 8f);
            }
            model.localPosition = new Vector3(0f, bob, 0f);

            if (transform.position.y < -2f) Teleport(new Vector3(0f, 0.1f, -4.3f));
        }

        void LateUpdate()
        {
            if (cam == null || game == null || !game.InInterior) return;
            Vector3 desired = transform.position + new Vector3(0f, 8.8f, -6.2f);
            cam.transform.position = Vector3.Lerp(cam.transform.position, desired,
                1f - Mathf.Exp(-7f * Time.deltaTime));
            cam.transform.LookAt(transform.position + new Vector3(0f, 0.9f, 0f));
        }
    }

    /// <summary>Кристалл или записка внутри 3D-комнаты.</summary>
    public class Collectible3D : MonoBehaviour
    {
        VirusGame game;
        Player3D player;
        bool isNote;
        int noteIndex;
        Transform visual;
        float t;
        float baseY;
        bool collected;

        public void Init(VirusGame g, Player3D p, bool note, int index, Transform vis)
        {
            game = g;
            player = p;
            isNote = note;
            noteIndex = index;
            visual = vis;
            baseY = transform.localPosition.y;
            t = Random.Range(0f, 6f);
        }

        void Update()
        {
            if (game == null || player == null || collected) return;
            t += Time.deltaTime;
            if (isNote)
            {
                transform.localPosition = new Vector3(transform.localPosition.x,
                    baseY + Mathf.Sin(t * 1.8f) * 0.08f, transform.localPosition.z);
                transform.localRotation = Quaternion.Euler(0f, 18f + t * 26f, 0f);
            }
            else
            {
                transform.localPosition = new Vector3(transform.localPosition.x,
                    baseY + Mathf.Sin(t * 1.6f) * 0.1f, transform.localPosition.z);
                transform.localRotation = Quaternion.Euler(0f, t * 40f, 0f);
            }

            if (!game.CanPlay) return;
            Vector3 a = player.transform.position;
            Vector3 b = transform.position;
            float dx = a.x - b.x, dz = a.z - b.z;
            if (dx * dx + dz * dz < 1.45f * 1.45f && Mathf.Abs(a.y - b.y) < 2.2f)
            {
                collected = true;
                if (isNote) game.CollectNote(noteIndex);
                else game.CollectCrystal();
                Destroy(gameObject);
            }
        }
    }
}
