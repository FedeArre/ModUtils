using Assets.SimpleLocalization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SimplePartLoader
{
    internal class PartManager
    {
        public static List<Part> modLoadedParts = new List<Part>();
        public static Hashtable transparentNames = new Hashtable();

        static bool translationsLoaded = false;

        public static void OnLoadCalled()
        {
            GameObject junkyardListParent = GameObject.Find("PartsParent");
            JunkPartsList jpl = junkyardListParent.GetComponent<JunkPartsList>();
            int sizeBeforeModify = jpl.Parts.Length;

            Array.Resize(ref jpl.Parts, sizeBeforeModify + modLoadedParts.Count);

            foreach (Part p in modLoadedParts)
            {
                GameObject.DontDestroyOnLoad(p.Prefab);

                jpl.Parts[sizeBeforeModify] = p.Prefab;
                sizeBeforeModify++;

                if (!translationsLoaded)
                    LocalizationManager.Dictionary["English"].Add(p.CarProps.PartName, p.CarProps.PartName + "" + p.CarProps.PartNameExtension);
            }

            translationsLoaded = true;
        }

        public static void OnUpdateCalled()
        {

        }
    }
}
