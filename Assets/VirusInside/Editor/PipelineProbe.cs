using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace VirusInside.EditorTools
{
    // Temporary diagnostic tool. Removed once the rendering setup is confirmed.
    public static class PipelineProbe
    {
        [MenuItem("Virus Inside/Diagnostics/Probe Render Pipeline")]
        public static void Probe()
        {
            try
            {
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                Debug.Log("[PROBE] current pipeline: " + Describe(GraphicsSettings.currentRenderPipeline));

                // ---- 1. URP 2D renderer with a 3D cube ----
                var camGo = new GameObject("ProbeCam");
                var cam = camGo.AddComponent<Camera>();
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = Color.black;
                cam.transform.position = new Vector3(0f, 0f, -5f);
                cam.transform.LookAt(Vector3.zero);
                var add = cam.GetUniversalAdditionalCameraData();
                if (add != null) add.renderType = CameraRenderType.Base;

                var lightGo = new GameObject("ProbeLight");
                var light = lightGo.AddComponent<Light>();
                light.type = LightType.Directional;
                light.intensity = 2f;
                lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

                var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.transform.position = Vector3.zero;
                var urpLit = Shader.Find("Universal Render Pipeline/Lit");
                Debug.Log("[PROBE] URP Lit shader found: " + (urpLit != null));
                if (urpLit != null)
                {
                    var m = new Material(urpLit);
                    m.color = Color.red;
                    cube.GetComponent<MeshRenderer>().sharedMaterial = m;
                }
                RenderSettings.ambientMode = AmbientMode.Flat;
                RenderSettings.ambientLight = new Color(0.3f, 0.3f, 0.3f);

                var px = RenderAndRead(cam, 32, 32);
                Debug.Log("[PROBE] URP2D + URP Lit cube center pixel: " + px + " (red means visible)");

                // ---- 2. URP 2D renderer with an unlit sprite ----
                var spriteMat = new Material(Shader.Find("Sprites/Default"));
                spriteMat.color = Color.green;
                var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
                quad.transform.position = new Vector3(2f, 0f, 1f);
                quad.transform.localScale = new Vector3(1.4f, 1.4f, 1f);
                quad.GetComponent<MeshRenderer>().sharedMaterial = spriteMat;
                var px2 = RenderAndRead(cam, 52, 32);
                Debug.Log("[PROBE] URP2D + unlit quad pixel: " + px2 + " (green means visible)");

                // ---- 3. Switch to built-in pipeline ----
                QualitySettings.renderPipeline = null;
                GraphicsSettings.defaultRenderPipeline = null;
                Debug.Log("[PROBE] after switch pipeline: " + Describe(GraphicsSettings.currentRenderPipeline));

                var standard = Shader.Find("Standard");
                Debug.Log("[PROBE] Standard shader found: " + (standard != null));
                if (standard != null)
                {
                    var m2 = new Material(standard);
                    m2.color = Color.blue;
                    cube.GetComponent<MeshRenderer>().sharedMaterial = m2;
                }
                var px3 = RenderAndRead(cam, 32, 32);
                Debug.Log("[PROBE] builtin + Standard cube center pixel: " + px3 + " (blue means visible)");

                // ---- 4. Create a URP 3D renderer and use it ----
                var urpAsset = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>("Assets/Settings/UniversalRP.asset");
                Debug.Log("[PROBE] urp asset: " + (urpAsset != null));
                if (urpAsset != null)
                {
                    try
                    {
                        var so = new SerializedObject(urpAsset);
                        var list = so.FindProperty("m_RendererDataList");
                        Debug.Log("[PROBE] renderer list count: " + (list != null ? list.arraySize : -1));
                    }
                    catch (Exception e)
                    {
                        Debug.Log("[PROBE] renderer list error: " + e.Message);
                    }
                }

                Debug.Log("[PROBE] DONE");
            }
            catch (Exception e)
            {
                Debug.LogError("[PROBE] FAILED: " + e);
            }
        }

        static string Describe(RenderPipelineAsset asset)
        {
            if (asset == null) return "Built-in";
            return asset.GetType().Name + " / " + asset.name;
        }

        static Color RenderAndRead(Camera cam, int x, int y)
        {
            var rt = new RenderTexture(64, 64, 24, RenderTextureFormat.ARGB32);
            rt.Create();
            var prev = cam.targetTexture;
            cam.targetTexture = rt;
            cam.Render();
            RenderTexture.active = rt;
            var tex = new Texture2D(64, 64, TextureFormat.RGBA32, false);
            tex.ReadPixels(new Rect(0, 0, 64, 64), 0, 0);
            tex.Apply();
            var c = tex.GetPixel(x, y);
            RenderTexture.active = null;
            cam.targetTexture = prev;
            UnityEngine.Object.DestroyImmediate(tex);
            rt.Release();
            UnityEngine.Object.DestroyImmediate(rt);
            return c;
        }
    }
}
