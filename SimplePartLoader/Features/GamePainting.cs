using PaintIn3D;
using System.Collections.Generic;
using UnityEngine;

namespace SimplePartLoader
{
    public class GamePainting
    {
        private static Material ChromeMaterial;
        private static Material BlackMaterial;
        private static Material PaintMaterial;
        private static Material ChromePaintableMaterial;
        private static Material GlassMaterial;
        private static PartPaintSetup DefaultSettings = new PartPaintSetup();

        public enum Quality
        {
            VeryLow = 1,
            Low = 2,
            Medium = 3,
            High = 4,
            VeryHigh = 5
        }

        public enum PaintPreset
        {
            FullSupport,
            Paint,
            PaintRust,
            PaintDirt,
            Dirt
        }

        public class PartPaintSetup
        {
            public bool ColorMap { get; set; } = true;
            public bool MetallicRustDust { get; set; } = true;
            public bool MainTex { get; set; } = true;
            public bool HoleMap { get; set; } = true;
            public bool ClearCoat { get; set; } = true;
            public bool PolishMap { get; set; } = true;
            public bool Counters { get; set; } = true;
        }

        internal class SpecialDirtPartSetup
        {
            public bool Chrome { get; set; }
            public bool Glass { get; set; }
            public bool Counters { get; set; } = true;
        }

        public static void SetupPart(Part p, PartPaintSetup config = null)
        {
            InternalSetupPart(p.Prefab, p.Mod, config);
        }

        public static void SetupGlassPart(Part p, bool enablePaintCounter = true)
        {
            InternalSetupSpecialDirtPart(p.Prefab, new SpecialDirtPartSetup() { Glass = true, Counters = enablePaintCounter }, p.Mod);
        }

        public static void SetupChromePart(Part p, bool enablePaintCounter = true)
        {
            InternalSetupSpecialDirtPart(p.Prefab, new SpecialDirtPartSetup() { Chrome = true, Counters = enablePaintCounter }, p.Mod);
        }

        internal static void InternalSetupPart(GameObject prefab, ModInstance mod, PartPaintSetup config = null)
        {
            if (config is null)
                config = DefaultSettings;


            if (!PaintMaterial)
            {
                PreloadMaterials();
                if (!PaintMaterial)
                {
                    CustomLogger.AddLine("GamePainting", "Error, material preload failed. Could not set painting for " + prefab.name);
                    return;
                }
            }

            if (prefab.GetComponent<P3dPaintable>())
            {
                CustomLogger.AddLine("GamePainting", $"Part {prefab.name} already has a P3dPaintable component, skipping.");
                return;
            }

            CarProperties cp = prefab.GetComponent<CarProperties>();
            if (!cp)
            {
                CustomLogger.AddLine("GamePainting", $"Error, part {prefab.name} does not have a CarProperties component, skipping.");
                return;
            }

            CustomLogger.AddLine("GamePainting", "Testing part " + prefab.name);

            OriginalMesh orMesh = prefab.GetComponent<OriginalMesh>();
            Mesh meshToUse = null;
            if (orMesh)
            {
                meshToUse = orMesh.Mesh;
            }

            // Ensure part has correct material
            var renderer = prefab.GetComponent<MeshRenderer>();
            if (!renderer)
            {
                CustomLogger.AddLine("GamePainting", $"Error, part {prefab.name} does not have a MeshRenderer component, skipping.");
                return;
            }

            var material = renderer.sharedMaterial;
            // If the part is already using a paintable material, keep it the same so we dont break glass / chrome dirtable parts from the game
            if (!material || !material.shader || !material.shader.name.StartsWith("Shader Graphs"))
            {
                renderer.material = PaintMaterial;
            }

            prefab.AddComponent<P3dPaintable>();
            prefab.AddComponent<P3dMaterialCloner>();
            int res = GetPaintRes(mod?.Settings?.PaintQuality ?? Quality.Low);

            if (config.ColorMap)
            {
                P3dPaintableTexture p3dColorMap = prefab.AddComponent<P3dPaintableTexture>();
                p3dColorMap.Color = Color.gray;
                p3dColorMap.Group = 0;
                p3dColorMap.Slot = new P3dSlot(0, "_L2ColorMap");

                UpdatePaintableTexture(p3dColorMap, res);

                cp.Paintable = true;

                if (config.Counters)
                {
                    P3dChangeCounter counter_colorMap = prefab.AddComponent<P3dChangeCounter>();
                    counter_colorMap.PaintableTexture = p3dColorMap;
                    counter_colorMap.Threshold = 0.1f;
                    counter_colorMap.enabled = false;
                    counter_colorMap.MaskMesh = meshToUse;
                }
            }

            if (config.MetallicRustDust)
            {
                P3dPaintableTexture p3dMetallicRust = prefab.AddComponent<P3dPaintableTexture>();
                p3dMetallicRust.Color = Color.white;
                p3dMetallicRust.Group = 100;
                p3dMetallicRust.Slot = new P3dSlot(0, "_L2MetallicRustDustSmoothness");
                UpdatePaintableTexture(p3dMetallicRust, res);

                cp.Fairable = true;
                cp.MeshRepairable = true;

                if (config.Counters)
                {
                    P3dChangeCounter counter_rust = prefab.AddComponent<P3dChangeCounter>();
                    counter_rust.PaintableTexture = p3dMetallicRust;
                    counter_rust.Threshold = 0.5f;
                    counter_rust.enabled = false;
                    counter_rust.Color = new Color(0, 0, 0, 1f);
                    counter_rust.MaskMesh = meshToUse;
                }
            }

            if (config.MainTex)
            {
                P3dPaintableTexture p3dDirt = prefab.AddComponent<P3dPaintableTexture>();
                p3dDirt.Color = Color.white;
                p3dDirt.Group = 5;
                p3dDirt.Slot = new P3dSlot(0, "_MainTex");
                UpdatePaintableTexture(p3dDirt, res);

                cp.Washable = true;

                if (config.Counters)
                {
                    P3dChangeCounter counterDirt = prefab.AddComponent<P3dChangeCounter>();
                    counterDirt.PaintableTexture = p3dDirt;
                    counterDirt.Threshold = 0.7f;
                    counterDirt.enabled = false;
                    counterDirt.Color = new Color(0.219f, 0.219f, 0.219f, 0f);
                    counterDirt.MaskMesh = meshToUse;
                }
            }

            if (config.HoleMap)
            {
                P3dPaintableTexture p3dHoles = prefab.AddComponent<P3dPaintableTexture>();
                p3dHoles.Color = Color.white;
                p3dHoles.Group = 20;
                p3dHoles.Slot = new P3dSlot(0, "HoleMap");
                UpdatePaintableTexture(p3dHoles, res);
            }

            if (config.ClearCoat)
            {
                P3dPaintableTexture p3dClearcoat = prefab.AddComponent<P3dPaintableTexture>();
                p3dClearcoat.Color = Color.white;
                p3dClearcoat.Group = 30;
                p3dClearcoat.Slot = new P3dSlot(0, "ClearCoatMap");
                UpdatePaintableTexture(p3dClearcoat, res);
            }

            if (config.PolishMap)
            {
                P3dPaintableTexture p3dPolish = prefab.AddComponent<P3dPaintableTexture>();
                p3dPolish.Color = Color.white;
                p3dPolish.Group = 60;
                p3dPolish.Slot = new P3dSlot(0, "PolishMap");
                UpdatePaintableTexture(p3dPolish, res);
            }
        }

