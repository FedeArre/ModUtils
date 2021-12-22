using Assets.SimpleLocalization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SimplePartLoader
{
    public class PartManager
    {
        public static List<Part> modLoadedParts = new List<Part>();
        public static Hashtable transparentData = new Hashtable(); // Using a list since a Part can be attached into multiple places

        internal static bool hasFirstLoadOccured = false;

        internal static void OnLoadCalled()
        {
            // Parts catalog - We need to first add our custom parts into the Junkyard part list since the parts catalog uses it as reference.
            GameObject junkyardListParent = GameObject.Find("PartsParent");
            GameObject carList = GameObject.Find("CarsParent"); // Car list of the game - Used for adding transparents
            GameObject[] cars = carList.GetComponent<CarList>().Cars;

            JunkPartsList jpl = junkyardListParent.GetComponent<JunkPartsList>();
            int sizeBeforeModify = jpl.Parts.Length;

            Array.Resize(ref jpl.Parts, sizeBeforeModify + modLoadedParts.Count); // We resize the array only once.

            foreach (Part p in modLoadedParts)
            {
                GameObject.DontDestroyOnLoad(p.Prefab);

                jpl.Parts[sizeBeforeModify] = p.Prefab;
                sizeBeforeModify++;

                if(!hasFirstLoadOccured)
                {
                    foreach(var dictionary in LocalizationManager.Dictionary)
                    {
                        dictionary.Value.Add(p.CarProps.PartName, p.CarProps.PartName);
                    }
                }
            }

            // Now we add our transparents into the game
            for(int i = 0; i < cars.Length; i++)
            {
                Transform[] childs = cars[i].GetComponentsInChildren<Transform>();

                foreach (Transform child in childs) // We check for every car part in the game
                {
                    TransparentData t = (TransparentData) transparentData[child.name];

                    if (t != null)
                    {
                        if (!child.GetComponent<transparents>()) // Add transparent into game for car prefabs.
                        {
                            GameObject transparentObject = GetTransparentReadyObject(t);
                            
                            transparentObject.transform.SetParent(carList.GetComponent<CarList>().Cars[i].transform.Find(GetTransformPath(child))); // Modify directly the object in the CarList

                            transparentObject.transform.localPosition = t.LocalPos;
                            transparentObject.transform.localScale = t.Scale;
                            transparentObject.transform.localRotation = t.LocalRot;
                        }

                        if (!hasFirstLoadOccured) // Add transparent into game resources for loading on saves.
                        {
                            if (cachedResources.Load(t.AttachesTo) != null) // Checking if valid AttachesTo has been given
                            {
                                GameObject transparentObject = GetTransparentReadyObject(t);
                                transparentObject.transform.SetParent(((GameObject)cachedResources.Load(t.AttachesTo)).transform);

                                transparentObject.transform.localPosition = t.LocalPos;
                                transparentObject.transform.localScale = t.Scale;
                                transparentObject.transform.localRotation = t.LocalRot;
                            }
                        }
                    }
                }
            }

            if (!hasFirstLoadOccured)
                hasFirstLoadOccured = true;
        }

        internal static GameObject GetTransparentReadyObject(TransparentData t)
        {
            GameObject transparentObject = GameObject.CreatePrimitive(PrimitiveType.Cube);

            GameObject.Destroy(transparentObject.GetComponent<BoxCollider>());

            transparentObject.name = t.Name; // Renamed prefab is the one that the game uses for looking for transparent. Prefab name in car props is used for identify which prefab has to be loaded.

            transparentObject.tag = "transparentpart";
            transparentObject.layer = LayerMask.NameToLayer("TransparentParts");

            transparents transparentComponent = transparentObject.AddComponent<transparents>();
            // We add dummy data so the component doesn't crash.
            transparentComponent.ATTACHABLES = new transparents.AttachingObjects[0];
            transparentComponent.DEPENDANTS = new transparents.dependantObjects[0];

            if (t.TestingEnabled)
               transparentObject.AddComponent<TransparentEdit>().transparentData = t;

            return transparentObject;
        }

        internal static string GetTransformPath(Transform transform)
        {
            string path = transform.name;
            while (transform.parent != null)
            {
                transform = transform.parent;
                if (transform.parent == null)
                    return path;

                path = transform.name + "/" + path;
            }

            return null; // Will never return this!
        }
    }
}
