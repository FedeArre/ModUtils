using PaintIn3D;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SimplePartLoader.Utils
{
    internal class PaintingSystem
    {
        public static void EnablePaintOnly(Part part, int materialIndex)
        {
            GameObject Prefab = part.Prefab;

            if (part.Paintable || Prefab.GetComponent<P3dPaintable>())
            {
                Debug.LogError($"[SPL]: Tried to use EnablePaintSupport on {Prefab.name} but already has painting components.");
                return;
            }

            Prefab.AddComponent<P3dPaintable>();

            P3dPaintableTexture paintableTexture = Prefab.AddComponent<P3dPaintableTexture>();
            paintableTexture.Slot = new P3dSlot(materialIndex, "_MainTex");
            paintableTexture.UpdateMaterial();

            Prefab.AddComponent<P3dMaterialCloner>().Index = materialIndex;
        }

        public static void EnablePaintAndRust(Part part, int l2Index)
        {

        }

        public static void EnableDirtOnly(Part part, int alphaIndex)
        {

        }

        public static void EnableFullSupport(Part part, int l2Index, int alphaIndex)
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
                    Debug.LogError("Found material at index " + i);
                    l2Material_index = i;
                }

                if (prefabRenderer.materials[i].shader.name == "Paint in 3D/Alpha")
                {
                    Debug.LogError("Found material alpha at index " + i);
                    alphaMaterial_index = i;
                }
            }

            // We create a L2 material if the object does not have.
            if (l2Material_index == -1)
            {
                CreatePaintRustMaterial(prefabRenderer, l2Index);
                if (l2Index != -1)
                    l2Material_index = l2Index; // It was added to our specific index.
                else
                    l2Material_index = prefabRenderer.materials.Length - 1; // It was added to the end.
            }

            if (alphaMaterial_index == -1)
            {
                CreateAlphaMaterial(prefabRenderer, alphaMaterial_index);

                if (alphaIndex != -1)
                    alphaMaterial_index = alphaIndex; // It was added to our specific index.
                else
                    alphaMaterial_index = prefabRenderer.materials.Length - 1; // It was added to the end.
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

            Debug.LogError($"Setting things up. alpha is {alphaMaterial_index} l2 {l2Material_index}");

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

        /// <summary>
        /// Creates a L2 material and assigns it into a renderer.
        /// </summary>
        /// <param name="renderer">The renderer of the object</param>
        /// <param name="indexForMaterial">The index for the L2 material. If the index is -1 the material will be added as a new element of the renderer materials at the end.</param>
        public static void CreatePaintRustMaterial(Renderer renderer, int indexForMaterial = -1)
        {
            Debug.LogError("Creating L2 material for " + renderer);
            Material l2Material = new Material(Shader.Find("Thunderbyte/RustDirt2Layers"));

            // Now we need to add this material to our object.
            if(indexForMaterial == -1)
            {
                Material[] newMaterialsArray = new Material[renderer.materials.Length + 1];

                for (int i = 0; i < renderer.materials.Length; i++)
                {
                    newMaterialsArray[i] = renderer.materials[i];
                }

                newMaterialsArray[newMaterialsArray.Length - 1] = l2Material;
                renderer.materials = newMaterialsArray;
            }
            else
            {
                renderer.materials[indexForMaterial] = l2Material;
            }
        }

        /// <summary>
        /// Creates a Paintin3D/Alpha material and assigns it into a renderer.
        /// </summary>
        /// <param name="renderer">The renderer of the object</param>
        /// <param name="indexForMaterial">The index for the alpha material. If the index is -1 the material will be added as a new element of the renderer materials at the end.</param>
        public static void CreateAlphaMaterial(Renderer renderer, int indexForMaterial = -1)
        {
            Material alphaMaterial = new Material(Shader.Find("Paint in 3D/Alpha"));

            // Now we need to add this material to our object.
            if (indexForMaterial == -1)
            {
                Material[] newMaterialsArray = new Material[renderer.materials.Length + 1];

                for (int i = 0; i < renderer.materials.Length; i++)
                {
                    newMaterialsArray[i] = renderer.materials[i];
                }

                newMaterialsArray[newMaterialsArray.Length - 1] = alphaMaterial;
                renderer.materials = newMaterialsArray;
            }
            else
            {
                renderer.materials[indexForMaterial] = alphaMaterial;
            }
        }
    }
}
