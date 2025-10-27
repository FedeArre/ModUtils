using PaintIn3D;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SimplePartLoader.Utils
{
    internal class ModUtils_Snapshoter : MonoBehaviour
    {
        Vector3 defaultPosition = new Vector3(-0.6f, -0.6f, -0.6f);
        Vector3 rotatedPosition = new Vector3(0.6f, -0.6f, 0.6f);

        void Start()
        {
            RuntimePreviewGenerator.BackgroundColor = new Color(0f, 0f, 0f, 0f);
            Sprite s = null;


            if (!Directory.Exists("./Mods/ModUtilsThumbnails"))
            {
                Directory.CreateDirectory("./Mods/ModUtilsThumbnails");
            }
            bool lightsCreated = false;

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

                    if (!lightsCreated)
                    {
                        float distance = 3f;

                        for (int i = 0; i < 4; i++)
                        {
                            // Create a new GameObject for each spotlight
                            GameObject lightObj = new GameObject($"SpotLight_{i + 1}");
                            Light spot = lightObj.AddComponent<Light>();

                            // Configure the light
                            spot.type = LightType.Spot;
                            spot.color = Color.white;
                            spot.intensity = 3f;
                            spot.range = 10f;
                            spot.spotAngle = 45f;

                            float angle = i * 90f * Mathf.Deg2Rad;
                            Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * distance;
                            lightObj.transform.position = instanciated.transform.position + offset + Vector3.up * 1.5f;

                            // Make it look at the center object
                            lightObj.transform.LookAt(instanciated.transform);
                        }

                        lightsCreated = true;
                    }

                    if (instanciated.GetComponent<CarProperties>() && instanciated.GetComponent<CarProperties>().Paintable)
                    {
                        var texture = instanciated.GetComponent<P3dPaintableTexture>();
                        if(texture is null)
                        {
                            CustomLogger.AddLine("ThumbnailGenerator", $"Part {p.CarProps.PrefabName} does not have a P3dPaintableTexture component! Skipping thumbnail generation.");
                            break;
                        }

                        texture.Color = Color.gray;
                        texture.Activate();
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

                    RuntimePreviewGenerator.PreviewDirection = p.RotateThumbnail ? rotatedPosition : defaultPosition;
                    s = Sprite.Create(RuntimePreviewGenerator.GenerateModelPreview(instanciated.transform, 500, 500, false), new Rect(0f, 0f, 500f, 500f), new Vector2(0.5f, 0.5f), 100f);
                    SnapshotCamera.SavePNG(s.texture, p.CarProps.PrefabName + ".png", "./Mods/ModUtilsThumbnails/");
                }
            }
        }
    }
}
