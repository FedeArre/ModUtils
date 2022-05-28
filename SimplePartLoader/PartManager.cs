using Assets.SimpleLocalization;
using RVP;
using SimplePartLoader.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SimplePartLoader
{
    public class PartManager
    {
        /// <summary>
        /// List of all the loaded parts in the loader. It will not have the dummy parts until the first load has been completed.
        /// </summary>
        internal static List<Part> modLoadedParts = new List<Part>();

        /// <summary>
        /// List of all the dummy parts that are loaded in memory. It will be empty after the first load is completed.
        /// </summary>
        internal static List<Part> dummyParts = new List<Part>();

        internal static List<Part> prefabGenParts = new List<Part> ();

        /// <summary>
        /// List of all the transparent that are saved in memory.
        /// </summary>
        internal static List<TransparentData> transparentData = new List<TransparentData>();

        /// <summary>
        /// Used for triggering first load event.
        /// </summary>
        internal static bool hasFirstLoadOccured = false;

        /// <summary>
        /// A list that contains all objects from the game.
        /// </summary>
        internal static List<GameObject> gameParts;

        /// <summary>
        /// Handles the OnLoad function when called.
        /// </summary>
        internal static void OnLoadCalled()
        {
            // We first load all our parts into the list.
            gameParts = new List<GameObject>();
            foreach(GameObject part in GameObject.Find("PartsParent").GetComponent<JunkPartsList>().Parts)
            {
                gameParts.Add(part);
            }

            if (GameObject.Find("SHOPITEMS")) // Safety check for survival mode.
            {
                foreach(Transform part in GameObject.Find("SHOPITEMS").GetComponentsInChildren(typeof(Transform)))
                {
                    if (!part.GetComponent<SaleItem>())
                        continue;

                    if (part.GetComponent<SaleItem>().Item.GetComponent<CarProperties>())
                    {
                        gameParts.Add(part.GetComponent<SaleItem>().Item);
                    }
                }
            }
            
            // We need to check if this is the first load.
            if (!hasFirstLoadOccured)
            {
                // We create our parts from the prefab generator
                foreach(Part part in prefabGenParts)
                {
                    PrefabGenerator data = part.Prefab.GetComponent<PrefabGenerator>();

                    SPL.CopyPartToPrefab(part, data.CopiesFrom);
                    if(!part.CarProps)
                    {
                        Debug.LogError($"[SPL]: Prefab generator was unable to create {part.Name}");
                        continue;
                    }

                    if (data.NewMesh)
                    {
                        if(!data.NewMesh.GetComponent<MeshCollider>())
                        {
                            Debug.LogError($"[SPL]: New mesh of {part.Name} does not have mesh collider!");
                        }

                        part.Prefab.GetComponent<MeshFilter>().sharedMesh = data.NewMesh.GetComponent<MeshFilter>().sharedMesh;
                        part.Prefab.GetComponent<MeshCollider>().sharedMesh = data.NewMesh.GetComponent<MeshFilter>().sharedMesh;
                        part.Prefab.GetComponent<Renderer>().materials = data.NewMesh.GetComponent<Renderer>().materials;
                    }

                    if(!String.IsNullOrWhiteSpace(data.PartName))
                    {
                        part.CarProps.PartName = data.PartName;
                        part.CarProps.PartNameExtension = "";
                    }

                    if(data.NewPrice != 0)
                    {
                        if (data.NewPrice < 0)
                            part.PartInfo.price += data.NewPrice;
                        else
                            part.PartInfo.price = data.NewPrice;
                    }

                    part.PartInfo.DontShowInCatalog = !data.EnablePartOnCatalog;
                    part.PartInfo.DontSpawnInJunyard = !data.EnablePartOnJunkyard;

                    if(data.CatalogImage)
                    {
                        part.PartInfo.Thumbnail = data.CatalogImage;
                    }

                    if(data.SavingFeatureEnabled)
                        part.EnableDataSaving();

                    if(data.AttachmentType != PrefabGenerator.AttachmentTypes.Default)
                    {
                        if (data.AttachmentType == PrefabGenerator.AttachmentTypes.Prytool)
                            part.UsePrytoolAttachment();
                        else
                            part.UseHandAttachment();
                    }

                    Debug.Log($"[SPL]: Loaded {part.Name} (ingame: {part.CarProps.PartName}) through prefab generator");
                }

                try
                {
                    SPL.InvokeFirstLoadEvent(); // We call the FirstLoad event. SPL handles it since is the class that has the delegate.
                }
                catch(Exception ex)
                {
                    Debug.LogError("[SPL]: Something went wrong during first load event! FirstLoad execution has been stopped. Error: ");
                    Debug.LogError(ex.ToString());
                    return;
                }
                
                // Now we add all our dummy parts into modLoadedParts and do a small safety check to see if all our parts are fine.
                foreach (Part part in dummyParts)
                    modLoadedParts.Add(part);

                foreach (Part part in modLoadedParts.ToList()) // Using toList allows to remove the part if required without errors. May not be the most efficent solution.
                {
                    if (!part.Prefab.GetComponent<CarProperties>() || !part.Prefab.GetComponent<Partinfo>())
                    {
                        Debug.LogError($"[SPL] The part {part.Prefab.name} has a missing component.");
                        modLoadedParts.Remove(part);
                    }

                    if (!part.Prefab.GetComponent<SPL_Part>())
                    {
                        part.Prefab.AddComponent<SPL_Part>();
                    }
                }
            }
                
            // Parts catalog - We need to add our custom parts into the Junkyard part list since the parts catalog uses it as reference.
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
                gameParts.Add(p.Prefab);

                // Localization
                if(!hasFirstLoadOccured)
                {
                    if (p.languages["English"] == null)
                        p.languages["English"] = p.CarProps.PartName;

                    foreach(var dictionary in LocalizationManager.Dictionary)
                    {
                        if (dictionary.Value.ContainsKey(p.CarProps.PartName)) // Ignore case where the name is shared so the translation already exists
                            continue;

                        if (p.languages[dictionary.Key] != null)
                            dictionary.Value.Add(p.CarProps.PartName, (string)p.languages[dictionary.Key]);
                        else
                            dictionary.Value.Add(p.CarProps.PartName, (string)p.languages["English"]); // Fallback to english if no locale was set.
                    }
                }
            }

            SPL.DevLog("Starting transparent attaching, transparents to attach: " + transparentData.Count);

            // We know load our transparents. We have to load them for the junkyard parts, car prefabs.
            foreach(TransparentData t in transparentData)
            {
                // We check the car part list for every possible part that has the transparent. This is slow but required for dummy part transparent attaching and will not impact FPS (Only loading time).
                foreach(GameObject part in gameParts)
                { 
                    if(t.AttachesTo == part.name)
                    {
                        SPL.DevLog($"Internally attaching transparent to {t.AttachesTo} (for object {t.Name})");

                        GameObject transparentObject = GetTransparentReadyObject(t);
                        transparentObject.transform.SetParent(part.transform);

                        transparentObject.transform.localPosition = t.LocalPos;
                        transparentObject.transform.localScale = t.Scale;
                        transparentObject.transform.localRotation = t.LocalRot;
                    }
                }

                for (int i = 0; i < cars.Length; i++) // Now we need to also attach the part into the car prefabs (or it will not spawn in the game)
                {
                    Transform[] childs = cars[i].GetComponentsInChildren<Transform>();

                    foreach (Transform child in childs) // We check for every car part in the game
                    {
                        if(t.AttachesTo == child.name)
                        {
                            if(!child.GetComponent<transparents>())
                            {
                                GameObject transparentObject = GetTransparentReadyObject(t);
                                transparentObject.transform.SetParent(child);

                                SPL.DevLog($"Internally attaching transparent to {t.AttachesTo} (for object {t.Name}) (car-prefab variant)");

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

            SPL.InvokeLoadFinishedEvent();
        }

        /// <summary>
        /// Generates a GameObject to be used as transparent
        /// </summary>
        /// <param name="t">The TransparentData instance that has all the information about the object</param>
        /// <returns>An GameObject ready to be a transparent (with the respective tag, layer, name, transparents component and without colliders)</returns>
        public static GameObject GetTransparentReadyObject(TransparentData t)
        {
            GameObject transparentObject = GameObject.CreatePrimitive(PrimitiveType.Cube);

            GameObject.Destroy(transparentObject.GetComponent<BoxCollider>());
            GameObject.Destroy(transparentObject.GetComponent<Renderer>());

            transparentObject.name = t.Name; // Renamed prefab is the one that the game uses for looking for transparent. Prefab name in car props is used for identify which prefab has to be loaded.

            transparentObject.tag = "transparentpart";
            transparentObject.layer = LayerMask.NameToLayer("TransparentParts");

            transparents transparentComponent = transparentObject.AddComponent<transparents>();
            
            // We now load data that may vary to our component.
            transparentComponent.ATTACHABLES = t.AttachingObjects;
            transparentComponent.DEPENDANTS = t.DependantObjects;
            transparentComponent.SavePosition = t.SavePosition;

            if (t.PartThatNeedsToBeOff != null)
            {
                transparentComponent.PartThatNeedsToBeOffname = t.PartThatNeedsToBeOff;
                transparentComponent.PartHaveToBeREmoved = true;
            }

            if (t.TestingEnabled)
               transparentObject.AddComponent<TransparentEdit>().transparentData = t;

            return transparentObject;
        }
    }
}
