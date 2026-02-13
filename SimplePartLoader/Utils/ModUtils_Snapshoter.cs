using PaintIn3D;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

namespace SimplePartLoader.Utils
{
    internal class ModUtils_Snapshoter : MonoBehaviour
    {
        Vector3 defaultDirection = new Vector3(-0.6f, -0.6f, -0.6f);
        Vector3 rotatedDirection = new Vector3(0.6f, -0.6f, 0.6f);

        const int THUMBNAIL_SIZE = 500;
        const string OUTPUT_PATH = "./Mods/ModUtilsThumbnails/";

        void Start()
        {
            if (!Directory.Exists(OUTPUT_PATH))
            {
                Directory.CreateDirectory(OUTPUT_PATH);
            }

            // Save current lighting settings
            AmbientMode prevAmbientMode = RenderSettings.ambientMode;
            Color prevAmbientLight = RenderSettings.ambientLight;
            float prevAmbientIntensity = RenderSettings.ambientIntensity;
            Color prevAmbientSkyColor = RenderSettings.ambientSkyColor;
            Color prevAmbientEquatorColor = RenderSettings.ambientEquatorColor;
            Color prevAmbientGroundColor = RenderSettings.ambientGroundColor;
            float prevReflectionIntensity = RenderSettings.reflectionIntensity;
            Material prevSkybox = RenderSettings.skybox;

            // Override to flat full-white ambient so lit shaders see maximum light everywhere
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = Color.white;
            RenderSettings.ambientIntensity = 1f;
            RenderSettings.ambientSkyColor = Color.white;
            RenderSettings.ambientEquatorColor = Color.white;
            RenderSettings.ambientGroundColor = Color.white;
            RenderSettings.reflectionIntensity = 0f;
            RenderSettings.skybox = null;

            foreach (Part p in PartManager.modLoadedParts)
            {
                if (p.Mod == null)
                {
                    continue;
                }

                if (p.Mod.Thumbnails)
                {
                    GameObject instanciated = GameObject.Instantiate(p.Prefab);
                    instanciated.transform.position = new Vector3(1000f, 350f, 1000f);

                    if (instanciated.GetComponent<CarProperties>() && instanciated.GetComponent<CarProperties>().Paintable)
                    {
                        var textures = instanciated.GetComponents<P3dPaintableTexture>();
                        if (textures.Length == 0)
                        {
                            CustomLogger.AddLine("ThumbnailGenerator", $"Part {p.CarProps.PrefabName} does not have a P3dPaintableTexture component! Skipping thumbnail generation.");
                            break;
                        }

                        foreach (var texture in textures)
                        {
                            texture.Activate();
                            texture.Clear();
                        }
                    }

                    foreach (var a in instanciated.GetComponentsInChildren<HexNut>())
                    {
                        GameObject.DestroyImmediate(a.gameObject);
                    }
                    foreach (var a in instanciated.GetComponentsInChildren<FlatNut>())
                    {
                        GameObject.DestroyImmediate(a.gameObject);
                    }
                    foreach (var a in instanciated.GetComponentsInChildren<WeldCut>())
                    {
                        GameObject.DestroyImmediate(a.gameObject);
                    }
                    foreach (var a in instanciated.GetComponentsInChildren<BoltNut>())
                    {
                        GameObject.DestroyImmediate(a.gameObject);
                    }

                    Vector3 direction;
                    if (p.ThumbnailRotation != null)
                        direction = p.ThumbnailRotation.Value;
                    else if (p.RotateThumbnail)
                        direction = rotatedDirection;
                    else
                        direction = defaultDirection;

                    Texture2D thumbnail = GenerateModelPreview(instanciated, direction);

                    if (thumbnail != null)
                    {
                        byte[] pngData = ImageConversion.EncodeToPNG(thumbnail);
                        File.WriteAllBytes(Path.Combine(OUTPUT_PATH, p.CarProps.PrefabName + ".png"), pngData);
                        DestroyImmediate(thumbnail);
                    }

                    DestroyImmediate(instanciated);
                }
            }

            // Restore original lighting settings
            RenderSettings.ambientMode = prevAmbientMode;
            RenderSettings.ambientLight = prevAmbientLight;
            RenderSettings.ambientIntensity = prevAmbientIntensity;
            RenderSettings.ambientSkyColor = prevAmbientSkyColor;
            RenderSettings.ambientEquatorColor = prevAmbientEquatorColor;
            RenderSettings.ambientGroundColor = prevAmbientGroundColor;
            RenderSettings.reflectionIntensity = prevReflectionIntensity;
            RenderSettings.skybox = prevSkybox;
        }

        Texture2D GenerateModelPreview(GameObject target, Vector3 direction)
        {
            Bounds bounds = CalculateBounds(target);
            if (bounds.size == Vector3.zero)
                return null;

            // Create a directional light pointing from the camera direction for even illumination
            GameObject lightObj = new GameObject("ThumbnailLight");
            Light dirLight = lightObj.AddComponent<Light>();
            dirLight.type = LightType.Directional;
            dirLight.color = Color.white;
            dirLight.intensity = 1.5f;
            dirLight.shadows = LightShadows.None;
            lightObj.transform.rotation = Quaternion.LookRotation(-direction.normalized);

            RenderTexture rt = new RenderTexture(THUMBNAIL_SIZE, THUMBNAIL_SIZE, 24, RenderTextureFormat.ARGB32);
            rt.antiAliasing = 4;
            rt.Create();

            GameObject camObj = new GameObject("ThumbnailCamera");
            Camera cam = camObj.AddComponent<Camera>();
            cam.targetTexture = rt;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0f, 0f, 0f, 0f);
            cam.nearClipPlane = 0.01f;
            cam.orthographic = false;

            float maxExtent = bounds.extents.magnitude;
            float camDistance = maxExtent * 2.5f;
            Vector3 camPosition = bounds.center + direction.normalized * camDistance;
            camObj.transform.position = camPosition;
            camObj.transform.LookAt(bounds.center);

            cam.farClipPlane = camDistance + maxExtent * 4f;

            cam.Render();

            RenderTexture previousActive = RenderTexture.active;
            RenderTexture.active = rt;

            Texture2D result = new Texture2D(THUMBNAIL_SIZE, THUMBNAIL_SIZE, TextureFormat.RGBA32, false);
            result.ReadPixels(new Rect(0, 0, THUMBNAIL_SIZE, THUMBNAIL_SIZE), 0, 0);
            result.Apply();

            RenderTexture.active = previousActive;

            DestroyImmediate(lightObj);
            DestroyImmediate(camObj);
            rt.Release();
            DestroyImmediate(rt);

            return result;
        }

        Bounds CalculateBounds(GameObject target)
        {
            Renderer[] renderers = target.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
                return new Bounds(target.transform.position, Vector3.zero);

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            return bounds;
        }
    }
}
