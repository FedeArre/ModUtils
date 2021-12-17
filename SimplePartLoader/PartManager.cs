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
        public static Hashtable transparentsData = new Hashtable();

        static bool translationsLoaded = false;
        static bool hasTransparentSet = false;

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
            if (tools.helditem == "Nothing" && hasTransparentSet)
                hasTransparentSet = false;

            else if(transparentsData[tools.helditem] != null)
            {
                hasTransparentSet = true;
                TransparentData transparentData = (TransparentData) transparentsData[tools.helditem];

                GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
                foreach (GameObject go in allObjects)
                {
                    if(go.name == transparentData.AttachesTo)
                    {
                        if (!go.GetComponent<transparents>())
                        {
                            bool hasTransparentAlready = false;
                            for (int i = 0; i < go.transform.childCount; i++)
                            {
                                if (go.transform.GetChild(i).name == transparentData.Name)
                                {
                                    hasTransparentAlready = true;
                                    break;
                                }
                            }

                            if (hasTransparentAlready)
                                continue;

                            GameObject transparentSpoiler = GameObject.CreatePrimitive(PrimitiveType.Cube);

                            UnityEngine.Object.Destroy(transparentSpoiler.GetComponent<BoxCollider>());

                            transparentSpoiler.name = transparentData.Name;
                            transparentSpoiler.transform.SetParent(go.transform);
                            transparentSpoiler.transform.localPosition = transparentData.LocalPos;
                            transparentSpoiler.transform.localScale = Vector3.one;
                            transparentSpoiler.transform.localRotation = transparentData.LocalRot;
                            transparentSpoiler.tag = "transparentpart";
                            transparentSpoiler.layer = 8; // layername is TransparentParts

                            transparents t = transparentSpoiler.AddComponent<transparents>();
                            t.ATTACHABLES = new transparents.AttachingObjects[0];
                            t.DEPENDANTS = new transparents.dependantObjects[0];
                        }
                    }
                }
        }
    }
}
