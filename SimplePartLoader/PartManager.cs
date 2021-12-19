using Assets.SimpleLocalization;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SimplePartLoader
{
    internal class PartManager
    {
        public static List<Part> modLoadedParts = new List<Part>();
        public static Hashtable transparentData; // Using a list since a Part can be attached into multiple places

        static bool hasFirstLoadOccured = false;

        public static void OnLoadCalled()
        {
            // Parts catalog - We need to first add our custom parts into the Junkyard part list since the parts catalog uses it as reference.
            GameObject junkyardListParent = GameObject.Find("PartsParent");
            GameObject carList = GameObject.Find("CarsParent"); // Car list of the game - Used for adding transparents
            GameObject[] cars = carList.GetComponent<CarList>().Cars;

            JunkPartsList jpl = junkyardListParent.GetComponent<JunkPartsList>();
            int sizeBeforeModify = jpl.Parts.Length;

            Array.Resize(ref jpl.Parts, sizeBeforeModify + modLoadedParts.Count); // We resize the array only once.

            foreach(Part p in modLoadedParts)
            {
                GameObject.DontDestroyOnLoad(p.Prefab);

                jpl.Parts[sizeBeforeModify] = p.Prefab;
                sizeBeforeModify++;
            }

            // Now we add our transparents into the game
            foreach(GameObject car in cars)
            {
                Transform[] childs = car.GetComponentsInChildren<Transform>();

                foreach (Part p in modLoadedParts)
                {
                    foreach (Transform child in childs) // We have to check in every car part from the vehicles prefab
                    {
                        if (p.transparentData[child.name] != null) // We know that 
                        {
                            if (!child.GetComponent<transparents>())
                            {
                                TransparentData t = (TransparentData) p.transparentData[child.name];


                            }
                        }
                    }

                }
            }
        }

        internal static GameObject GetTransparentReadyObject(Part p, TransparentData t)
        {
            GameObject transparentObject = GameObject.CreatePrimitive(PrimitiveType.Cube);

            GameObject.Destroy(transparentObject.GetComponent<BoxCollider>());

            transparentObject.name = p.PartInfo.RenamedPrefab; // Renamed prefab is the one that the game uses for looking for transparent. Prefab name in car props is used for identify which prefab has to be loaded.

            transparentObject.transform.localPosition = t.LocalPos;
            transparentObject.transform.localScale = t.Scale;
            transparentObject.transform.localRotation = t.LocalRot;

            transparentObject.tag = "transparentpart";
            transparentObject.layer = LayerMask.NameToLayer("TransparentParts");

            transparents transparentComponent = transparentObject.AddComponent<transparents>();
            // We add dummy data so the component doesn't crash.
            transparentComponent.ATTACHABLES = new transparents.AttachingObjects[0];
            transparentComponent.DEPENDANTS = new transparents.dependantObjects[0];

            if (p.TestingEnabled)
                transparentObject.AddComponent<TransparentEdit>().transparentData = t;

            return transparentObject;
        }
    }
}
