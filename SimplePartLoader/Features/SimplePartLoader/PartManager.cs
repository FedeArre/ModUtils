﻿using Assets.SimpleLocalization;
using KCC;
using RVP;
using SimplePartLoader.CarGen;
using SimplePartLoader.Features.StartOptionBuilder;
using SimplePartLoader.Objects;
using SimplePartLoader.Objects.EditorComponents;
using SimplePartLoader.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.ConstrainedExecution;
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
        /// A list that contains all objects from the game. This is exposed since ModUtils v1.1.0. It is not recommended to use this list unless required.
        /// </summary>
        public static List<GameObject> gameParts;

        internal static bool ThumbnailGeneratorEnabled = false;

        internal static List<string> CarCategoriesToAdd = new List<string>();
        
        internal static List<string> EngineCategoriesToAdd = new List<string>();
        /// <summary>
        /// Handles the OnLoad function when called.
        /// </summary>
        internal static void OnLoadCalled()
        {
            // We first load all our parts into the list.
            gameParts = new List<GameObject>();
            foreach (GameObject part in GameObject.Find("PartsParent").GetComponent<JunkPartsList>().Parts)
            {
                if (part != null && part.tag != "Item")
                    gameParts.Add(part);
                else if (part != null && part.tag == "Item")
                {
                    // Handle special case to get brake shoes / pads for new cars that only come on box.
                    PickupTool pt = part.GetComponent<PickupTool>();
                    if (pt && pt.Box)
                    {
                        gameParts.Add(pt.InBoxprefab);
                    }
                }
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

            if(!GameObject.Find("ModLoader").GetComponent<EACheck>())
            {
                CustomLogger.AddLine("Main", "EA check component was not present");
                return;
            }

            SPL.DevLog("Starting first load check");
            // We need to check if this is the first load.
            if (!hasFirstLoadOccured)
            {
                SPL.DevLog("First load of the game");
                // We create our parts from the prefab generator
                LoadPrefabGeneratorParts();
                SPL.DevLog("Finished prefab generator stuff - Now invoking first load");

                try
                {
                    SPL.InvokeFirstLoadEvent(); // We call the FirstLoad event. SPL handles it since is the class that has the delegate.
                }
                catch(Exception ex)
                {
                    CustomLogger.AddLine("Parts", ex);
                    return;
                }

                SPL.DevLog("Part registering");
                
                // Now we add all our dummy parts into modLoadedParts and do a small safety check to see if all our parts are fine.
                foreach (Part part in dummyParts)
                    modLoadedParts.Add(part);

                foreach (Part part in prefabGenParts)
                    modLoadedParts.Add(part);

                foreach (Part part in modLoadedParts.ToList()) // Using toList allows to remove the part if required without errors. May not be the most efficent solution.
                {
                    if (!part.Prefab.GetComponent<CarProperties>() || !part.Prefab.GetComponent<Partinfo>())
                    {
                        CustomLogger.AddLine("Parts", $"The part {part.Prefab.name} ({part.PartType}) has a missing component when trying to load it to the game.");
                        modLoadedParts.Remove(part);
                        continue;
                    }

                    if(part.Mod != null)
                    {
                        if(part.Mod.RequiresSteamCheck && !part.Mod.Checked)
                        {
                            CustomLogger.AddLine("PartsEA", $"Removing {part.Prefab.name}");
                            GameObject.Destroy(part.Prefab);
                            modLoadedParts.Remove(part);
                        }
                    }

                    if (!part.Prefab.GetComponent<SPL_Part>())
                    {
                        part.Prefab.AddComponent<SPL_Part>().Mod = part.Mod;
                    }

                    MaterialSetup ms = part.Prefab.GetComponent<MaterialSetup>();
                    if (ms)
                    {
                        if (ms.SetPartToBlackMaterial)
                            part.Renderer.material = PaintingSystem.GetBodymatMaterial(part.Mod.Settings.UseBackfaceShader);

                        if (ms.EnableChromeStationSupport)
                            part.CarProps.ChromeMat = PaintingSystem.GetChromeMaterial();

                        if (ms.SupportType != PaintTypes.DontAdd)
                            PaintingSystem.SetupPart(part, (PaintingSystem.Types) ms.SupportType);

                        GameObject.Destroy(ms);
                    }
                }
            }
            SPL.DevLog("Injecting into catalog & localization stuff");

            // Parts catalog - We need to add our custom parts into the Junkyard part list since the parts catalog uses it as reference.
            GameObject junkyardListParent = GameObject.Find("PartsParent");
            GameObject carList = GameObject.Find("CarsParent"); // Car list of the game - Used for adding transparents
            GameObject[] cars = carList.GetComponent<CarList>().Cars;

            JunkPartsList jpl = junkyardListParent.GetComponent<JunkPartsList>();
            int sizeBeforeModify = jpl.Parts.Length;

            Array.Resize(ref jpl.Parts, sizeBeforeModify + modLoadedParts.Count); // We resize the array only once.

            if(CustomLogger.DebugEnabled)
            {
                CustomLogger.AddLine("Parts", "Parts catalog has been modified. New size: " + jpl.Parts.Length);
                foreach (Part p in modLoadedParts)
                {
                    CustomLogger.AddLine("Parts", $"Added part: {p.Name} (GameObject name: {p.Prefab}", true);
                }
            }

            foreach (Part p in modLoadedParts)
            {
                if(!p.Prefab)
                {
                    continue;
                }
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

            if (SPL.PREFAB_NAME_COLLISION_CHECK)
            {
                CustomLogger.AddLine("PrefabNameCollisionCheck", "Checking for prefab name collisions...");
                List<string> prefabNames = new List<string>();
                foreach (GameObject go in gameParts)
                {
                    CarProperties cp = go.GetComponent<CarProperties>();
                    if (!cp)
                        continue;
                    
                    if (prefabNames.Contains(cp.PrefabName))
                    {
                        CustomLogger.AddLine("PrefabNameCollisionCheck", $"Duplicate prefab name detected: {go.name} (prefab name: {cp.PrefabName})");
                    }
                    else
                    {
                        prefabNames.Add(cp.PrefabName);
                    }
                }
            }

            SPL.DevLog("Starting transparent attaching, transparents to attach: " + transparentData.Count);

            // We now load our transparents. We have to load them for the junkyard parts, car prefabs.
            foreach(TransparentData t in transparentData)
            {
                // We check the car part list for every possible part that has the transparent. This is slow but required for dummy part transparent attaching and will not impact FPS (Only loading time).
                foreach(GameObject part in gameParts)
                { 
                    if(t.AttachesTo == part.name)
                    {
                        bool preventTransparentCreation = false;

                        foreach(transparents transparent in part.GetComponentsInChildren<transparents>())
                        {
                            if(transparent.name == t.Name && transparent.SavePosition == t.SavePosition)
                            {
                                preventTransparentCreation = true;
                            }
                        }

                        if(preventTransparentCreation)
                        {
                            CustomLogger.AddLine("Transparents", $"Prevented transparent {t.Name} creation due to existing transparent with same save position ({t.SavePosition})");
                            break;
                        }

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
            {
                hasFirstLoadOccured = true;
                MainStartOptionBuilder.StartBuilder();
                MainCarGenerator.StartCarGen();
            }

            if(ThumbnailGeneratorEnabled)
            {
                GameObject PictureTake = new GameObject("ModUtils_Snapshoter");
                PictureTake.AddComponent<ModUtils_Snapshoter>();
            }

            // Load custom categories
            if(CarCategoriesToAdd.Count != 0 || EngineCategoriesToAdd.Count != 0)
            {
                CatalogueManager catalog = Resources.FindObjectsOfTypeAll<CatalogueManager>().First();
                
                foreach (string s in CarCategoriesToAdd)
                {
                    TMPro.TMP_Dropdown.OptionData newItem = new TMPro.TMP_Dropdown.OptionData();
                    newItem.text = s;
                    catalog.CarDropdown.options.Add(newItem);
                }

                foreach (string s in EngineCategoriesToAdd)
                {
                    TMPro.TMP_Dropdown.OptionData newItem = new TMPro.TMP_Dropdown.OptionData();
                    newItem.text = s;
                    catalog.EngineDropdown.options.Add(newItem);
                }
            }

            MainCarGenerator.AddCars();
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

            DestroyConsideringSetting(t.Owner, transparentObject.GetComponent<BoxCollider>());
            DestroyConsideringSetting(t.Owner, transparentObject.GetComponent<Renderer>());

            transparentObject.name = t.Name; // Renamed prefab is the one that the game uses for looking for transparent. Prefab name in car props is used for identify which prefab has to be loaded.

            transparentObject.tag = "transparentpart";
            transparentObject.layer = LayerMask.NameToLayer("TransparentParts");

            transparents transparentComponent = transparentObject.AddComponent<transparents>();
            
            // We now load data that may vary to our component.
            transparentComponent.ATTACHABLES = t.AttachingObjects;
            transparentComponent.DEPENDANTS = t.DependantObjects;
            transparentComponent.SavePosition = t.SavePosition;

            if(t.MeshToUse)
            {
                transparentObject.GetComponent<MeshFilter>().sharedMesh = t.MeshToUse;
            }

            if (t.PartThatNeedsToBeOff != null)
            {
                transparentComponent.PartThatNeedsToBeOffname = t.PartThatNeedsToBeOff;
                transparentComponent.PartHaveToBeREmoved = true;
            }

            if (t.TestingEnabled)
            {
                CustomLogger.AddLine("TransparentEditor", $"{t.Name} ({t.Owner.Name}) has the transparent editor enabled");
                if(t.Owner.Mod != null)
                    CustomLogger.AddLine("TransparentEditor", $"Part added by mod {t.Owner.Mod.Name}");
                
                if(t.Owner.CarProps)
                    CustomLogger.AddLine("TransparentEditor", $"Part prefab name: {t.Owner.CarProps.PrefabName} ({t.Owner.CarProps.PartName})");

                transparentObject.AddComponent<TransparentEdit>().transparentData = t;
            }
            
            return transparentObject;
        }

        internal static void LoadPrefabGeneratorParts()
        {
            foreach (Part part in prefabGenParts)
            {;
                // We first get the data from our part
                PrefabGenerator data = part.Prefab.GetComponent<PrefabGenerator>();

                CustomLogger.AddLine("Debug", "Prefab generator trying to generate " + data.PrefabName);

                // We convert all our HexNut / FlatNut / BoltNut / WeldCut to be supported by the game.
                // This is to allow developers to use either the "Mark as [..]" components or work directly with the components
                foreach (Transform t in part.GetTransforms())
                {
                    if (t.GetComponent<HexNut>())
                    {
                        HexNut hx = t.GetComponent<HexNut>();
                        CarProperties cp = hx.gameObject.AddComponent<CarProperties>();
                        cp.Attached = true;
                        cp.DMGdisplacepart = part.BoltDisplacement;
                        
                        hx.gameObject.AddComponent<DISABLER>();

                        hx.gameObject.layer = LayerMask.NameToLayer("Bolts");
                        hx.tight = true;

                        hx.gameObject.AddComponent<InternalMarker>();

                        if (!hx.GetComponent<BoxCollider>())
                            hx.gameObject.AddComponent<BoxCollider>();
                    }
                    else if (t.GetComponent<BoltNut>())
                    {
                        BoltNut bn = t.GetComponent<BoltNut>();
                        CarProperties cp = bn.gameObject.AddComponent<CarProperties>();
                        cp.Attached = true;
                        cp.DMGdisplacepart = part.BoltDisplacement;
                        
                        bn.gameObject.AddComponent<DISABLER>();

                        bn.gameObject.layer = LayerMask.NameToLayer("Bolts");
                        bn.tight = true;

                        bn.gameObject.AddComponent<InternalMarker>();

                        if (!bn.GetComponent<BoxCollider>())
                            bn.gameObject.AddComponent<BoxCollider>();
                    }
                    else if (t.GetComponent<FlatNut>())
                    {
                        FlatNut fn = t.GetComponent<FlatNut>();
                        CarProperties cp = fn.gameObject.AddComponent<CarProperties>();
                        cp.Attached = true;
                        cp.DMGdisplacepart = part.BoltDisplacement;
                        
                        fn.gameObject.AddComponent<DISABLER>();

                        fn.gameObject.layer = LayerMask.NameToLayer("FlatBolts");
                        fn.tight = true;

                        fn.gameObject.AddComponent<InternalMarker>();

                        if (!fn.GetComponent<BoxCollider>())
                            fn.gameObject.AddComponent<BoxCollider>();
                    }
                    else if (t.GetComponent<WeldCut>())
                    {
                        WeldCut wc = t.GetComponent<WeldCut>();
                        CarProperties cp = wc.gameObject.AddComponent<CarProperties>();
                        cp.Attached = true;
                        cp.DMGdisplacepart = part.BoltDisplacement;
                        
                        wc.gameObject.AddComponent<DISABLER>();

                        wc.gameObject.layer = LayerMask.NameToLayer("Weld");
                        wc.welded = true;

                        wc.gameObject.AddComponent<InternalMarker>();

                        if (!wc.GetComponent<MeshCollider>())
                            wc.gameObject.AddComponent<MeshCollider>().convex = true;
                    }
                }

                if(!data.EnableMeshChange && part.GetComponent<MeshFilter>())
                {
                    part.ReportIssue($"Wrong setup in Prefab Generator - Has a MeshFilter component but EnableMeshChange is set to false.");
                    continue;
                }
                
                // We clone to our prefab
                SPL.CopyPartToPrefab(part, data.CopiesFrom, data.EnableMeshChange);

                if (!part.CarProps)
                {
                    CustomLogger.AddLine("Parts", $"Prefab generator was unable to create {part.Name}");
                    continue;
                }

                // Now we remove all specific childs / move them.
                foreach(ChildDestroy cd in part.Prefab.GetComponentsInChildren<ChildDestroy>())
                {
                    foreach(Transform t in part.GetTransforms())
                    {
                        if (t == part.Prefab.transform) continue;

                        if (cd.StartsWith)
                        {
                            if (t.name.StartsWith(cd.ChildName))
                                DestroyConsideringSetting(part, t.gameObject);
                        }
                        else if (cd.EndsWith)
                        {
                            if (t.name.EndsWith(cd.ChildName))
                                DestroyConsideringSetting(part, t.gameObject);
                        }
                        else
                        {
                            if (t.name == cd.ChildName)
                                DestroyConsideringSetting(part, t.gameObject);
                        }
                    }
                }
                
                foreach (ChildMove cd in part.Prefab.GetComponentsInChildren<ChildMove>())
                {
                    foreach (Transform t in part.GetTransforms())
                    {
                        if (cd.StartsWith)
                        {
                            if (t.name.StartsWith(cd.ChildName))
                            {
                                if(cd.Move)
                                {
                                    t.localPosition = cd.NewPosition;
                                }
                                
                                if(cd.Rotate)
                                {
                                    t.localEulerAngles = cd.NewRotation;
                                }
                            }
                        }
                        else if (cd.EndsWith)
                        {
                            if (t.name.EndsWith(cd.ChildName))
                            {
                                if (cd.Move)
                                {
                                    t.localPosition = cd.NewPosition;
                                }

                                if (cd.Rotate)
                                {
                                    t.localEulerAngles = cd.NewRotation;
                                }
                            }
                        }
                        else
                        {
                            if (t.name == cd.ChildName)
                            {
                                if (cd.Move)
                                {
                                    t.localPosition = cd.NewPosition;
                                }

                                if (cd.Rotate)
                                {
                                    t.localEulerAngles = cd.NewRotation;
                                }
                            }
                        }
                    }
                }
                
                // Setting part name if set
                if (!String.IsNullOrWhiteSpace(data.PartName))
                {
                    part.CarProps.PartName = data.PartName;
                    part.CarProps.PartNameExtension = "";
                }

                // Setting part price if is valid. Reuses the same if 0.
                if (data.NewPrice != 0)
                {
                    if (data.NewPrice < 0)
                        part.PartInfo.price += Math.Abs(data.NewPrice);
                    else
                        part.PartInfo.price = data.NewPrice;
                }

                part.PartInfo.DontShowInCatalog = !data.EnablePartOnCatalog;
                part.PartInfo.DontSpawnInJunyard = !data.EnablePartOnJunkyard;
                part.PartInfo.HingePivot = null; // Force reset of it (if required)

                // Mesh stuff
                if (data.EnableMeshChange)
                {
                    switch (data.UseMaterialsFrom)
                    {
                        case PrefabGenerator.MaterialSettingTypes.DummyOriginal:
                            Material[] dummyMats = part.GetDummyOriginal().GetComponent<Renderer>().materials;
                            if (dummyMats.Length == part.Renderer.materials.Length)
                                part.Renderer.materials = dummyMats;
                            else if (dummyMats.Length < part.Renderer.materials.Length)
                            {
                                Material[] newMats = part.Renderer.materials;
                                for (int i = 0; i < dummyMats.Length; i++)
                                    newMats[i] = dummyMats[i];
                                
                                part.Renderer.materials = newMats;
                            }
                            else
                            {
                                part.ReportIssue($"Prefab has {part.Renderer.materials.Length} materials but the dummy original has {dummyMats.Length}");
                                part.Renderer.materials = dummyMats;
                            }
                            break;
                            
                        case PrefabGenerator.MaterialSettingTypes.PaintingSetup:
                            PaintingSystem.SetMaterialsForObject(part, 2, 0, 1);
                            part.EnablePartPainting(PaintingSystem.Types.FullPaintingSupport);
                            break;
                    }

                    if (!data.GetComponent<MeshFilter>().sharedMesh.isReadable && (part.CarProps.Paintable || part.CarProps.DMGdeformMesh || part.CarProps.DMGdisplacepart))
                        part.ReportIssue($"Mesh is not readable and part is paintable or deformable.");
                }

                // To enable chroming on our part
                if (data.EnableChromed)
                {
                    part.CarProps.ChromeMat = PaintingSystem.GetChromeMaterial();
                }

                if (data.CatalogImage)
                {
                    if (data.CatalogImage.width < 500 || data.CatalogImage.height < 500)
                        part.ReportIssue($"Thumbnail is too small! Size has to be at least 500x500!");

                    part.PartInfo.Thumbnail = data.CatalogImage;
                }

                if (!String.IsNullOrWhiteSpace(data.RenamedPrefab))
                {
                    part.PartInfo.RenamedPrefab = data.RenamedPrefab;
                }

                if (data.SavingFeatureEnabled)
                    part.EnableDataSaving();

                switch (data.AttachmentType)
                {
                    case PrefabGenerator.AttachmentTypes.Prytool:
                        part.UsePrytoolAttachment();
                        break;

                    case PrefabGenerator.AttachmentTypes.Hand:
                        part.UseHandAttachment();
                        break;

                    case PrefabGenerator.AttachmentTypes.ForceUseMarkedBolts:
                    case PrefabGenerator.AttachmentTypes.UseMarkedBolts:
                        {
                            // We first remove all the FlatNut / HexNut / BoltNut on our part.
                            foreach (HexNut hx in part.Prefab.GetComponentsInChildren<HexNut>())
                            {
                                if (!hx.GetComponent<InternalMarker>())
                                    DestroyConsideringSetting(part, hx.gameObject);
                            }
                            foreach (FlatNut fn in part.Prefab.GetComponentsInChildren<FlatNut>())
                            {
                                if (!fn.GetComponent<InternalMarker>())
                                    DestroyConsideringSetting(part, fn.gameObject);
                            }

                            foreach (BoltNut bn in part.Prefab.GetComponentsInChildren<BoltNut>())
                            {
                                if (!bn.GetComponent<InternalMarker>())
                                    DestroyConsideringSetting(part, bn.gameObject);
                            }

                            foreach (WeldCut wc in part.Prefab.GetComponentsInChildren<WeldCut>())
                            {
                                if (!wc.GetComponent<InternalMarker>())
                                    DestroyConsideringSetting(part, wc.gameObject);
                            }
                            
                            // Now we need to convert our MarkAsFlatnut | MarkAsHexnut / MarkAsBoltnut to actual bolts.
                            foreach (MarkAsHexnut mhx in part.Prefab.GetComponentsInChildren<MarkAsHexnut>())
                                Functions.ConvertToHexnut(mhx.gameObject);

                            foreach (MarkAsFlatnut mfn in part.Prefab.GetComponentsInChildren<MarkAsFlatnut>())
                                Functions.ConvertToFlatNut(mfn.gameObject);

                            foreach (MarkAsBoltnut mbn in part.Prefab.GetComponentsInChildren<MarkAsBoltnut>())
                                Functions.ConvertToBoltNut(mbn.gameObject);

                            // Also, we make the part actually use bolts
                            if(data.AttachmentType == PrefabGenerator.AttachmentTypes.ForceUseMarkedBolts)
                            {
                                part.Prefab.layer = LayerMask.NameToLayer("Ignore Raycast");
                                part.Prefab.tag = "Untagged";

                                Pickup prefabPickup = part.Prefab.AddComponent<Pickup>();
                                prefabPickup.canHold = true;
                                prefabPickup.tempParent = GameObject.Find("hand");
                                prefabPickup.SphereCOl = GameObject.Find("SphereCollider");

                                if (part.Prefab.GetComponent<PickupHand>())
                                    DestroyConsideringSetting(part, part.Prefab.GetComponent<PickupHand>());

                                if (part.Prefab.GetComponent<PickupWindow>())
                                    DestroyConsideringSetting(part, part.Prefab.GetComponent<PickupWindow>());

                                if (part.Prefab.GetComponent<RemoveWindow>())
                                    DestroyConsideringSetting(part, part.Prefab.GetComponent<RemoveWindow>());

                                if (part.Prefab.GetComponent<PickupDoor>())
                                    DestroyConsideringSetting(part, part.Prefab.GetComponent<PickupDoor>());

                                if (part.Prefab.GetComponent<PickupSpring>())
                                    DestroyConsideringSetting(part, part.Prefab.GetComponent<PickupSpring>());
                                
                                if (part.Prefab.GetComponent<OpenDoor>())
                                    DestroyConsideringSetting(part, part.Prefab.GetComponent<OpenDoor>());
                            }
                            
                            break;
                        }
                }

                foreach (MarkAsTransparent markedTransparent in part.Prefab.GetComponentsInChildren<MarkAsTransparent>())
                {
                    TransparentData tempData = new TransparentData(markedTransparent.name, null, Vector3.zero, Quaternion.identity, false);
                    tempData.MeshToUse = part.GetComponent<MeshFilter>().sharedMesh;
                    tempData.SavePosition = markedTransparent.SavePosition;
                    tempData.Owner = part;
                    
                    GameObject transparentObject = GetTransparentReadyObject(tempData);

                    transparentObject.GetComponent<transparents>().Type = markedTransparent.Type;
                    
                    transparentObject.transform.SetParent(markedTransparent.transform.parent);
                    transparentObject.transform.localPosition = markedTransparent.transform.localPosition;
                    transparentObject.transform.localRotation = markedTransparent.transform.localRotation;
                    transparentObject.transform.localScale = markedTransparent.transform.localScale;
                    
                    DestroyConsideringSetting(part, markedTransparent.gameObject);
                }

                part.PrefabGenLoaded = true;
                // Destroy some stuff
                DestroyConsideringSetting(part, part.Prefab.GetComponent<PrefabGenerator>());
                
                foreach (var bn in part.Prefab.GetComponentsInChildren<MarkAsBoltnut>())
                    DestroyConsideringSetting(part, bn);
                foreach (var bn in part.Prefab.GetComponentsInChildren<MarkAsHexnut>())
                    DestroyConsideringSetting(part, bn);
                foreach (var bn in part.Prefab.GetComponentsInChildren<MarkAsFlatnut>())
                    DestroyConsideringSetting(part, bn);

                foreach (var bn in part.Prefab.GetComponentsInChildren<ChildMove>())
                    DestroyConsideringSetting(part, bn);
                foreach (var bn in part.Prefab.GetComponentsInChildren<ChildDestroy>())
                    DestroyConsideringSetting(part, bn);
            }
        }

        public static void DestroyConsideringSetting(Part p, GameObject toDestroy)
        {
            if (p.Mod != null)
            {
                if (p.Mod.Settings.EnableImmediateDestroys)
                    GameObject.DestroyImmediate(toDestroy);
                else
                    GameObject.Destroy(toDestroy);
            }
            else
                GameObject.Destroy(toDestroy);
        }
        
        public static void DestroyConsideringSetting(Part p, Component toDestroy)
        {
            if (p.Mod != null)
            {
                if (p.Mod.Settings.EnableImmediateDestroys)
                    GameObject.DestroyImmediate(toDestroy);
                else
                    GameObject.Destroy(toDestroy);
            }
            else
                GameObject.Destroy(toDestroy);
        }
    }
}