        internal static void InternalSetupSpecialDirtPart(GameObject prefab, SpecialDirtPartSetup config)
        {
            InternalSetupSpecialDirtPart(prefab, config, null);
        }

        internal static void InternalSetupSpecialDirtPart(GameObject prefab, SpecialDirtPartSetup config, ModInstance mod)
        {
            if (config is null)
                config = new SpecialDirtPartSetup();

            if (config.Glass)
            {
                if (!GlassMaterial)
                {
                    PreloadMaterials();
                    if (!GlassMaterial)
                    {
                        CustomLogger.AddLine("GamePainting", "Error, material preload failed. Could not set painting for " + prefab.name);
                        return;
                    }
                }
            }
            else if (config.Chrome)
            {
                if (!ChromePaintableMaterial)
                {
                    PreloadMaterials();
                    if (!ChromePaintableMaterial)
                    {
                        CustomLogger.AddLine("GamePainting", "Error, material preload failed. Could not set painting for " + prefab.name);
                        return;
                    }
                }
            }

            if (prefab.GetComponent<P3dPaintable>())
            {
                CustomLogger.AddLine("GamePainting", $"Part {prefab.name} already has a P3dPaintable component, skipping.");
                return;
            }

            CarProperties cp = prefab.GetComponent<CarProperties>();
            if (!cp)
            {
                CustomLogger.AddLine("GamePainting", $"Error, part {prefab.name} does not have a CarProperties component, skipping.");
                return;
            }

            CustomLogger.AddLine("GamePainting", "Testing part " + prefab.name);

            // Ensure part has correct material
            var renderer = prefab.GetComponent<MeshRenderer>();
            if (!renderer)
            {
                CustomLogger.AddLine("GamePainting", $"Error, part {prefab.name} does not have a MeshRenderer component, skipping.");
                return;
            }

            renderer.material = config.Glass ? GlassMaterial : ChromePaintableMaterial;
            prefab.AddComponent<P3dPaintable>();
            prefab.AddComponent<P3dMaterialCloner>();
            int res = GetPaintRes(mod?.Settings?.PaintQuality ?? Quality.Low);

            OriginalMesh orMesh = prefab.GetComponent<OriginalMesh>();
            Mesh meshToUse = null;
            if (orMesh)
            {
                meshToUse = orMesh.Mesh;
            }

            P3dPaintableTexture p3dDirt = prefab.AddComponent<P3dPaintableTexture>();
            p3dDirt.Color = Color.white;
            p3dDirt.Group = 5;
            p3dDirt.Slot = new P3dSlot(0, "_MainTex");
            UpdatePaintableTexture(p3dDirt, res);

            cp.Washable = true;

            if (config.Counters)
            {
                P3dChangeCounter counterDirt = prefab.AddComponent<P3dChangeCounter>();
                counterDirt.PaintableTexture = p3dDirt;
                counterDirt.Threshold = 0.7f;
                counterDirt.enabled = false;
                counterDirt.Color = new Color(0.219f, 0.219f, 0.219f, 0f);
                counterDirt.MaskMesh = meshToUse;
            }

            if(config.Glass)
            {
                P3dPaintableTexture p3dDecalSupport = prefab.AddComponent<P3dPaintableTexture>();
                p3dDecalSupport.Color = Color.white;
                p3dDecalSupport.Group = 91;
                p3dDecalSupport.Slot = new P3dSlot(0, "Decals");
                UpdatePaintableTexture(p3dDecalSupport, res);

                if(!prefab.GetComponent<GlassDirtSetupIdentifier>())
                {
                    prefab.AddComponent<GlassDirtSetupIdentifier>();
                }
            }
            else if(config.Chrome)
            {
                if(!prefab.GetComponent<ChromeDirtSetupIdentifier>())
                {
                    prefab.AddComponent<ChromeDirtSetupIdentifier>();
                }
            }
        }

