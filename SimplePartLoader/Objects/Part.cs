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
        public void EnablePaintSupport(int paintMaterialIndex, int rustMaterialIndex = -1, int washMaterialIndex = -1)
        {
            if(Paintable || Prefab.GetComponent<P3dPaintable>())
            {
                Debug.LogError($"[SPL]: Tried to use EnablePaintSupport on {Name} but already has painting components.");
                return;
            }

            P3dPaintable p3dPaintable = Prefab.AddComponent<P3dPaintable>();

            // Material checks
            Renderer prefabRenderer = Prefab.GetComponent<Renderer>();
            if(prefabRenderer.materials.Length > paintMaterialIndex)
            {
                Debug.LogError($"[SPL]: Invalid paint material index on {Name}.");
                return;
            }

            // Paint
            P3dPaintableTexture paintableTexture_paint = Prefab.AddComponent<P3dPaintableTexture>();
            P3dMaterialCloner materialCloner_paint = Prefab.AddComponent<P3dMaterialCloner>();
            P3dSlot p3dSlot_paint = new P3dSlot(paintMaterialIndex, "_MainTex");
            P3dChangeCounter counter_paint = Prefab.AddComponent<P3dChangeCounter>();

            paintableTexture_paint.Slot = p3dSlot_paint;
            materialCloner_paint.Index = paintMaterialIndex;
            counter_paint.PaintableTexture = paintableTexture_paint;
        }
    }
}
