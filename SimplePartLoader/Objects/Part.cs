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

        public void EnablePainting(SPL.PaintingSupportedTypes type, int rustOrPaintMaterialIndex = -1, int alphaMaterialIndex = -1)
        {
            switch (type)
            {
                case SPL.PaintingSupportedTypes.FullPaintingSupport:
                    PaintingSystem.EnableFullSupport(this, rustOrPaintMaterialIndex, alphaMaterialIndex);
                    break;

                case SPL.PaintingSupportedTypes.OnlyPaint:
                    PaintingSystem.EnablePaintOnly(this, rustOrPaintMaterialIndex);
                    break;

                case SPL.PaintingSupportedTypes.OnlyPaintAndRust:
                    PaintingSystem.EnablePaintAndRust(this, rustOrPaintMaterialIndex);
                    break;

                case SPL.PaintingSupportedTypes.OnlyDirt:
                    PaintingSystem.EnableDirtOnly(this, alphaMaterialIndex);
                    break;

                default:
                    Debug.LogError("[SPL]: An invalid type has been sent to Part.EnablePainting, part: " +Prefab.name);
                    break;
            }
        }

        /*public void EnableFullPaintSupport(int rustDustMaterial = -1, int alphaMaterial = -1)
        {
            PaintingSystem.EnableFullSupport(this, rustDustMaterial, alphaMaterial);
        }*/
    }
}
