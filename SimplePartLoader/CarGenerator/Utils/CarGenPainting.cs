using PaintIn3D;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SimplePartLoader.CarGen
{
    internal class CarGenPainting
    {
        internal static void EnablePaintOnly(GameObject Prefab, PaintingSystem.PartPaintResolution res)
        {
            if (Prefab.GetComponent<P3dPaintable>())
            {
                Debug.LogError($"[ModUtils/CarGen/PaintingSystem/Error]: Tried to use EnablePaintSupport on {Prefab.name} but already has painting components.");
                return;
            }

            Prefab.AddComponent<P3dPaintable>();

            // Material checks
            Renderer prefabRenderer = Prefab.GetComponent<Renderer>();
            int l2Material_index = -1;

            // Looking up the material or creating it.
            for (int i = 0; i < prefabRenderer.materials.Length; i++)
            {
                if (prefabRenderer.materials[i].shader.name == "Thunderbyte/RustDirt2Layers")
                {
                    l2Material_index = i;
                    break;
                }
            }

            if (l2Material_index == -1)
            {
                Debug.LogError("[ModUtils/CarGen/PaintingSystem/Error]: Missing Thunderbyte/RustDirt2Layers material (Paint & Rust) on part " + Prefab.name);
                return;
            }

            // Now we add all the painting components
            P3dMaterialCloner materialCloner_l2 = Prefab.AddComponent<P3dMaterialCloner>();

            P3dPaintableTexture paintableTexture_colorMap = Prefab.AddComponent<P3dPaintableTexture>();

            P3dChangeCounter counter_colorMap = Prefab.AddComponent<P3dChangeCounter>();

            P3dSlot p3dSlot_colorMap = new P3dSlot(l2Material_index, "_L2ColorMap");

            // Setting up the components

            // Material cloner
            materialCloner_l2.Index = l2Material_index;

            // Paintable textures
            paintableTexture_colorMap.Slot = p3dSlot_colorMap;

            counter_colorMap.PaintableTexture = paintableTexture_colorMap;
            counter_colorMap.Threshold = 0.1f;
            counter_colorMap.enabled = false;

            CheckHighResolutionPaint(Prefab, res);
        }

        internal static void EnablePaintAndRust(GameObject Prefab, PaintingSystem.PartPaintResolution res)
        {
            if (Prefab.GetComponent<P3dPaintable>())
            {
                Debug.LogError($"[ModUtils/CarGen/PaintingSystem/Error]: Tried to use EnablePaintSupport on {Prefab.name} but already has painting components.");
                return;
            }

            Prefab.AddComponent<P3dPaintable>();

            // Material checks
            Renderer prefabRenderer = Prefab.GetComponent<Renderer>();
            int l2Material_index = -1;

            // Looking up the material or creating it.
            for (int i = 0; i < prefabRenderer.materials.Length; i++)
            {
                if (prefabRenderer.materials[i].shader.name == "Thunderbyte/RustDirt2Layers")
                {
                    l2Material_index = i;
                    break;
                }
            }

            if (l2Material_index == -1)
            {
                Debug.LogError("[ModUtils/CarGen/PaintingSystem/Error]: Missing Thunderbyte/RustDirt2Layers material (Paint & Rust) on part " + Prefab.name);
                return;
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

            counter_colorMap.PaintableTexture = paintableTexture_colorMap;
            counter_colorMap.Threshold = 0.1f;
            counter_colorMap.enabled = false;

            CheckHighResolutionPaint(Prefab, res);
        }


        internal static void EnableDirtOnly(GameObject Prefab, PaintingSystem.PartPaintResolution res)
        {
            if (Prefab.GetComponent<P3dPaintable>())
            {
                Debug.LogError($"[ModUtils/CarGen/PaintingSystem/Error]: Tried to use EnablePaintSupport on {Prefab.name} but already has painting components.");
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

            if (alphaMaterial_index == -1)
            {
                Debug.LogError("[ModUtils/CarGen/PaintingSystem/Error]: Missing Paint in 3D/Alpha material (Dirt) on part " + Prefab.name);
                return;
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
        }

        internal static void EnableFullSupport(GameObject Prefab, PaintingSystem.PartPaintResolution res)
        {
            if (Prefab.GetComponent<P3dPaintable>())
            {
                Debug.LogError($"[ModUtils/CarGen/PaintingSystem/Error]: Tried to use EnablePaintSupport on {Prefab.name} but already has painting components.");
                return;
            }

            Prefab.AddComponent<P3dPaintable>();

            // Material checks
            Renderer prefabRenderer = Prefab.GetComponent<Renderer>();
            int l2Material_index = -1, alphaMaterial_index = -1;

            for (int i = 0; i < prefabRenderer.materials.Length; i++)
            {
                if (prefabRenderer.materials[i].shader.name == "Thunderbyte/RustDirt2Layers")
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
                Debug.LogError("[ModUtils/CarGen/PaintingSystem/Error]: Missing Paint in 3D/Alpha material (Dirt) on part " + Prefab.name);
                return;
            }

            if (l2Material_index == -1)
            {
                Debug.LogError("[ModUtils/CarGen/PaintingSystem/Error]: Missing Thunderbyte/RustDirt2Layers material (Paint & Rust) on part " + Prefab.name);
                return;
            }

            // Check for original mesh
            OriginalMesh orMesh = Prefab.GetComponent<OriginalMesh>();
            Mesh meshToUse = null;
            if (orMesh)
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

            CheckHighResolutionPaint(Prefab, res);
        }

        internal static void EnablePaintAndDirt(GameObject Prefab, PaintingSystem.PartPaintResolution res)
        {
            if (Prefab.GetComponent<P3dPaintable>())
            {
                Debug.LogError($"[ModUtils/CarGen/PaintingSystem/Error]: Tried to use EnablePaintSupport on {Prefab.name} but already has painting components.");
                return;
            }

            Prefab.AddComponent<P3dPaintable>();

            // Material checks
            Renderer prefabRenderer = Prefab.GetComponent<Renderer>();
            int l2Material_index = -1, alphaMaterial_index = -1;

            for (int i = 0; i < prefabRenderer.materials.Length; i++)
            {
                if (prefabRenderer.materials[i].shader.name == "Thunderbyte/RustDirt2Layers")
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
                Debug.LogError("[ModUtils/CarGen/PaintingSystem/Error]: Missing Paint in 3D/Alpha material (Dirt) on part " + Prefab.name);
                return;
            }

            if (l2Material_index == -1)
            {
                Debug.LogError("[ModUtils/CarGen/PaintingSystem/Error]: Missing Thunderbyte/RustDirt2Layers material (Paint & Rust) on part " + Prefab.name);
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

            CheckHighResolutionPaint(Prefab, res);
        }

        internal static void CheckHighResolutionPaint(GameObject Prefab, PaintingSystem.PartPaintResolution res)
        {
            P3dPaintableTexture texture = Prefab.GetComponent<P3dPaintableTexture>();
            
            switch(res)
            {
                case PaintingSystem.PartPaintResolution.High:
                    texture.Width = 2048;
                    texture.Height = 2048;
                    break;
                case PaintingSystem.PartPaintResolution.Medium:
                    texture.Width = 1024;
                    texture.Height = 1024;
                    break;
                case PaintingSystem.PartPaintResolution.Low:
                    texture.Width = 512;
                    texture.Height = 512;
                    break;
            }
        }
    }
    
}
