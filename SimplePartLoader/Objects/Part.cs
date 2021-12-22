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

        internal bool Paintable;

        //internal Hashtable languages = new Hashtable();

        public Part(GameObject prefab, CarProperties carProp, Partinfo partinfo)
        {
            Prefab = prefab;
            CarProps = carProp;
            PartInfo = partinfo;

            /*foreach (var language in (SPL.Language[]) Enum.GetValues(typeof(SPL.Language)))
            {
                languages[SPL.GetLanguageByType(language)] = CarProps.PartName;
            }*/
        }

        public void SetupTransparent(string attachesTo, Vector3 transparentLocalPos, Quaternion transaprentLocalRot, bool testingModeEnable = false)
        {
            PartManager.transparentData[attachesTo] = new TransparentData(PartInfo.RenamedPrefab, attachesTo, transparentLocalPos, transaprentLocalRot, testingModeEnable);
        }

        public void SetupTransparent(string attachesTo, Vector3 transparentLocalPos, Quaternion transaprentLocalRot, Vector3 scale, bool testingModeEnable = false)
        {
            PartManager.transparentData[attachesTo] = new TransparentData(PartInfo.RenamedPrefab, attachesTo, transparentLocalPos, transaprentLocalRot, scale, testingModeEnable);
        }

        public void MakePartPaintable(int materialIndex, string slotTextureType = "_MainTex")
        {
            if (Paintable)
                return;

            Paintable = true;

            CarProps.Paintable = true;
            Prefab.AddComponent<P3dPaintable>();
            Prefab.AddComponent<P3dPaintableTexture>().Slot = new P3dSlot(materialIndex, slotTextureType);
            Prefab.GetComponent<P3dPaintableTexture>().UpdateMaterial();
            Prefab.AddComponent<P3dMaterialCloner>().Index = materialIndex;
        }

        /*internal string GetLocale(string key)
        {
            if(languages.ContainsKey(key))
                return (string) languages[key];

            return (string) languages["English"];
        }

        public void Localize(SPL.Language language, string newTranslation)
        {
            languages[SPL.GetLanguageByType(language)] = CarProps.PartName;
        }*/
    }
}
