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

        internal bool SavingEnabled;

        public Part(GameObject prefab, CarProperties carProp, Partinfo partinfo)
        {
            Prefab = prefab;
            CarProps = carProp;
            PartInfo = partinfo;
        }

        [Obsolete("SetupTransparent will be removed on SimplePartLoader 1.4, use AddTransparent instead!")]
        public void SetupTransparent(string attachesTo, Vector3 transparentLocalPos, Quaternion transaprentLocalRot, bool testingModeEnable = false)
        {
            TransparentData td = new TransparentData(PartInfo.RenamedPrefab, attachesTo, transparentLocalPos, transaprentLocalRot, testingModeEnable);
            PartManager.transparentData.Add(td);
        }

        [Obsolete("SetupTransparent will be removed on SimplePartLoader 1.4, use AddTransparent instead!")]
        public void SetupTransparent(string attachesTo, Vector3 transparentLocalPos, Quaternion transaprentLocalRot, Vector3 scale, bool testingModeEnable = false)
        {
            TransparentData td = new TransparentData(PartInfo.RenamedPrefab, attachesTo, transparentLocalPos, transaprentLocalRot, scale, testingModeEnable);
            PartManager.transparentData.Add(td);
        }
        
        public TransparentData AddTransparent(string attachesTo, Vector3 transparentLocalPos, Quaternion transaprentLocalRot, bool testingModeEnable = false)
        {
            TransparentData td = new TransparentData(PartInfo.RenamedPrefab, attachesTo, transparentLocalPos, transaprentLocalRot, testingModeEnable);
            PartManager.transparentData.Add(td);
            return td;
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
                RemoveAttachmentsFromPart();

                Prefab.AddComponent<PickupWindow>();
                Prefab.AddComponent<RemoveWindow>();

                Prefab.layer = LayerMask.NameToLayer("Windows");
                Prefab.tag = "Window";
            }
        }

        public void EnableDataSaving()
        {
            if (SavingEnabled)
                return;

            SavingEnabled = true;
            Prefab.AddComponent<SaveData>();
        }

        public void UseHandAttachment()
        {
            if (Prefab.GetComponent<Pickup>())
                GameObject.Destroy(Prefab.GetComponent<Pickup>());

            if (!Prefab.GetComponent<PickupHand>())
            {
                RemoveAttachmentsFromPart();

                Prefab.AddComponent<PickupHand>();
                Prefab.AddComponent<RemoveWindow>();

                Prefab.layer = LayerMask.NameToLayer("Windows");
                Prefab.tag = "Window";
            }
        }

        public void EnablePartPainting(SPL.PaintingSupportedTypes type, int paintMaterial = -1)
        {
            switch (type)
            {
                case SPL.PaintingSupportedTypes.FullPaintingSupport:
                    PaintingSystem.EnableFullSupport(this);
                    break;

                case SPL.PaintingSupportedTypes.OnlyPaint:
                    PaintingSystem.EnablePaintOnly(this, paintMaterial);
                    break;

                case SPL.PaintingSupportedTypes.OnlyPaintAndRust:
                    PaintingSystem.EnablePaintAndRust(this);
                    break;

                case SPL.PaintingSupportedTypes.OnlyDirt:
                    PaintingSystem.EnableDirtOnly(this);
                    break;

                case SPL.PaintingSupportedTypes.OnlyPaintAndDirt:
                    PaintingSystem.EnablePaintAndDirt(this);
                    break;

                default:
                    Debug.LogError("[SPL]: An invalid type has been sent to Part.EnablePainting, part: " +Prefab.name);
                    break;
            }
        }

        private void RemoveAttachmentsFromPart()
        {
            foreach (WeldCut wc in Prefab.GetComponentsInChildren<WeldCut>())
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

            if(Prefab.GetComponent<RemoveWindow>())
                GameObject.Destroy(Prefab.GetComponent<RemoveWindow>());

            if(Prefab.GetComponent<PickupHand>())
                GameObject.Destroy(Prefab.GetComponent<PickupHand>());

            if(Prefab.GetComponent<PickupWindow>())
                GameObject.Destroy(Prefab.GetComponent<PickupWindow>());
        }
    }
}
