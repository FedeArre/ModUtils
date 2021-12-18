using Assets.SimpleLocalization;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SimplePartLoader
{
    internal class PartManager
    {
        public static List<Part> modLoadedParts = new List<Part>();

        static bool firstLoad = false;

        public static void OnLoadCalled()
        {
            // Parts catalog - We need to first add our custom parts into the Junkyard part list since the parts catalog uses it as reference.
            GameObject junkyardListParent = GameObject.Find("PartsParent");
            JunkPartsList jpl = junkyardListParent.GetComponent<JunkPartsList>();
            int sizeBeforeModify = jpl.Parts.Length;

            Array.Resize(ref jpl.Parts, sizeBeforeModify + modLoadedParts.Count); // We resize the array only once.

            foreach (Part p in modLoadedParts)
            {
                GameObject.DontDestroyOnLoad(p.Prefab);

                jpl.Parts[sizeBeforeModify] = p.Prefab;
                sizeBeforeModify++;

                if (!firstLoad) // First load: We need to add our transparents
                {
                    foreach(TransparentData t in p.transparentData)
                    {
                        if (cachedResources.Load(t.AttachesTo) != null) // Checking if valid AttachesTo has been given
                        {
                            GameObject transparentObject = GameObject.CreatePrimitive(PrimitiveType.Cube);

                            GameObject.Destroy(transparentObject.GetComponent<BoxCollider>());

                            transparentObject.name = p.PartInfo.RenamedPrefab;
                            
                            transparentObject.transform.localPosition = t.LocalPos;
                            transparentObject.transform.localScale = t.Scale;
                            transparentObject.transform.localRotation = t.LocalRot;

                            transparentObject.tag = "transparentpart";
                            transparentObject.layer = LayerMask.NameToLayer("TransparentParts");

                            transparents transparentComponent = transparentObject.AddComponent<transparents>();
                            // We add dummy data so the component doesn't crash.
                            transparentComponent.ATTACHABLES = new transparents.AttachingObjects[0];
                            transparentComponent.DEPENDANTS = new transparents.dependantObjects[0];

                            if (ModMain.IsTransparentEditingEnabled && p.TestingEnabled)
                                transparentObject.AddComponent<TransparentEdit>().transparentData = t;
                            

                            transparentObject.transform.SetParent(((GameObject)cachedResources.Load(t.AttachesTo)).transform); // We load the cached resource as GameObject and 
                        }
                    }

                    LocalizationManager.Dictionary["English"].Add(p.CarProps.PartName, p.CarProps.PartName + "" + p.CarProps.PartNameExtension);

                    firstLoad = true;
                }
            }

        }
    }
}
