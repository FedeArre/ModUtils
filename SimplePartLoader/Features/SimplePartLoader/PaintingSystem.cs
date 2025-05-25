using PaintIn3D;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SimplePartLoader
{
    [Obsolete("Please use the new GamePainting API instead. This is kept for compatibility purposes")]
    public class PaintingSystem
    {
        static Material ChromeMaterial = null;
        static Material PaintRustMaterial = null;
        static Material DirtMaterial = null;
        static Material BaseMaterial = null;
        internal static Material CullBaseMaterial = null;
        internal static Shader BackfaceShader = null;
        public enum Types
        {
            FullPaintingSupport = 1,
            OnlyPaint,
            OnlyPaintAndRust,
            OnlyDirt,
            OnlyPaintAndDirt
        }

        public enum PartPaintResolution
        {
            Low = 1,
            Medium = 2,
            High = 3
        }

        internal static void EnablePaintOnly(Part part, int materialIndex)
        {
            GameObject Prefab = part.Prefab;

            if (part.Paintable || Prefab.GetComponent<P3dPaintable>())
            {
                CustomLogger.AddLine("PaintingSystem", $"Tried to use EnablePaintSupport on {Prefab.name} but already has painting components.");
                return;
            }

            if(materialIndex != -1)
            {
                Prefab.AddComponent<P3dPaintable>();

                P3dPaintableTexture paintableTexture = Prefab.AddComponent<P3dPaintableTexture>();
                paintableTexture.Color = Color.gray;
                paintableTexture.Slot = new P3dSlot(materialIndex, "_MainTex");
                paintableTexture.UpdateMaterial();

                Prefab.AddComponent<P3dMaterialCloner>().Index = materialIndex;

                CheckHighResolutionPaint(part, paintableTexture);

                TryApplyL2Overwrite(part);
                part.CarProps.Paintable = true;
                part.Paintable = true;
            }
            else
            {
                Prefab.AddComponent<P3dPaintable>();

                // Material checks
                Renderer prefabRenderer = Prefab.GetComponent<Renderer>();
                int l2Material_index = -1;

                // Looking up the material or creating it.
                for (int i = 0; i < prefabRenderer.materials.Length; i++)
                {
                    if (prefabRenderer.materials[i].shader.name == "Thunderbyte/RustDirt2Layers URP")
                    {
                        l2Material_index = i;
                        break;
                    }
                }

                if (l2Material_index == -1)
                {
                    CustomLogger.AddLine("PaintingSystem", $"Missing Thunderbyte/RustDirt2Layers URP material (Paint & Rust) on part " + part.Prefab.name);
                    return;
                }

                // Now we add all the painting components
                P3dMaterialCloner materialCloner_l2 = Prefab.AddComponent<P3dMaterialCloner>();

                P3dPaintableTexture paintableTexture_colorMap = Prefab.AddComponent<P3dPaintableTexture>();

                P3dChangeCounter counter_colorMap = Prefab.AddComponent<P3dChangeCounter>();
                
                P3dSlot p3dSlot_colorMap = new P3dSlot(l2Material_index, "_L2ColorMap");

                // Check for original mesh
                OriginalMesh orMesh = Prefab.GetComponent<OriginalMesh>();
                Mesh meshToUse = null;
                if (orMesh)
                {
                    meshToUse = orMesh.Mesh;
                }

                // Setting up the components

                // Material cloner
                materialCloner_l2.Index = l2Material_index;

                // Paintable textures
                paintableTexture_colorMap.Slot = p3dSlot_colorMap;

                counter_colorMap.PaintableTexture = paintableTexture_colorMap;
                counter_colorMap.Threshold = 0.1f;
                counter_colorMap.enabled = false;
                counter_colorMap.MaskMesh = meshToUse;

                CheckHighResolutionPaint(part, paintableTexture_colorMap);

                TryApplyL2Overwrite(part);
                // Final details
                part.Paintable = true;
                part.CarProps.Paintable = true;
            }
        }

        internal static void EnablePaintAndRust(Part part)
        {
            GameObject Prefab = part.Prefab;

            if (part.Paintable || Prefab.GetComponent<P3dPaintable>())
            {
                CustomLogger.AddLine("PaintingSystem", $"Tried to use EnablePaintSupport on {Prefab.name} but already has painting components.");
                return;
            }

            Prefab.AddComponent<P3dPaintable>();

            // Material checks
            Renderer prefabRenderer = Prefab.GetComponent<Renderer>();
            int l2Material_index = -1;

            // Looking up the material or creating it.
            for (int i = 0; i < prefabRenderer.materials.Length; i++)
            {
                if (prefabRenderer.materials[i].shader.name == "Thunderbyte/RustDirt2Layers URP")
                {
                    l2Material_index = i;
                    break;
                }
            }

            if (l2Material_index == -1)
            {
                CustomLogger.AddLine("PaintingSystem", $"Missing Thunderbyte/RustDirt2Layers URP material (Paint & Rust) on part " + part.Prefab.name);
                return;
            }

            // Check for original mesh
            OriginalMesh orMesh = Prefab.GetComponent<OriginalMesh>();
            Mesh meshToUse = null;
            if (orMesh)
            {
                meshToUse = orMesh.Mesh;
            }

            // Now we add all the painting components
            P3dMaterialCloner materialCloner_l2 = Prefab.AddComponent<P3dMaterialCloner>();

            P3dPaintableTexture paintableTexture_colorMap = Prefab.AddComponent<P3dPaintableTexture>();
            P3dPaintableTexture paintableTexture_rust = Prefab.AddComponent<P3dPaintableTexture>();
            P3dPaintableTexture paintableTexture_grungeMap = Prefab.AddComponent<P3dPaintableTexture>();

            P3dChangeCounter counter_rust = Prefab.AddComponent<P3dChangeCounter>();
            P3dChangeCounter counter_colorMap = Prefab.AddComponent<P3dChangeCounter>();

            P3dSlot p3dSlot_rustDirt = new P3dSlot(l2Material_index, "_L2MetallicRustDustSmoothness");
            P3dSlot p3dSlot_colorMap = new P3dSlot(l2Material_index, "_L2ColorMap");
            P3dSlot p3dSlot_grungeMap = new P3dSlot(l2Material_index, "_GrungeMap");

            // Setting up the components

            // Material cloner
            materialCloner_l2.Index = l2Material_index;

            // Paintable textures
            paintableTexture_colorMap.Slot = p3dSlot_colorMap;

            paintableTexture_grungeMap.Slot = p3dSlot_grungeMap;
            paintableTexture_grungeMap.Group = 100;

            paintableTexture_rust.Slot = p3dSlot_rustDirt;
            paintableTexture_rust.Group = 100;

            // Counters
            counter_rust.PaintableTexture = paintableTexture_rust;
            counter_rust.Threshold = 0.5f;
            counter_rust.enabled = false;
            counter_rust.Color = new Color(0, 0, 0, 1f);
            counter_rust.MaskMesh = meshToUse;

            counter_colorMap.PaintableTexture = paintableTexture_colorMap;
            counter_colorMap.Threshold = 0.1f;
            counter_colorMap.enabled = false;
            counter_colorMap.MaskMesh = meshToUse;

            CheckHighResolutionPaint(part, paintableTexture_colorMap);

            TryApplyL2Overwrite(part);
            // Final details
            part.Paintable = true;
            part.CarProps.Paintable = true;
            part.CarProps.DMGdeformMesh = true; // NOTE! As a side effect this will enable mesh deform on crashes.
            part.CarProps.Fairable = true;
        }


        internal static void EnableDirtOnly(Part part)
        {
            GameObject Prefab = part.Prefab;

            if (part.Paintable || Prefab.GetComponent<P3dPaintable>())
            {
                CustomLogger.AddLine("PaintingSystem", $"Tried to use EnablePaintSupport on {Prefab.name} but already has painting components.");
                return;
            }

            Prefab.AddComponent<P3dPaintable>();

            // Material checks
            Renderer prefabRenderer = Prefab.GetComponent<Renderer>();
            int alphaMaterial_index = -1;

            // Looking up the material or creating it.
            for (int i = 0; i < prefabRenderer.materials.Length; i++)
            {
                if (prefabRenderer.materials[i].shader.name == "Paint in 3D/Alpha")
                {
                    alphaMaterial_index = i;
                    break;
                }
            }

            if(alphaMaterial_index == -1)
            {
                CustomLogger.AddLine("PaintingSystem", $"Missing Paint in 3D/Alpha material URP (Dirt) on part " + part.Prefab.name);
                return;
            }

            // Check for original mesh
            OriginalMesh orMesh = Prefab.GetComponent<OriginalMesh>();
            Mesh meshToUse = null;
            if (orMesh)
            {
                meshToUse = orMesh.Mesh;
            }

            // Setting up the components

            // Material cloner
            P3dMaterialCloner materialCloner_dirt = Prefab.AddComponent<P3dMaterialCloner>();
            materialCloner_dirt.Index = alphaMaterial_index;

            // Paintable texture
            P3dPaintableTexture paintableTexture_dirt = Prefab.AddComponent<P3dPaintableTexture>();
            paintableTexture_dirt.Slot = new P3dSlot(alphaMaterial_index, "_MainTex");
            paintableTexture_dirt.Group = 5;

            // Counter
            P3dChangeCounter counter_dirt = Prefab.AddComponent<P3dChangeCounter>();
            counter_dirt.PaintableTexture = paintableTexture_dirt;
            counter_dirt.Threshold = 0.7f;
            counter_dirt.enabled = false;
            counter_dirt.Color = new Color(0.219f, 0.219f, 0.219f, 0f);
            counter_dirt.MaskMesh = meshToUse;

            // Final details
            part.CarProps.Washable = true;
        }

        internal static void EnableFullSupport(Part part)
        {
            GameObject Prefab = part.Prefab;

            if (part.Paintable || Prefab.GetComponent<P3dPaintable>())
            {
                CustomLogger.AddLine("PaintingSystem", $"Tried to use EnablePaintSupport on {Prefab.name} but already has painting components.");
                return;
            }

            Prefab.AddComponent<P3dPaintable>();

            // Material checks
            Renderer prefabRenderer = Prefab.GetComponent<Renderer>();
            int l2Material_index = -1, alphaMaterial_index = -1;

            for (int i = 0; i < prefabRenderer.materials.Length; i++)
            {
                if (prefabRenderer.materials[i].shader.name == "Thunderbyte/RustDirt2Layers URP")
                {
                    l2Material_index = i;
                    break;
                }
            }

            for (int i = 0; i < prefabRenderer.materials.Length; i++)
            {
                if (prefabRenderer.materials[i].shader.name == "Paint in 3D/Alpha")
                {
                    alphaMaterial_index = i;
                    break;
                }
            }

            if(alphaMaterial_index == -1)
            {
                CustomLogger.AddLine("PaintingSystem", $"Missing Paint in 3D/Alpha material (Dirt) on part " + part.Prefab.name);
                return;
            }

            if(l2Material_index == -1)
            {
                CustomLogger.AddLine("PaintingSystem", $"Missing Thunderbyte/RustDirt2Layers material (Paint & Rust) on part " + part.Prefab.name);
                return;
            }

            // Check for original mesh
            OriginalMesh orMesh = Prefab.GetComponent<OriginalMesh>();
            Mesh meshToUse = null;
            if(orMesh)
            {
                meshToUse = orMesh.Mesh;
            }

            // Now we create our painting components.
            P3dMaterialCloner materialCloner_l2 = Prefab.AddComponent<P3dMaterialCloner>();
            P3dMaterialCloner materialCloner_paint = Prefab.AddComponent<P3dMaterialCloner>();

            P3dPaintableTexture paintableTexture_colorMap = Prefab.AddComponent<P3dPaintableTexture>();
            P3dPaintableTexture paintableTexture_rust = Prefab.AddComponent<P3dPaintableTexture>();
            P3dPaintableTexture paintableTexture_dirt = Prefab.AddComponent<P3dPaintableTexture>();
            P3dPaintableTexture paintableTexture_grungeMap = Prefab.AddComponent<P3dPaintableTexture>();

            P3dChangeCounter counter_dirt = Prefab.AddComponent<P3dChangeCounter>();
            P3dChangeCounter counter_rust = Prefab.AddComponent<P3dChangeCounter>();
            P3dChangeCounter counter_colorMap = Prefab.AddComponent<P3dChangeCounter>();

            P3dSlot p3dSlot_rustDirt = new P3dSlot(l2Material_index, "_L2MetallicRustDustSmoothness");
            P3dSlot p3dSlot_colorMap = new P3dSlot(l2Material_index, "_L2ColorMap");
            P3dSlot p3dSlot_grungeMap = new P3dSlot(l2Material_index, "_GrungeMap");
            P3dSlot p3dSlot_dirt = new P3dSlot(alphaMaterial_index, "_MainTex");

            // Setting up the components

            // Material cloner
            materialCloner_l2.Index = l2Material_index;
            materialCloner_paint.Index = alphaMaterial_index;

            // Paintable textures
            paintableTexture_colorMap.Slot = p3dSlot_colorMap;
            
            paintableTexture_grungeMap.Slot = p3dSlot_grungeMap;
            paintableTexture_grungeMap.Group = 100;

            paintableTexture_rust.Slot = p3dSlot_rustDirt;
            paintableTexture_rust.Group = 100;
            
            paintableTexture_dirt.Slot = p3dSlot_dirt;
            paintableTexture_dirt.Group = 5;

            // Counters
            counter_rust.PaintableTexture = paintableTexture_rust;
            counter_rust.Threshold = 0.5f;
            counter_rust.enabled = false;
            counter_rust.Color = new Color(0, 0, 0, 1f);
            counter_rust.MaskMesh = meshToUse;
            
            counter_colorMap.PaintableTexture = paintableTexture_colorMap;
            counter_colorMap.Threshold = 0.1f;
            counter_colorMap.enabled = false;
            counter_colorMap.MaskMesh = meshToUse;

            counter_dirt.PaintableTexture = paintableTexture_dirt;
            counter_dirt.Threshold = 0.7f;
            counter_dirt.enabled = false;
            counter_dirt.Color = new Color(0.219f, 0.219f, 0.219f, 0f);
            counter_dirt.MaskMesh = meshToUse;

            CheckHighResolutionPaint(part, paintableTexture_colorMap);

            TryApplyL2Overwrite(part);
            // Final details
            part.Paintable = true;
            part.CarProps.Paintable = true;
            part.CarProps.Washable = true;
            part.CarProps.Fairable = true;
            part.CarProps.DMGdeformMesh = true; // NOTE! As a side effect this will enable mesh deform on crashes.
        }

        internal static void EnablePaintAndDirt(Part part)
        {
            GameObject Prefab = part.Prefab;

            if (part.Paintable || Prefab.GetComponent<P3dPaintable>())
            {
                CustomLogger.AddLine("PaintingSystem", $"Tried to use EnablePaintSupport on {Prefab.name} but already has painting components.");
                return;
            }

            Prefab.AddComponent<P3dPaintable>();

            // Material checks
            Renderer prefabRenderer = Prefab.GetComponent<Renderer>();
            int l2Material_index = -1, alphaMaterial_index = -1;

            for (int i = 0; i < prefabRenderer.materials.Length; i++)
            {
                if (prefabRenderer.materials[i].shader.name == "Thunderbyte/RustDirt2Layers URP")
                {
                    l2Material_index = i;
                    break;
                }
            }

            for (int i = 0; i < prefabRenderer.materials.Length; i++)
            {
                if (prefabRenderer.materials[i].shader.name == "Paint in 3D/Alpha")
                {
                    alphaMaterial_index = i;
                    break;
                }
            }

            if (alphaMaterial_index == -1)
            {
                CustomLogger.AddLine("PaintingSystem", $"Missing Paint in 3D/Alpha material (Dirt) on part " + part.Prefab.name);
                return;
            }

            if (l2Material_index == -1)
            {
                CustomLogger.AddLine("PaintingSystem", $"Missing Thunderbyte/RustDirt2Layers material (Paint & Rust) on part " + part.Prefab.name);
                return;
            }

            // Now we create our painting components.
            P3dMaterialCloner materialCloner_l2 = Prefab.AddComponent<P3dMaterialCloner>();
            P3dMaterialCloner materialCloner_paint = Prefab.AddComponent<P3dMaterialCloner>();

            P3dPaintableTexture paintableTexture_colorMap = Prefab.AddComponent<P3dPaintableTexture>();
            P3dPaintableTexture paintableTexture_dirt = Prefab.AddComponent<P3dPaintableTexture>();
            P3dPaintableTexture paintableTexture_rust = Prefab.AddComponent<P3dPaintableTexture>();

            P3dSlot p3dSlot_rustDirt = new P3dSlot(l2Material_index, "_L2MetallicRustDustSmoothness");
            P3dSlot p3dSlot_colorMap = new P3dSlot(l2Material_index, "_L2ColorMap");
            P3dSlot p3dSlot_dirt = new P3dSlot(alphaMaterial_index, "_MainTex");

            // Setting up the components

            // Material cloner
            materialCloner_l2.Index = l2Material_index;
            materialCloner_paint.Index = alphaMaterial_index;

            // Paintable textures
            paintableTexture_colorMap.Slot = p3dSlot_colorMap;

            paintableTexture_rust.Slot = p3dSlot_rustDirt;
            paintableTexture_rust.Group = 100;

            paintableTexture_dirt.Slot = p3dSlot_dirt;
            paintableTexture_dirt.Group = 5;
            
            CheckHighResolutionPaint(part, paintableTexture_colorMap);

            TryApplyL2Overwrite(part);
            // Final details
            part.Paintable = true;
            part.CarProps.Paintable = true;
            part.CarProps.Washable = true;
            part.CarProps.Fairable = true;
            part.CarProps.DMGdeformMesh = true; // NOTE! As a side effect this will enable mesh deform on crashes.
        }

        public static void CheckHighResolutionPaint(Part p, P3dPaintableTexture texture)
        {
            if(p.Mod != null)
            {
                switch(p.Mod.Settings.PaintResolution)
                {
                    case PartPaintResolution.High:
                        texture.Width = 2048;
                        texture.Height = 2048;
                        break;
                    case PartPaintResolution.Medium:
                        texture.Width = 1024;
                        texture.Height = 1024;
                        break;
                    case PartPaintResolution.Low:
                        texture.Width = 512;
                        texture.Height = 512;
                        break;
                }
            }
        }

        public static Material GetDirtMaterial()
        {
            if (!DirtMaterial)
            {
                foreach (GameObject go in PartManager.gameParts)
                {
                    if (go != null)
                    {
                        if (go.name == "DoorFR06")
                        {
                            DirtMaterial = go.GetComponent<Renderer>().materials[1];
                        }
                    }
                }
            }

            if (!DirtMaterial)
            {
                CustomLogger.AddLine("PaintingSystem", $"GetDirtMaterial was not able to retrive a dirt material. Make sure you are using it on FirstLoad event.");
            }
            return DirtMaterial;
        }

        public static Material GetPaintRustMaterial()
        {
            if (!PaintRustMaterial)
            {
                foreach (GameObject go in PartManager.gameParts)
                {
                    if (go != null)
                    {
                        if (go.name == "DoorFR06")
                        {
                            PaintRustMaterial = go.GetComponent<Renderer>().materials[0];
                        }
                    }
                }
            }

            if (!PaintRustMaterial)
            {
                CustomLogger.AddLine("PaintingSystem", $"GetPaintRustMaterial was not able to retrive a paint-rust material. Make sure you are using it on FirstLoad event.");
            }
            return PaintRustMaterial;
        }

        public static Material GetBodymatMaterial()
        {
            return GetBodymatMaterial(false);
        }
        
        public static Material GetBodymatMaterial(bool useBackfaceShader = false)
        {
            if (!BaseMaterial)
            {
                foreach (GameObject go in PartManager.gameParts)
                {
                    if (go != null)
                    {
                        if (go.name == "DoorFR06")
                        {
                            BaseMaterial = go.GetComponent<Renderer>().materials[2];
                        }
                    }
                }
            }

            if (!BaseMaterial)
            {
                CustomLogger.AddLine("PaintingSystem", $"GetBodymatMaterial was not able to retrive the body material. Make sure you are using it on FirstLoad event.");
            }
            
            return useBackfaceShader ? CullBaseMaterial : BaseMaterial;
        }

        public static Material GetChromeMaterial()
        {
            if(!ChromeMaterial)
            {
                foreach (GameObject go in PartManager.gameParts)
                {
                    if (go != null)
                    {
                        if (go.name == "DoorFR06")
                        {
                            ChromeMaterial = go.GetComponent<CarProperties>().ChromeMat;
                            ChromeMaterial.shader = Shader.Find("Azerilo/Double Sided Standard");
                        }
                    }
                }
            }
            
            if (!ChromeMaterial)
            {
                CustomLogger.AddLine("PaintingSystem", $"GetChromeMaterial was not able to retrive the chrome material. Make sure you are using it on FirstLoad event.");
            }
            return ChromeMaterial;
        }

        [Obsolete("Use SetupPart method instead")]
        /// <summary>
        /// Sets the painting materials for a given Part. If index is -1 no material is affected for that type
        /// </summary>
        /// <param name="p">The part to get the painting</param>
        /// <param name="bodymatIndex">Not to be confused with the paintable material. Black material that hides the one-sided paintable material</param>
        /// <param name="paintRustIndex">Should be always at index 0, paintable material</param>
        /// <param name="dirtIndex">Should be always at index 1, dirt material</param>
        public static void SetMaterialsForObject(Part p, int bodymatIndex = -1, int paintRustIndex = -1, int dirtIndex = -1)
        {
            SetMaterialsForObject(p, bodymatIndex, paintRustIndex, dirtIndex, false);
        }

        [Obsolete("Use SetupPart method instead")]
        public static void SetMaterialsForObject(Part p, int bodymatIndex = -1, int paintRustIndex = -1, int dirtIndex = -1, bool force = true)
        {
            Material[] matsOfPart = p.Renderer.materials;
            if(force && matsOfPart.Length < 3)
            {
                matsOfPart = new Material[3];
            }

            if (bodymatIndex != -1)
            {
                if (matsOfPart.Length <= bodymatIndex)
                {
                    CustomLogger.AddLine("PaintingSystem", $"SetMaterialsForObject tried to setup bodymat index {bodymatIndex} on part {p.Prefab.name} but it only has {matsOfPart.Length} materials.");
                    return;
                }
                bool shader = false;
                if (p.Mod != null)
                {
                    if (p.ForceShaderStatus != ShaderSettings.NONE)
                    {
                        shader = p.ForceShaderStatus == ShaderSettings.FORCE_BACKSIDE;
                    }
                    else
                    {
                        shader = p.Mod.Settings.UseBackfaceShader;
                    }
                }
                matsOfPart[bodymatIndex] = GetBodymatMaterial(shader);
            }

            if (paintRustIndex != -1)
            {
                if (matsOfPart.Length <= paintRustIndex)
                {
                    CustomLogger.AddLine("PaintingSystem", $"SetMaterialsForObject tried to setup paint/rust index {paintRustIndex} on part {p.Prefab.name} but it only has {matsOfPart.Length} materials.");
                    return;
                }
                matsOfPart[paintRustIndex] = GetPaintRustMaterial();
            }

            if (dirtIndex != -1)
            {
                if (matsOfPart.Length <= dirtIndex)
                {
                    CustomLogger.AddLine("PaintingSystem", $"SetMaterialsForObject tried to setup dirt index {dirtIndex} on part {p.Prefab.name} but it only has {matsOfPart.Length} materials.");
                    return;
                }
                matsOfPart[dirtIndex] = GetDirtMaterial();
            }

            p.Renderer.materials = matsOfPart;
        }

        public static void SetupPart(Part p, PaintingSystem.Types type, bool dontUseBlack = false)
        {
            switch (type)
            {
                case Types.FullPaintingSupport:
                    SetMaterialsForObject(p, dontUseBlack ? -1 : 2, 0, 1, true);
                    p.EnablePartPainting(type);
                    break;
                case Types.OnlyPaint:
                    SetMaterialsForObject(p, dontUseBlack ? -1 : 2, 0, -1, true);
                    p.EnablePartPainting(type);
                    break;
                case Types.OnlyDirt:
                    SetMaterialsForObject(p, -1, -1, 1, true);
                    p.EnablePartPainting(type);
                    break;
                case Types.OnlyPaintAndDirt:
                    SetMaterialsForObject(p, dontUseBlack ? -1 : 2, 0, 1, true);
                    p.EnablePartPainting(type);
                    break;
                case Types.OnlyPaintAndRust:
                    SetMaterialsForObject(p, dontUseBlack ? -1 : 2, 0, -1, true);
                    p.EnablePartPainting(type);
                    break;
            }
        }

        internal static void TryApplyL2Overwrite(Part p)
        {
            TryApplyL2Overwrite(p.Prefab);
        }

        internal static void TryApplyL2Overwrite(GameObject go)
        {
            L2PaintOverwrite comp = go.GetComponent<L2PaintOverwrite>();
            if (!comp)
                return;

            Material matToChange = go.GetComponent<Renderer>().material;
            if (!matToChange || matToChange.shader.name != "Thunderbyte/RustDirt2Layers URP")
                return;

            if(comp._texcoord)
                matToChange.SetTexture("_texcoord", comp._texcoord);

            if (comp._GrungeMap)
                matToChange.SetTexture("_GrungeMap", comp._GrungeMap);

            if (comp._L1ColorMap)
                matToChange.SetTexture("_L1ColorMap", comp._L1ColorMap);

            if (comp._L1EmissionMap)
                matToChange.SetTexture("_L1EmissionMap", comp._L1EmissionMap);

            if (comp._L1MetallicRustDustSmoothness)
                matToChange.SetTexture("_L1MetallicRustDustSmoothness", comp._L1MetallicRustDustSmoothness);

            if (comp._L1Normal)
                matToChange.SetTexture("_L1Normal", comp._L1Normal);

            if (comp._L2EmissionMap)
                matToChange.SetTexture("_L2EmissionMap", comp._L2EmissionMap);

            if (comp._L2MetallicRustDustSmoothness)
                matToChange.SetTexture("_L2MetallicRustDustSmoothness", comp._L2MetallicRustDustSmoothness);

            if (comp._L2Normal)
                matToChange.SetTexture("_L2Normal", comp._L2Normal);
        }
    }
}
