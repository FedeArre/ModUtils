using PaintIn3D;
using SimplePartLoader.Utils;
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

        public void EnableFullPaintSupport()
        {
            PaintingSystem.EnableFullSupport(this, -1, -1);
        }

        /* 
            For painting:
            2 types of materials are required: Thunderbyte/RustDirt2Layers and Paint in 3D/Alpha (shaders)
            
            Group values are correct.
            Order: ColorMap, RustDirt, MainTex (alpha shader), Grunge map.
            Change counter can be disabled.

            
         */
        // Painting support
        /*public void EnablePaintSupport(int paintRustMaterial = -2, int dirtMaterial = -2)
        {
            
            

            

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
                
            }

            // Painting components
            // We need to add 3 paintable textures, 2 change counters, a color counter and a material cloner.
            
            
            
        }*/
    }
}
