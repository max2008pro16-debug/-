using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace VirusInside
{
    /// <summary>
    /// Procedural art + loading of the existing pixel-art sprites.
    /// Everything here works at runtime, so the game needs no prefabs or baked materials.
    /// </summary>
    public static class VirusArt
    {
        public enum Pipeline { Urp, Builtin }

        public static Pipeline Pipeline3D = Pipeline.Urp;

        static readonly Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();
        static Font font;
        static Sprite white;
        static Sprite rounded;
        static Sprite vignette;
        static Sprite shadow;
        static Sprite glow;
        static Sprite snowPatch;
        static Sprite crystalIcon;
        static Sprite medkitIcon;
        static Sprite altarSprite;
        static Sprite knifeSprite;
        static Sprite noteIcon;
        static Sprite doorMark;
        static Sprite treeSnow;
        static Sprite treeSand;
        static Sprite rock;

        // ---------------------------------------------------------------- palette
        public static readonly Color Ink = new Color(0.055f, 0.07f, 0.08f, 1f);
        public static readonly Color Panel = new Color(0.075f, 0.10f, 0.11f, 0.96f);
        public static readonly Color PanelSoft = new Color(0.10f, 0.14f, 0.15f, 0.94f);
        public static readonly Color Ivory = new Color(0.91f, 0.90f, 0.84f, 1f);
        public static readonly Color Dim = new Color(0.62f, 0.65f, 0.63f, 1f);
        public static readonly Color Cyan = new Color(0.42f, 0.90f, 0.83f, 1f);
        public static readonly Color CyanDark = new Color(0.14f, 0.38f, 0.37f, 1f);
        public static readonly Color Gold = new Color(0.89f, 0.70f, 0.36f, 1f);
        public static readonly Color Red = new Color(0.80f, 0.24f, 0.24f, 1f);
        public static readonly Color RedDark = new Color(0.32f, 0.09f, 0.10f, 1f);
        public static readonly Color Green = new Color(0.42f, 0.82f, 0.52f, 1f);

        // ---------------------------------------------------------------- font
        public static Font Font
        {
            get
            {
                if (font != null) return font;
                try
                {
                    font = Font.CreateDynamicFontFromOSFont(
                        new[] { "Segoe UI", "Arial", "Tahoma", "Verdana", "Liberation Sans" }, 28);
                }
                catch { font = null; }

                if (font == null)
                {
                    try { font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); } catch { }
                }
                if (font == null)
                {
                    try { font = Resources.GetBuiltinResource<Font>("Arial.ttf"); } catch { }
                }
                return font;
            }
        }

        // ---------------------------------------------------------------- loading
        /// <summary>Loads a PNG from Resources/Art, trims transparent padding and scales it to a world height.</summary>
        public static Sprite Load(string name, float targetHeight = 0f, float pivotY = 0f)
        {
            string key = name + "#" + targetHeight.ToString("F3") + "#" + pivotY.ToString("F3");
            if (cache.TryGetValue(key, out var cached)) return cached;

            var tex = Resources.Load<Texture2D>("Art/" + name);
            if (tex == null)
            {
                Debug.LogWarning("[VirusInside] missing art: " + name);
                return White;
            }
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Clamp;

            var rect = new Rect(0, 0, tex.width, tex.height);
            if (targetHeight > 0.01f)
            {
                var trimmed = OpaqueBounds(tex);
                if (trimmed.width > 2 && trimmed.height > 2) rect = trimmed;
            }

            float ppu = targetHeight > 0.01f ? rect.height / targetHeight : Mathf.Max(1f, tex.height / 2f);
            var spr = Sprite.Create(tex, rect, new Vector2(0.5f, pivotY), ppu, 0, SpriteMeshType.FullRect);
            spr.name = name;
            cache[key] = spr;
            return spr;
        }

        static Rect OpaqueBounds(Texture2D tex)
        {
            try
            {
                var px = tex.GetPixels32();
                int w = tex.width, h = tex.height;
                int minX = w, minY = h, maxX = -1, maxY = -1;
                for (int y = 0; y < h; y++)
                {
                    int row = y * w;
                    for (int x = 0; x < w; x++)
                    {
                        if (px[row + x].a > 12)
                        {
                            if (x < minX) minX = x;
                            if (x > maxX) maxX = x;
                            if (y < minY) minY = y;
                            if (y > maxY) maxY = y;
                        }
                    }
                }
                if (maxX < minX || maxY < minY) return new Rect(0, 0, w, h);
                int pad = 2;
                minX = Mathf.Max(0, minX - pad);
                minY = Mathf.Max(0, minY - pad);
                maxX = Mathf.Min(w - 1, maxX + pad);
                maxY = Mathf.Min(h - 1, maxY + pad);
                return new Rect(minX, minY, maxX - minX + 1, maxY - minY + 1);
            }
            catch
            {
                return new Rect(0, 0, tex.width, tex.height);
            }
        }

        public static Sprite Ground(string name)
        {
            string key = "ground:" + name;
            if (cache.TryGetValue(key, out var cached)) return cached;
            var tex = Resources.Load<Texture2D>("Art/" + name);
            if (tex == null)
            {
                Debug.LogWarning("[VirusInside] missing ground art: " + name);
                return White;
            }
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Repeat;
            var spr = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f),
                64f, 0, SpriteMeshType.FullRect);
            cache[key] = spr;
            return spr;
        }

        // ---------------------------------------------------------------- ui sprites
        public static Sprite White
        {
            get
            {
                if (white != null) return white;
                var t = Blank(8, 8);
                Fill(t, 0, 0, 8, 8, Color.white);
                t.Apply();
                white = MakeSprite(t, 100f, new Vector2(0.5f, 0.5f));
                return white;
            }
        }

        public static Sprite Rounded
        {
            get
            {
                if (rounded != null) return rounded;
                int s = 64, r = 16;
                var t = Blank(s, s);
                for (int y = 0; y < s; y++)
                {
                    for (int x = 0; x < s; x++)
                    {
                        float dx = Mathf.Max(Mathf.Max(r - x, x - (s - 1 - r)), 0);
                        float dy = Mathf.Max(Mathf.Max(r - y, y - (s - 1 - r)), 0);
                        if (dx * dx + dy * dy <= r * r + 4f) t.SetPixel(x, y, Color.white);
                    }
                }
                t.Apply();
                rounded = Sprite.Create(t, new Rect(0, 0, s, s), new Vector2(0.5f, 0.5f), 100f, 0,
                    SpriteMeshType.FullRect, new Vector4(20, 20, 20, 20));
                return rounded;
            }
        }

        public static Sprite Vignette
        {
            get
            {
                if (vignette != null) return vignette;
                int s = 256;
                var t = new Texture2D(s, s, TextureFormat.RGBA32, false);
                var px = new Color32[s * s];
                for (int y = 0; y < s; y++)
                {
                    for (int x = 0; x < s; x++)
                    {
                        float nx = (x - s * 0.5f) / (s * 0.5f);
                        float ny = (y - s * 0.5f) / (s * 0.5f);
                        float d = Mathf.Sqrt(nx * nx + ny * ny) / 1.42f;
                        float a = Mathf.Clamp01((d - 0.42f) / 0.58f);
                        a = a * a * 0.80f;
                        px[y * s + x] = new Color(0f, 0f, 0f, a);
                    }
                }
                t.SetPixels32(px);
                t.wrapMode = TextureWrapMode.Clamp;
                t.filterMode = FilterMode.Bilinear;
                t.Apply();
                vignette = MakeSprite(t, 100f, new Vector2(0.5f, 0.5f));
                return vignette;
            }
        }

        public static Sprite Shadow
        {
            get
            {
                if (shadow != null) return shadow;
                int w = 64, h = 28;
                var t = new Texture2D(w, h, TextureFormat.RGBA32, false);
                var px = new Color32[w * h];
                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        float nx = (x - w * 0.5f) / (w * 0.5f);
                        float ny = (y - h * 0.5f) / (h * 0.5f);
                        float d = Mathf.Sqrt(nx * nx + ny * ny);
                        float a = Mathf.Clamp01(1f - d) * 0.45f;
                        px[y * w + x] = new Color(0f, 0f, 0f, a * a);
                    }
                }
                t.SetPixels32(px);
                t.wrapMode = TextureWrapMode.Clamp;
                t.filterMode = FilterMode.Bilinear;
                t.Apply();
                shadow = MakeSprite(t, 64f * 2.2f, new Vector2(0.5f, 0.5f));
                return shadow;
            }
        }

        public static Sprite Glow
        {
            get
            {
                if (glow != null) return glow;
                int s = 64;
                var t = new Texture2D(s, s, TextureFormat.RGBA32, false);
                var px = new Color32[s * s];
                for (int y = 0; y < s; y++)
                {
                    for (int x = 0; x < s; x++)
                    {
                        float nx = (x - s * 0.5f) / (s * 0.5f);
                        float ny = (y - s * 0.5f) / (s * 0.5f);
                        float d = Mathf.Clamp01(1f - Mathf.Sqrt(nx * nx + ny * ny));
                        float a = d * d * 0.9f;
                        px[y * s + x] = new Color(1f, 1f, 1f, a);
                    }
                }
                t.SetPixels32(px);
                t.wrapMode = TextureWrapMode.Clamp;
                t.filterMode = FilterMode.Bilinear;
                t.Apply();
                glow = MakeSprite(t, 128f, new Vector2(0.5f, 0.5f));
                return glow;
            }
        }

        public static Sprite SnowPatch
        {
            get
            {
                if (snowPatch != null) return snowPatch;
                int w = 128, h = 80;
                var t = Blank(w, h);
                var c = new Color(0.86f, 0.92f, 0.99f, 0.96f);
                var c2 = new Color(0.72f, 0.83f, 0.94f, 0.7f);
                Ellipse(t, 64, 40, 60, 36, c2);
                Ellipse(t, 58, 42, 54, 32, c);
                Ellipse(t, 20, 36, 22, 18, c);
                Ellipse(t, 108, 34, 18, 15, c);
                t.Apply();
                snowPatch = MakeSprite(t, 128f, new Vector2(0.5f, 0.5f));
                return snowPatch;
            }
        }

        public static Sprite CrystalIcon
        {
            get
            {
                if (crystalIcon != null) return crystalIcon;
                int w = 48, h = 64;
                var t = Blank(w, h);
                var outline = new Color(0.05f, 0.22f, 0.28f, 1f);
                var poly = new[] {
                    new Vector2(24, 62), new Vector2(45, 32), new Vector2(24, 2), new Vector2(3, 32)
                };
                var inner = new[] {
                    new Vector2(24, 56), new Vector2(39, 32), new Vector2(24, 8), new Vector2(9, 32)
                };
                var light = new[] {
                    new Vector2(24, 56), new Vector2(33, 32), new Vector2(24, 14), new Vector2(17, 32)
                };
                FillPoly(t, poly, outline);
                FillPoly(t, inner, Cyan);
                FillPoly(t, light, new Color(0.75f, 1f, 0.96f, 1f));
                Line(t, 24, 56, 24, 14, new Color(0.35f, 0.8f, 0.78f, 1f));
                t.Apply();
                crystalIcon = MakeSprite(t, 100f, new Vector2(0.5f, 0.5f));
                return crystalIcon;
            }
        }

        public static Sprite MedkitIcon
        {
            get
            {
                if (medkitIcon != null) return medkitIcon;
                int w = 48, h = 48;
                var t = Blank(w, h);
                RectFill(t, 3, 6, 42, 38, new Color(0.08f, 0.1f, 0.1f, 1f));
                RectFill(t, 5, 8, 38, 34, new Color(0.92f, 0.93f, 0.89f, 1f));
                RectFill(t, 18, 40, 12, 6, new Color(0.4f, 0.42f, 0.4f, 1f));
                RectFill(t, 20, 43, 8, 3, new Color(0.92f, 0.93f, 0.89f, 1f));
                RectFill(t, 20, 14, 8, 22, Red);
                RectFill(t, 13, 21, 22, 8, Red);
                RectFill(t, 20, 30, 8, 6, new Color(0.95f, 0.45f, 0.42f, 1f));
                t.Apply();
                medkitIcon = MakeSprite(t, 100f, new Vector2(0.5f, 0.5f));
                return medkitIcon;
            }
        }

        public static Sprite Altar
        {
            get
            {
                if (altarSprite != null) return altarSprite;
                int w = 128, h = 112;
                var t = Blank(w, h);
                var stone = new Color(0.24f, 0.29f, 0.31f, 1f);
                var stoneLight = new Color(0.36f, 0.42f, 0.43f, 1f);
                var stoneDark = new Color(0.12f, 0.15f, 0.17f, 1f);
                Ellipse(t, 64, 22, 62, 18, stoneDark);
                Ellipse(t, 64, 26, 58, 16, stone);
                Ellipse(t, 64, 34, 48, 14, stoneLight);
                Ellipse(t, 64, 38, 44, 12, stone);
                RectFill(t, 26, 30, 14, 24, stone);
                RectFill(t, 88, 30, 14, 24, stone);
                RectFill(t, 24, 50, 80, 7, stoneLight);
                Ellipse(t, 64, 72, 26, 20, stoneDark);
                Ellipse(t, 64, 74, 22, 16, stone);
                Ellipse(t, 64, 76, 15, 10, new Color(0.06f, 0.16f, 0.19f, 1f));
                Ellipse(t, 64, 77, 10, 6, CyanDark);
                Ellipse(t, 64, 78, 6, 3, new Color(0.55f, 1f, 0.9f, 1f));
                for (int i = 0; i < 8; i++)
                {
                    float a = i * Mathf.PI * 2f / 8f;
                    int x = 64 + Mathf.RoundToInt(Mathf.Cos(a) * 31f);
                    int y = 38 + Mathf.RoundToInt(Mathf.Sin(a) * 9f);
                    RectFill(t, x, y, 3, 3, i % 2 == 0 ? Cyan : Gold);
                }
                t.Apply();
                altarSprite = MakeSprite(t, 64f, new Vector2(0.5f, 0.08f));
                return altarSprite;
            }
        }

        public static Sprite Knife
        {
            get
            {
                if (knifeSprite != null) return knifeSprite;
                int w = 80, h = 32;
                var t = Blank(w, h);
                var blade = new[] {
                    new Vector2(72, 24), new Vector2(30, 23), new Vector2(22, 16), new Vector2(30, 9), new Vector2(72, 8)
                };
                FillPoly(t, blade, new Color(0.86f, 0.89f, 0.90f, 1f));
                FillPoly(t, new[] {
                    new Vector2(70, 21), new Vector2(34, 20), new Vector2(28, 16), new Vector2(34, 12), new Vector2(70, 11)
                }, new Color(0.58f, 0.65f, 0.68f, 1f));
                RectFill(t, 14, 13, 10, 6, Gold);
                RectFill(t, 2, 12, 13, 8, new Color(0.35f, 0.10f, 0.10f, 1f));
                RectFill(t, 4, 14, 8, 4, new Color(0.55f, 0.18f, 0.16f, 1f));
                t.Apply();
                knifeSprite = MakeSprite(t, 64f, new Vector2(0.5f, 0.5f));
                return knifeSprite;
            }
        }

        public static Sprite NoteIcon
        {
            get
            {
                if (noteIcon != null) return noteIcon;
                int w = 40, h = 52;
                var t = Blank(w, h);
                RectFill(t, 4, 2, 32, 48, new Color(0.08f, 0.1f, 0.1f, 1f));
                RectFill(t, 6, 4, 28, 44, new Color(0.93f, 0.91f, 0.82f, 1f));
                RectFill(t, 9, 39, 22, 3, new Color(0.55f, 0.52f, 0.45f, 1f));
                RectFill(t, 9, 33, 16, 2, new Color(0.65f, 0.62f, 0.54f, 1f));
                RectFill(t, 9, 28, 20, 2, new Color(0.65f, 0.62f, 0.54f, 1f));
                RectFill(t, 9, 23, 14, 2, new Color(0.65f, 0.62f, 0.54f, 1f));
                RectFill(t, 9, 18, 19, 2, new Color(0.65f, 0.62f, 0.54f, 1f));
                RectFill(t, 25, 7, 7, 7, new Color(0.62f, 0.15f, 0.15f, 1f));
                t.Apply();
                noteIcon = MakeSprite(t, 100f, new Vector2(0.5f, 0.5f));
                return noteIcon;
            }
        }

        public static Sprite DoorMark
        {
            get
            {
                if (doorMark != null) return doorMark;
                int s = 64;
                var t = Blank(s, s);
                for (int i = 0; i < 20; i++)
                {
                    float a = i * Mathf.PI * 2f / 20f;
                    int x = 32 + Mathf.RoundToInt(Mathf.Cos(a) * 23f);
                    int y = 32 + Mathf.RoundToInt(Mathf.Sin(a) * 23f);
                    RectFill(t, x - 1, y - 1, 3, 3, new Color(0.95f, 0.78f, 0.42f, 1f));
                }
                FillPoly(t, new[] {
                    new Vector2(32, 14), new Vector2(46, 34), new Vector2(36, 34), new Vector2(36, 48),
                    new Vector2(28, 48), new Vector2(28, 34), new Vector2(18, 34)
                }, new Color(0.98f, 0.84f, 0.48f, 1f));
                t.Apply();
                doorMark = MakeSprite(t, 100f, new Vector2(0.5f, 0.5f));
                return doorMark;
            }
        }

        public static Sprite TreeSnow
        {
            get
            {
                if (treeSnow != null) return treeSnow;
                int w = 64, h = 104;
                var t = Blank(w, h);
                RectFill(t, 27, 2, 10, 34, new Color(0.18f, 0.13f, 0.11f, 1f));
                Ellipse(t, 32, 62, 25, 32, new Color(0.08f, 0.17f, 0.18f, 1f));
                Ellipse(t, 18, 72, 15, 18, new Color(0.08f, 0.17f, 0.18f, 1f));
                Ellipse(t, 46, 72, 15, 18, new Color(0.08f, 0.17f, 0.18f, 1f));
                Ellipse(t, 32, 84, 20, 15, new Color(0.10f, 0.21f, 0.22f, 1f));
                Ellipse(t, 32, 90, 17, 12, new Color(0.86f, 0.92f, 0.99f, 0.95f));
                Ellipse(t, 20, 76, 8, 6, new Color(0.86f, 0.92f, 0.99f, 0.9f));
                Ellipse(t, 45, 74, 8, 6, new Color(0.86f, 0.92f, 0.99f, 0.9f));
                t.Apply();
                treeSnow = MakeSprite(t, 64f, new Vector2(0.5f, 0.04f));
                return treeSnow;
            }
        }

        public static Sprite TreeSand
        {
            get
            {
                if (treeSand != null) return treeSand;
                int w = 64, h = 100;
                var t = Blank(w, h);
                RectFill(t, 28, 2, 8, 40, new Color(0.25f, 0.17f, 0.11f, 1f));
                RectFill(t, 31, 2, 3, 40, new Color(0.34f, 0.24f, 0.15f, 1f));
                Ellipse(t, 32, 62, 22, 28, new Color(0.20f, 0.24f, 0.14f, 1f));
                Ellipse(t, 18, 70, 13, 16, new Color(0.24f, 0.27f, 0.15f, 1f));
                Ellipse(t, 47, 72, 13, 16, new Color(0.17f, 0.21f, 0.12f, 1f));
                Ellipse(t, 32, 82, 16, 12, new Color(0.27f, 0.30f, 0.17f, 1f));
                t.Apply();
                treeSand = MakeSprite(t, 64f, new Vector2(0.5f, 0.04f));
                return treeSand;
            }
        }

        public static Sprite Rock
        {
            get
            {
                if (rock != null) return rock;
                int w = 56, h = 40;
                var t = Blank(w, h);
                FillPoly(t, new[] {
                    new Vector2(4, 8), new Vector2(12, 24), new Vector2(26, 34), new Vector2(46, 30),
                    new Vector2(52, 16), new Vector2(40, 6), new Vector2(18, 2)
                }, new Color(0.18f, 0.20f, 0.21f, 1f));
                FillPoly(t, new[] {
                    new Vector2(9, 10), new Vector2(16, 23), new Vector2(27, 30), new Vector2(42, 26),
                    new Vector2(46, 17), new Vector2(36, 10), new Vector2(20, 6)
                }, new Color(0.33f, 0.36f, 0.37f, 1f));
                Line(t, 20, 24, 34, 26, new Color(0.48f, 0.51f, 0.50f, 1f));
                t.Apply();
                rock = MakeSprite(t, 64f, new Vector2(0.5f, 0.1f));
                return rock;
            }
        }

        // ---------------------------------------------------------------- 3d materials
        public static Material Mat3D(Color color, Texture2D tex = null, float smoothness = 0.18f, Color? emission = null)
        {
            Shader sh = null;
            if (Pipeline3D == Pipeline.Urp)
            {
                sh = Shader.Find("Universal Render Pipeline/Lit");
                if (sh == null) sh = Shader.Find("Universal Render Pipeline/Unlit");
            }
            else
            {
                sh = Shader.Find("Standard");
            }
            if (sh == null) sh = Shader.Find("Universal Render Pipeline/Lit");
            if (sh == null) sh = Shader.Find("Standard");
            if (sh == null) sh = Shader.Find("Sprites/Default");

            var m = new Material(sh);
            m.name = "VirusMat";
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", color);
            if (m.HasProperty("_Color")) m.SetColor("_Color", color);
            m.color = color;
            if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", smoothness);
            if (m.HasProperty("_Glossiness")) m.SetFloat("_Glossiness", smoothness);
            if (m.HasProperty("_Metallic")) m.SetFloat("_Metallic", 0f);
            if (tex != null)
            {
                m.mainTexture = tex;
                if (m.HasProperty("_BaseMap")) m.SetTexture("_BaseMap", tex);
                if (tex.wrapMode == TextureWrapMode.Repeat)
                {
                    if (m.HasProperty("_BaseMap")) m.SetTextureScale("_BaseMap", new Vector2(3f, 2f));
                    if (m.HasProperty("_MainTex")) m.SetTextureScale("_MainTex", new Vector2(3f, 2f));
                }
            }
            if (emission.HasValue)
            {
                m.EnableKeyword("_EMISSION");
                if (m.HasProperty("_EmissionColor")) m.SetColor("_EmissionColor", emission.Value);
            }
            return m;
        }

        public static Material MatUnlit(Color color)
        {
            Shader sh = null;
            if (Pipeline3D == Pipeline.Urp) sh = Shader.Find("Universal Render Pipeline/Unlit");
            if (sh == null) sh = Shader.Find("Unlit/Color");
            if (sh == null) sh = Shader.Find("Universal Render Pipeline/Lit");
            if (sh == null) sh = Shader.Find("Standard");
            var m = new Material(sh);
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", color);
            if (m.HasProperty("_Color")) m.SetColor("_Color", color);
            m.color = color;
            return m;
        }

        public static Mesh Octahedron()
        {
            var mesh = new Mesh();
            mesh.name = "VirusOctahedron";
            mesh.vertices = new[]
            {
                new Vector3(0, 1, 0), new Vector3(0, -1, 0),
                new Vector3(1, 0, 0), new Vector3(-1, 0, 0),
                new Vector3(0, 0, 1), new Vector3(0, 0, -1)
            };
            mesh.triangles = new[]
            {
                0, 4, 2,  0, 2, 5,  0, 5, 3,  0, 3, 4,
                1, 2, 4,  1, 5, 2,  1, 3, 5,  1, 4, 3
            };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        // ---------------------------------------------------------------- primitives
        static Sprite MakeSprite(Texture2D tex, float ppu, Vector2 pivot)
        {
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), pivot, ppu, 0, SpriteMeshType.FullRect);
        }

        static Texture2D Blank(int w, int h)
        {
            var t = new Texture2D(w, h, TextureFormat.RGBA32, false);
            var px = new Color32[w * h];
            for (int i = 0; i < px.Length; i++) px[i] = new Color32(0, 0, 0, 0);
            t.SetPixels32(px);
            t.wrapMode = TextureWrapMode.Clamp;
            t.filterMode = FilterMode.Point;
            return t;
        }

        static void Fill(Texture2D t, int x, int y, int w, int h, Color c)
        {
            for (int yy = y; yy < y + h; yy++)
            {
                if (yy < 0 || yy >= t.height) continue;
                for (int xx = x; xx < x + w; xx++)
                {
                    if (xx < 0 || xx >= t.width) continue;
                    t.SetPixel(xx, yy, c);
                }
            }
        }

        static void RectFill(Texture2D t, int x, int y, int w, int h, Color c) => Fill(t, x, y, w, h, c);

        static void Ellipse(Texture2D t, float cx, float cy, float rx, float ry, Color c)
        {
            int x0 = Mathf.FloorToInt(cx - rx), x1 = Mathf.CeilToInt(cx + rx);
            int y0 = Mathf.FloorToInt(cy - ry), y1 = Mathf.CeilToInt(cy + ry);
            for (int y = y0; y <= y1; y++)
            {
                for (int x = x0; x <= x1; x++)
                {
                    if (x < 0 || y < 0 || x >= t.width || y >= t.height) continue;
                    float nx = (x - cx) / Mathf.Max(0.001f, rx);
                    float ny = (y - cy) / Mathf.Max(0.001f, ry);
                    if (nx * nx + ny * ny <= 1f) t.SetPixel(x, y, c);
                }
            }
        }

        static void Line(Texture2D t, int x0, int y0, int x1, int y1, Color c)
        {
            int dx = Mathf.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
            int dy = -Mathf.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
            int err = dx + dy;
            while (true)
            {
                if (x0 >= 0 && y0 >= 0 && x0 < t.width && y0 < t.height) t.SetPixel(x0, y0, c);
                if (x0 == x1 && y0 == y1) break;
                int e2 = 2 * err;
                if (e2 >= dy) { err += dy; x0 += sx; }
                if (e2 <= dx) { err += dx; y0 += sy; }
            }
        }

        static void FillPoly(Texture2D t, Vector2[] pts, Color c)
        {
            int minY = int.MaxValue, maxY = int.MinValue;
            for (int i = 0; i < pts.Length; i++)
            {
                int py = Mathf.RoundToInt(pts[i].y);
                if (py < minY) minY = py;
                if (py > maxY) maxY = py;
            }
            var xs = new List<float>();
            for (int y = minY; y <= maxY; y++)
            {
                xs.Clear();
                for (int i = 0; i < pts.Length; i++)
                {
                    var a = pts[i];
                    var b = pts[(i + 1) % pts.Length];
                    if ((a.y <= y && b.y > y) || (b.y <= y && a.y > y))
                    {
                        float k = (y - a.y) / (b.y - a.y);
                        xs.Add(a.x + k * (b.x - a.x));
                    }
                }
                xs.Sort();
                for (int i = 0; i + 1 < xs.Count; i += 2)
                {
                    int x0 = Mathf.CeilToInt(xs[i]);
                    int x1 = Mathf.FloorToInt(xs[i + 1]);
                    for (int x = x0; x <= x1; x++)
                    {
                        if (x >= 0 && y >= 0 && x < t.width && y < t.height) t.SetPixel(x, y, c);
                    }
                }
            }
        }

        public static void SetLayerRecursively(GameObject go, int layer)
        {
            go.layer = layer;
            foreach (Transform child in go.transform) SetLayerRecursively(child.gameObject, layer);
        }
    }
}
