using PaintIn3D;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SimplePartLoader
{
    public class PaintingSystem
    {
        public enum Types
        {
            FullPaintingSupport = 1,
            OnlyPaint,
            OnlyPaintAndRust,
            OnlyDirt,
            OnlyPaintAndDirt
        }

        internal static void EnablePaintOnly(Part part, int materialIndex)
        {
            GameObject Prefab = part.Prefab;

            if (part.Paintable || Prefab.GetComponent<P3dPaintable>())
            {
                Debug.LogError($"[SPL]: Tried to use EnablePaintSupport on {Prefab.name} but already has painting components.");
                return;
            }

            if(materialIndex == -1)
            {
                Prefab.AddComponent<P3dPaintable>();

                P3dPaintableTexture paintableTexture = Prefab.AddComponent<P3dPaintableTexture>();
                paintableTexture.Slot = new P3dSlot(materialIndex, "_MainTex");
                paintableTexture.UpdateMaterial();

                Prefab.AddComponent<P3dMaterialCloner>().Index = materialIndex;

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
                    if (prefabRenderer.materials[i].shader.name == "Thunderbyte/RustDirt2Layers")
                    {
                        l2Material_index = i;
                        break;
                    }
                }

                if (l2Material_index == -1)
                {
                    Debug.LogError("[SPL]: Missing Thunderbyte/RustDirt2Layers material (Paint & Rust) on part " + part.Prefab.name);
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
                Debug.LogError($"[SPL]: Tried to use EnablePaintSupport on {Prefab.name} but already has painting components.");
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
                Debug.LogError("[SPL]: Missing Thunderbyte/RustDirt2Layers material (Paint & Rust) on part " + part.Prefab.name);
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

            counter_colorMap.PaintableTexture = paintableTexture_colorMap;
            counter_colorMap.Threshold = 0.1f;
            counter_colorMap.enabled = false;

            // Final details
            part.Paintable = true;
            part.CarProps.Paintable = true;
            part.CarProps.DMGdeformMesh = true; // NOTE! As a side effect this will enable mesh deform on crashes.
        }


        internal static void EnableDirtOnly(Part part)
        {
            GameObject Prefab = part.Prefab;

            if (part.Paintable || Prefab.GetComponent<P3dPaintable>())
            {
                Debug.LogError($"[SPL]: Tried to use EnablePaintSupport on {Prefab.name} but already has painting components.");
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
                Debug.LogError("[SPL]: Missing Paint in 3D/Alpha material (Dirt) on part " + part.Prefab.name);
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

            // Final details
            part.Paintable = true;
            part.CarProps.Washable = true;
        }

        internal static void EnableFullSupport(Part part)
        {
            GameObject Prefab = part.Prefab;

            if (part.Paintable || Prefab.GetComponent<P3dPaintable>())
            {
                Debug.LogError($"[SPL]: Tried to use EnablePaintSupport on {Prefab.name} but already has painting components.");
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

            if(alphaMaterial_index == -1)
            {
                Debug.LogError("[SPL]: Missing Paint in 3D/Alpha material (Dirt) on part " + part.Prefab.name);
                return;
            }

            if(l2Material_index == -1)
            {
                Debug.LogError("[SPL]: Missing Thunderbyte/RustDirt2Layers material (Paint & Rust) on part " + part.Prefab.name);
                return;
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

            counter_colorMap.PaintableTexture = paintableTexture_colorMap;
            counter_colorMap.Threshold = 0.1f;
            counter_colorMap.enabled = false;

            counter_dirt.PaintableTexture = paintableTexture_dirt;
            counter_dirt.Threshold = 0.7f;
            counter_dirt.enabled = false;

            // Final details
            part.Paintable = true;
            part.CarProps.Paintable = true;
            part.CarProps.Washable = true;
            part.CarProps.DMGdeformMesh = true; // NOTE! As a side effect this will enable mesh deform on crashes.
        }

        internal static void EnablePaintAndDirt(Part part)
        {
            GameObject Prefab = part.Prefab;

            if (part.Paintable || Prefab.GetComponent<P3dPaintable>())
            {
                Debug.LogError($"[SPL]: Tried to use EnablePaintSupport on {Prefab.name} but already has painting components.");
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
                Debug.LogError("[SPL]: Missing Paint in 3D/Alpha material (Dirt) on part " + part.Prefab.name);
                return;
            }

            if (l2Material_index == -1)
            {
                Debug.LogError("[SPL]: Missing Thunderbyte/RustDirt2Layers material (Paint & Rust) on part " + part.Prefab.name);
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

            // Final details
            part.Paintable = true;
            part.CarProps.Paintable = true;
            part.CarProps.Washable = true;
            part.CarProps.DMGdeformMesh = true; // NOTE! As a side effect this will enable mesh deform on crashes.
        }

        public static Material GetDirtMaterial()
        {
            Material m = null;
            
            foreach(GameObject go in PartManager.gameParts)
            {
                if(go != null)
                {
                    if(go.name == "DoorFR06")
                    {
                        m = go.GetComponent<Renderer>().materials[1];
                    }
                }
            }

            if (!m)
            {
                Debug.LogError("[SPL]: GetDirtMaterial was not able to retrive a dirt material. Make sure you are using it on FirstLoad event.");
            }
            return m;
        }

        public static Material GetPaintRustMaterial()
        {
            Material m = null;

            foreach (GameObject go in PartManager.gameParts)
            {
                if (go != null)
                {
                    if (go.name == "DoorFR06")
                    {
                        m = go.GetComponent<Renderer>().materials[0];
                    }
                }
            }

            if (!m)
            {
                Debug.LogError("[SPL]: GetPaintRustMaterial was not able to retrive a paint/rust material. Make sure you are using it on FirstLoad event.");
            }
            return m;
        }

        public static Material GetBodymatMaterial()
        {
            Material m = null;

            foreach (GameObject go in PartManager.gameParts)
            {
                if (go != null)
                {
                    if (go.name == "DoorFR06")
                    {
                        m = go.GetComponent<Renderer>().materials[2];
                    }
                }
            }

            if (!m)
            {
                Debug.LogError("[SPL]: GetBodymatMaterial was not able to retrive the body material. Make sure you are using it on FirstLoad event.");
            }
            return m;
        }
    }
}