        internal class ChromeDirtSetupIdentifier : MonoBehaviour { }
        internal class GlassDirtSetupIdentifier : MonoBehaviour { }

        public static Material GetBlackMaterial()
        {
            if (!BlackMaterial) PreloadMaterials();

            return BlackMaterial;
        }

        public static Material GetChromeMaterial()
        {
            if (!ChromeMaterial) PreloadMaterials();

            return ChromeMaterial;
        }

        public static Material GetPaintMaterial()
        {
            if (!PaintMaterial) PreloadMaterials();

            return PaintMaterial;
        }

        private static void PreloadMaterials()
        {
            foreach (GameObject go in PartManager.gameParts)
            {
                if (go != null)
                {
                    if (go.name == "DoorFL06")
                    {
                        PaintMaterial = go.transform.GetComponent<MeshRenderer>().material;
                        ChromeMaterial = go.transform.Find("DoorFLFram06").GetComponent<Renderer>().material;
                    }
                    else if (go.name == "Firewall06")
                    {
                        BlackMaterial = go.transform.GetComponent<MeshRenderer>().material;
                    }
                    else if(go.name == "WindowFR06")
                    {
                        GlassMaterial = go.transform.GetComponent<MeshRenderer>().material;
                    }
                    else if(go.name == "BumperR07")
                    {
                        ChromePaintableMaterial = go.transform.GetComponent<MeshRenderer>().material;
                    }
                }

                if (BlackMaterial && ChromeMaterial && PaintMaterial && GlassMaterial && ChromePaintableMaterial)
                    break;
            }

            if (!BlackMaterial || !ChromeMaterial || !PaintMaterial || !GlassMaterial || !ChromePaintableMaterial)
            {
                CustomLogger.AddLine("GamePainting", "Error, material preload failed. Is player in-game?");
            }

        }

        private static void UpdatePaintableTexture(P3dPaintableTexture texture, int resolution)
        {
            texture.Width = resolution;
            texture.Height = resolution;
            texture.UpdateMaterial();
        }

        private static int GetPaintRes(Quality res)
        {
            if (ModMain.ForcedPaintQuality != null && ModMain.ForcedPaintQuality.selectedOption != 0)
            {
                res = (Quality)ModMain.ForcedPaintQuality.selectedOption;
            }

            switch (res)
            {
                case Quality.VeryLow:
                    return 256;

                case Quality.Low:
                    return 512;

                case Quality.Medium:
                    return 1024;

                case Quality.High:
                    return 2048;

                case Quality.VeryHigh:
                    return 4096;

                default:
                    return 512;
            }
        }

        public static PartPaintSetup GetPreset(PaintPreset preset)
        {
            return new PartPaintSetup()
            {
                Counters = true,
                ColorMap = preset is PaintPreset.FullSupport || preset is PaintPreset.Paint || preset is PaintPreset.PaintRust || preset is PaintPreset.PaintDirt,
                MetallicRustDust = preset is PaintPreset.FullSupport || preset is PaintPreset.PaintRust,
                ClearCoat = preset is PaintPreset.FullSupport || preset is PaintPreset.Paint || preset is PaintPreset.PaintRust || preset is PaintPreset.PaintDirt,
                HoleMap = preset is PaintPreset.FullSupport || preset is PaintPreset.Paint || preset is PaintPreset.PaintRust,
                MainTex = preset is PaintPreset.FullSupport || preset is PaintPreset.PaintDirt || preset is PaintPreset.Dirt,
                PolishMap = preset is PaintPreset.FullSupport || preset is PaintPreset.Paint || preset is PaintPreset.PaintRust || preset is PaintPreset.PaintDirt
            };
        }
    }
}
