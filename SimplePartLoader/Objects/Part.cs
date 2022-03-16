using PaintIn3D;
using System;
using System.Collections;
using UnityEngine;

namespace SimplePartLoader
{
    public class Part
    {
        public GameObject Prefab;
        public CarProperties CarProps;
        public Partinfo PartInfo;

        internal string Name;
        internal bool Paintable;

        internal Hashtable languages = new Hashtable();

        public Part(GameObject prefab, CarProperties carProp, Partinfo partinfo)
        {
            Prefab = prefab;
            CarProps = carProp;
            PartInfo = partinfo;
        }

        public void SetupTransparent(string attachesTo, Vector3 transparentLocalPos, Quaternion transaprentLocalRot, bool testingModeEnable = false)
        {
            PartManager.transparentData.Add(new TransparentData(PartInfo.RenamedPrefab, attachesTo, transparentLocalPos, transaprentLocalRot, testingModeEnable));
        }

        public void SetupTransparent(string attachesTo, Vector3 transparentLocalPos, Quaternion transaprentLocalRot, Vector3 scale, bool testingModeEnable = false)
        {
            PartManager.transparentData.Add(new TransparentData(PartInfo.RenamedPrefab, attachesTo, transparentLocalPos, transaprentLocalRot, scale, testingModeEnable));
        }

        public void Localize(string language, string newTranslation)
        {
            languages[language] = newTranslation;
        }

        public void UsePrytoolAttachment()
        {
            if(Prefab.GetComponent<Pickup>())
                GameObject.Destroy(Prefab.GetComponent<Pickup>());

            if (!Prefab.GetComponent<PickupWindow>())
            {
                Prefab.AddComponent<PickupWindow>();
                Prefab.AddComponent<RemoveWindow>();

                Prefab.layer = LayerMask.NameToLayer("Windows");
                Prefab.tag = "Window";

                // Also check if part has welds / bolts
                foreach(WeldCut wc in Prefab.GetComponentsInChildren<WeldCut>())
                {
                    GameObject.Destroy(wc.gameObject);
                }

                foreach (BoltNut bn in Prefab.GetComponentsInChildren<BoltNut>())
                {
                    GameObject.Destroy(bn.gameObject);
                }

                foreach (HexNut hn in Prefab.GetComponentsInChildren<HexNut>())
                {
                    GameObject.Destroy(hn.gameObject);
                }
            }
        }

        // Painting support
        /*public void EnablePaintSupport(int paintRustMaterial = -2, int dirtMaterial = -2)
        {
            if(Paintable || Prefab.GetComponent<P3dPaintable>())
            {
                Debug.LogError($"[SPL]: Tried to use EnablePaintSupport on {Prefab.name} but already has painting components.");
                return;
            }

            P3dPaintable p3dPaintable = Prefab.AddComponent<P3dPaintable>();

            // Material checks
            Renderer prefabRenderer = Prefab.GetComponent<Renderer>();
            if(prefabRenderer.materials.Length < paintMaterialIndex)
            {
                Debug.LogError($"[SPL]: Invalid material index for painting on {Prefab.name}.");
                return;
            }

            // Rust and dirt - Material check
            // We first need to check if the object has a rust-dirt material already created. If no, we have to create it.
            Material l2Material = null;
            int l2Material_index = -1;

            for(int i = 0; i < prefabRenderer.materials.Length; i++)
            {
                if(prefabRenderer.materials[i].shader.name == "Thunderbyte/RustDirt2Layers")
                {
                    Debug.LogError("Found material at index " + i);
                    l2Material = prefabRenderer.materials[i];
                    l2Material_index = i;
                    break;
                }
            }

            if (!l2Material)
            {
                l2Material = new Material(Shader.Find("Thunderbyte/RustDirt2Layers"));

                // Now we need to add this material to our object.
                Material[] newMaterialsArray = new Material[prefabRenderer.materials.Length+1];
                
                for(int i = 0; i < prefabRenderer.materials.Length; i++)
                {
                    newMaterialsArray[i] = prefabRenderer.materials[i];
                }

                newMaterialsArray[newMaterialsArray.Length - 1] = l2Material;
                l2Material_index = newMaterialsArray.Length - 1;
                prefabRenderer.materials = newMaterialsArray;
            }

            // Painting components
            // We need to add 3 paintable textures, 2 change counters, a color counter and a material cloner.
            P3dMaterialCloner materialCloner_l2 = Prefab.AddComponent<P3dMaterialCloner>();

            P3dPaintableTexture paintableTexture_colorMap = Prefab.AddComponent<P3dPaintableTexture>();
            P3dPaintableTexture paintableTexture_rustDirt = Prefab.AddComponent<P3dPaintableTexture>();
            
            P3dMaterialCloner materialCloner_paint = Prefab.AddComponent<P3dMaterialCloner>();
            P3dPaintableTexture paintableTexture_paint = Prefab.AddComponent<P3dPaintableTexture>();
            P3dPaintableTexture paintableTexture_grungeMap = Prefab.AddComponent<P3dPaintableTexture>();
            
            P3dChangeCounter counter_paint = Prefab.AddComponent<P3dChangeCounter>();
            P3dChangeCounter counter_rustDirt = Prefab.AddComponent<P3dChangeCounter>();
            P3dChangeCounter counter_colorMap = Prefab.AddComponent<P3dChangeCounter>();

            P3dSlot p3dSlot_rustDirt = new P3dSlot(l2Material_index, "_L2MetallicRustDustSmoothness");
            P3dSlot p3dSlot_colorMap = new P3dSlot(l2Material_index, "_L2ColorMap");
            P3dSlot p3dSlot_grungeMap = new P3dSlot(l2Material_index, "_GrungeMap");
            P3dSlot p3dSlot_paint = new P3dSlot(paintMaterialIndex, "_MainTex");
            
            Debug.LogError($"Setting things up. pmi is {paintMaterialIndex} l2 {l2Material_index}");

            // Setting up the components
            paintableTexture_colorMap.Slot = p3dSlot_colorMap;

            paintableTexture_grungeMap.Slot = p3dSlot_grungeMap;
            paintableTexture_grungeMap.Group = 100;

            paintableTexture_rustDirt.Slot = p3dSlot_rustDirt;
            paintableTexture_rustDirt.Group = 100;
            
            paintableTexture_paint.Slot = p3dSlot_paint;
            paintableTexture_paint.Group = 5;

            materialCloner_l2.Index = l2Material_index;

            counter_rustDirt.PaintableTexture = paintableTexture_rustDirt;
            counter_colorMap.PaintableTexture = paintableTexture_colorMap;

            counter_rustDirt.Threshold = 0.5f;

            counter_colorMap.Threshold = 0.1f;


            materialCloner_paint.Index = paintMaterialIndex;
            
            counter_paint.PaintableTexture = paintableTexture_paint;
            counter_paint.Threshold = 0.7f;

            CarProps.Paintable = true;
            CarProps.Washable = true;
        }*/
    }
}
