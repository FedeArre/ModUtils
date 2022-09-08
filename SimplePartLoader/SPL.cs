using Assets.SimpleLocalization;
using PaintIn3D;
using SimplePartLoader.Utils;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace SimplePartLoader
{
    public class SPL
    {
        public delegate void FirstLoadDelegate();
        public static event FirstLoadDelegate FirstLoad;

        public delegate void LoadFinishDelegate();
        public static event LoadFinishDelegate LoadFinish;

        public delegate void DataLoadedDelegate();
        public static event DataLoadedDelegate DataLoaded;

        public static bool DEVELOPER_LOG = false;
        public static bool ENABLE_SAVE_DISSASAMBLE = false;
        public static bool PREFAB_NAME_COLLISION_CHECK = false;
        
        // All availables paint types in the game
        [Obsolete("This enum will be removed in ModUtils v1.1.1, use PaintingSystem.Types instead!")]
        public enum PaintingSupportedTypes
        {
            FullPaintingSupport = 1,
            OnlyPaint,
            OnlyPaintAndRust,
            OnlyDirt,
            OnlyPaintAndDirt
        }

        /// <summary>
        /// Adds a prefab as a car part into the game
        /// </summary>
        /// <param name="bundle">The bundle in which the prefab is located. Has to be loaded!</param>
        /// <param name="prefabName">The name of the prefab to be loaded</param>
        /// <exception cref="Exception">An exception will be thrown if the bundle or prefabName are invalid, if the prefab already exists or if essential components are missing</exception>
        /// <returns></returns>
        [Obsolete("This method is deprecated, use ModInstance.Load instead")]
        public static Part LoadPart(AssetBundle bundle, string prefabName)
        {
            // Safety checks
            if (!bundle)
                SplError("Tried to create a part without valid AssetBundle");

            if (String.IsNullOrWhiteSpace(prefabName))
                SplError("Tried to create a part without prefab name");

            if (Saver.modParts.ContainsKey(prefabName))
                SplError($"Tried to create an already existing prefab ({prefabName})");

            GameObject prefab = bundle.LoadAsset<GameObject>(prefabName);
            if (!prefab)
                SplError($"Tried to create a prefab but it was not found in the AssetBundle ({prefabName})");

            CarProperties prefabCarProp = prefab.GetComponent<CarProperties>();
            Partinfo prefabPartInfo = prefab.GetComponent<Partinfo>();

            if (!prefabCarProp || !prefabPartInfo)
                SplError("An essential component is missing! (Prefab: " + prefabName + ")");

            // Automatically add some components and also assign the correct layer.
            // Pickup and DISABLER for the part - Required so they work properly!
            // Also add CarProperties to all nuts of the part, unexpected behaviour can happen if the component is missing.
            prefab.layer = LayerMask.NameToLayer("Ignore Raycast");

            Pickup prefabPickup = prefab.AddComponent<Pickup>();
            prefabPickup.canHold = true;
            prefabPickup.tempParent = GameObject.Find("hand");
            prefabPickup.SphereCOl = GameObject.Find("SphereCollider");

            prefab.AddComponent<DISABLER>();

            prefabCarProp.PREFAB = prefab; // Saving will not work without this due to a condition located in Saver.Save()
            
            foreach(HexNut hx in prefab.GetComponentsInChildren<HexNut>())
            {
                hx.gameObject.AddComponent<CarProperties>();
                hx.gameObject.AddComponent<DISABLER>();

                hx.gameObject.layer = LayerMask.NameToLayer("Bolts");
                hx.tight = true;
                
                if (!hx.GetComponent<BoxCollider>())
                        hx.gameObject.AddComponent<BoxCollider>();
            }

            foreach (BoltNut bn in prefab.GetComponentsInChildren<BoltNut>())
            {
                bn.gameObject.AddComponent<CarProperties>();
                bn.gameObject.AddComponent<DISABLER>();

                bn.gameObject.layer = LayerMask.NameToLayer("Bolts");
                bn.tight = true;

                if (!bn.GetComponent<BoxCollider>())
                    bn.gameObject.AddComponent<BoxCollider>();
            }

            foreach (FlatNut fn in prefab.GetComponentsInChildren<FlatNut>())
            {
                fn.gameObject.AddComponent<CarProperties>();
                fn.gameObject.AddComponent<DISABLER>();

                fn.gameObject.layer = LayerMask.NameToLayer("FlatBolts");
                fn.tight = true;
                
                if (!fn.GetComponent<BoxCollider>())
                    fn.gameObject.AddComponent<BoxCollider>();
            }

            foreach(WeldCut wc in prefab.GetComponentsInChildren<WeldCut>())
            {
                wc.gameObject.AddComponent<CarProperties>();
                wc.gameObject.AddComponent<DISABLER>();

                wc.gameObject.layer = LayerMask.NameToLayer("Weld");
                wc.welded = true;
                
                if (!wc.GetComponent<MeshCollider>())
                    wc.gameObject.AddComponent<MeshCollider>().convex = true;
            }

            Part p = new Part(prefab, prefabCarProp, prefabPartInfo, prefab.GetComponent<Renderer>(), null);
            PartManager.modLoadedParts.Add(p);

            Saver.modParts.Add(p.CarProps.PrefabName, prefab);

            GameObject.DontDestroyOnLoad(prefab); // We make sure that our prefab is not deleted in the first scene change
            Debug.Log("[ModUtils/SPL]: Succesfully loaded part (not-dummy) " + prefabName);
            return p; // We provide the Part instance so the developer can setup the transparents
        }

        /// <summary>
        /// Allows to load a dummy part into the memory to setup it on runtime
        /// </summary>
        /// <param name="bundle">The bundle in which the prefab is located. Has to be loaded!</param>
        /// <param name="prefabName">The name of the prefab to be loaded</param>
        /// <exception cref="Exception">An exception will be thrown if the bundle or prefabName are invalid or if the prefab already exists</exception>
        /// <returns>The Part instance</returns>
        [Obsolete("This method is deprecated, use ModInstance.Load instead")]
        public static Part LoadDummy(AssetBundle bundle, string prefabName)
        {
            return LoadDummy(bundle, prefabName, false);
        }

        /// <summary>
        /// Allows to load a dummy part into the memory to setup it on runtime
        /// </summary>
        /// <param name="bundle">The bundle in which the prefab is located. Has to be loaded!</param>
        /// <param name="prefabName">The name of the prefab to be loaded</param>
        /// <param name="betterCopy">Enables a more precise cloning method</param>
        /// <exception cref="Exception">An exception will be thrown if the bundle or prefabName are invalid or if the prefab already exists</exception>
        /// <returns>The Part instance</returns>
        [Obsolete("This method is deprecated, use ModInstance.Load instead")]
        public static Part LoadDummy(AssetBundle bundle, string prefabName, bool betterCopy = true)
        {
            // Safety checks
            if (!bundle)
                SplError("Tried to create a part without valid AssetBundle");

            if (String.IsNullOrWhiteSpace(prefabName))
                SplError("Tried to create a part without prefab name");

            if (Saver.modParts.ContainsKey(prefabName))
                SplError($"Tried to create an already existing prefab ({prefabName})");

            GameObject prefab = bundle.LoadAsset<GameObject>(prefabName);
            if (!prefab)
                SplError($"Tried to create a prefab but it was not found in the AssetBundle ({prefabName})");

            Part p = new Part(prefab, null, null, null, null);
            
            p.UseBetterCopy = betterCopy;
            GameObject.DontDestroyOnLoad(prefab); // We make sure that our prefab is not deleted in the first scene change

            if (prefab.GetComponent<PrefabGenerator>())
            {
                PrefabGenerator prefabGen = prefab.GetComponent<PrefabGenerator>();
                p.Name = prefabGen.PrefabName;
                Saver.modParts.Add(p.Name, prefab);

                PartManager.prefabGenParts.Add(p);
                DevLog($"Dummy part (Using prefab generator) added into list ({prefabName})");
            }
            else
            {
                p.Name = prefabName;
                Saver.modParts.Add(prefabName, prefab);

                PartManager.dummyParts.Add(p);

                DevLog($"Dummy part (Not using prefab generator) added into list ({prefabName})");
            }

            return p;
        }

        /// <summary>
        /// Allows to copy all the components from a car part of the game into a dummy part.
        /// </summary>
        /// <param name="p">The dummy part</param>
        /// <param name="partName">The name of the part that is going that provide the components to copy</param>
        /// <param name="ignoreBuiltin">Ignore Unity built-in components (Renderer, collider and MeshFilter) while doing the copy</param>
        /// <param name="doNotCopyChilds">Disables the recursive child copy</param>
        public static void CopyPartToPrefab(Part p, string partName, bool ignoreBuiltin = false, bool doNotCopyChilds = false)
        {
            if (p == null) // Safety check
            {
                Debug.LogError("[ModUtils/SPL/Error]: Tried to do full copy into empty part");
                return;
            }

            // We first delete all the components from our part.
            foreach (Component comp in p.Prefab.GetComponents<Component>())
            {
                if (comp is PrefabGenerator)
                    continue;

                if (!(comp is Transform) && !((comp is Renderer || comp is Collider || comp is MeshFilter) && ignoreBuiltin))
                {
                    GameObject.Destroy(comp);
                }
            }

            // Then we look up for the car part and store it
            GameObject carPart = Functions.GetCarPart(partName);

            if (!carPart)
            {
                Debug.LogError($"[ModUtils/SPL/Error] Car part was not found on CopyFullPartToPrefab! Part: {partName}");
                return;
            }

            p.Prefab.layer = carPart.layer; // Set the layer depending on the car part.

            // Now we copy all the components from the car part into the prefab
            foreach (Component comp in carPart.GetComponents<Component>())
            {
                if (!(comp is Transform) && !((comp is Renderer || comp is Collider || comp is MeshFilter) && ignoreBuiltin))
                {
                    if (comp is P3dPaintable || comp is P3dPaintableTexture || comp is P3dChangeCounter || comp is P3dMaterialCloner || comp is P3dColorCounter)
                        continue;

                    p.Prefab.AddComponent(comp.GetType()).GetCopyOf(comp, p.UseBetterCopy);
                    
                    DevLog(p, $"Now copying component to base object ({comp})");
                }
            }

            if (!doNotCopyChilds)
                AttachPrefabChilds(p.Prefab, carPart, p.UseBetterCopy); // Call the recursive function that copies all the child hierarchy.

            // Setting things up so the game knows what part is this (and also the Saver)
            p.CarProps = p.Prefab.GetComponent<CarProperties>();
            p.PartInfo = p.Prefab.GetComponent<Partinfo>();
            p.Renderer = p.Prefab.GetComponent<Renderer>();
            
            p.CarProps.PREFAB = p.Prefab;
            p.CarProps.PrefabName = p.Name;

            p.PartInfo.RenamedPrefab = String.IsNullOrEmpty(carPart.GetComponent<Partinfo>().RenamedPrefab) ? carPart.transform.name : carPart.GetComponent<Partinfo>().RenamedPrefab; // Fixes transparents breaking after reloading

            p.OriginalGameobject = carPart;

            Debug.LogError($"[ModUtils/SPL]: {p.Name} was succesfully loaded");
        }

        /// <summary>
        /// Forces the registering of a part into the internal mod list. Useful for parts that do not exist in survival.
        /// </summary>
        /// <param name="p"></param>
        /// <param name="addToCatalog"></param>
        public static void ForcePartRegister(Part p, bool addToCatalog = false)
        {
            Debug.LogError("[ModUtils/SPL]: Forcing register of part " + p.Name + " into the internal mod parts list.");

            PartManager.modLoadedParts.Add(p);
            GameObject.DontDestroyOnLoad(p.Prefab);
            PartManager.gameParts.Add(p.Prefab);

            if (addToCatalog) // Adding part into catalog
            {
                GameObject junkyardListParent = GameObject.Find("PartsParent");
                JunkPartsList jpl = junkyardListParent.GetComponent<JunkPartsList>();
                int sizeBeforeModify = jpl.Parts.Length;

                Array.Resize(ref jpl.Parts, sizeBeforeModify + 1);
                jpl.Parts[sizeBeforeModify] = p.Prefab;
            }

            // Localization
            if (p.languages["English"] == null)
                p.languages["English"] = p.CarProps.PartName;

            foreach (var dictionary in LocalizationManager.Dictionary)
            {
                if (dictionary.Value.ContainsKey(p.CarProps.PartName)) // Ignore case where the name is shared so the translation already exists
                    continue;

                if (p.languages[dictionary.Key] != null)
                    dictionary.Value.Add(p.CarProps.PartName, (string)p.languages[dictionary.Key]);
                else
                    dictionary.Value.Add(p.CarProps.PartName, (string)p.languages["English"]); // Fallback to english if no locale was set.
            }
        }

        /// <summary>
        /// Updates the DEPENDANTS and ATTACHABLES for the specified part
        /// </summary>
        /// <param name="part">The part to update</param>
        public static void UpdateTransparentsReference(Part part)
        {
            CarBuilding.UpdateTransparentsReferences(part.Prefab);
        }

        /// <summary>
        /// Recursive function that copies all the child hierarchy from a car part into a dummy part.
        /// </summary>
        /// <param name="partToAttach">The parent (top on hierarchy) GameObject that get the clones</param>
        /// <param name="original">The original part to be copied</param>
        /// <param name="preciseCloning">Precise cloning enabled</param>
        internal static void AttachPrefabChilds(GameObject partToAttach, GameObject original, bool preciseCloning)
        {
            DevLog("Attaching childs to " + partToAttach.name);

            // Now we also do the same for the childs of the object.
            for (int i = 0; i < original.transform.childCount; i++)
            {
                DevLog("Attaching " + original.transform.GetChild(i).name);
                GameObject childObject = new GameObject();
                childObject.transform.SetParent(partToAttach.transform);

                childObject.name = original.transform.GetChild(i).name;
                childObject.layer = original.transform.GetChild(i).gameObject.layer;
                childObject.tag = original.transform.GetChild(i).tag;

                childObject.transform.localPosition = original.transform.GetChild(i).localPosition;
                childObject.transform.localRotation = original.transform.GetChild(i).localRotation;
                childObject.transform.localScale = original.transform.GetChild(i).localScale;

                foreach (Component comp in original.transform.GetChild(i).GetComponents<Component>())
                {
                    if (comp is Transform || comp == null) // Note that this function does not take in account ignoreBuiltin from CopyPartToPrefab
                        continue;

                    if (!childObject.GetComponent(comp.GetType()))
                    {
                        childObject.AddComponent(comp.GetType()).GetCopyOf(comp, preciseCloning);

                        DevLog("Copying component " + comp.GetType());
                    }
                    else
                    {
                        Functions.CopyComponentData(childObject.GetComponent(comp.GetType()), original.transform.GetChild(i).GetComponent(comp.GetType()), preciseCloning);

                        DevLog("Cloning component" + comp.GetType());
                    }
                }

                if (original.transform.GetChild(i).childCount != 0)
                    AttachPrefabChilds(childObject, original.transform.GetChild(i).gameObject, preciseCloning);
            }
        }

        /// <summary>
        /// Invokes the FirstLoad event if any script is suscribed to it.
        /// </summary>
        internal static void InvokeFirstLoadEvent()
        {
            if (FirstLoad != null)
            {
                DevLog("First load was invoked - Developer logging is enabled (Please disable before releasing your mod!)");
                foreach(var handler in FirstLoad.GetInvocationList())
                {
                    try
                    {
                        handler.DynamicInvoke();
                    }
                    catch(Exception ex)
                    {
                        Debug.Log("[ModUtils/SPL/Error]: Exception caught while loading a mod, you should report this to the mod developer.");
                        Debug.Log($"[ModUtils/SPL/Error]: Exception details: {ex.ToString()} (ST: {ex.StackTrace})");
                        Debug.Log($"[ModUtils/SPL/Error]: Method: {handler.Method.Name}, type: {handler.Method.ReflectedType.Name}, assembly: {handler.Method.ReflectedType.Assembly.FullName}");
                    }
                }
            }
        }

        /// <summary>
        /// Invokes the LoadFinished event if any script is suscribed to it.
        /// </summary>
        internal static void InvokeLoadFinishedEvent()
        {
            if (LoadFinish != null)
            {
                DevLog("Load finish has been called - Developer logging is enabled (Please disable before releasing your mod!)");
                foreach (var handler in LoadFinish.GetInvocationList())
                {
                    try
                    {
                        handler.DynamicInvoke();
                    }
                    catch (Exception ex)
                    {
                        Debug.Log("[ModUtils/SPL/Error]: Exception caught while on load finish, you should report this to the mod developer.");
                        Debug.Log($"[ModUtils/SPL/Error]: Exception details: {ex.ToString()} (ST: {ex.StackTrace})");
                        Debug.Log($"[ModUtils/SPL/Error]: Method: {handler.Method.Name}, type: {handler.Method.ReflectedType.Name}, assembly: {handler.Method.ReflectedType.Assembly.FullName}");
                    }
                }
            }
        }

        /// <summary>
        /// Invokes the DataLoaded event if any script is suscribed to it.
        /// </summary>
        internal static void InvokeDataLoadedEvent()
        {
            if (DataLoaded != null)
            {
                DevLog("Data loaded has been called - Developer logging is enabled (Please disable before releasing your mod!)");
                foreach (var handler in DataLoaded.GetInvocationList())
                {
                    try
                    {
                        handler.DynamicInvoke();
                    }
                    catch (Exception ex)
                    {
                        Debug.Log("[ModUtils/SPL/Error]: Exception caught while on data loaded event, you should report this to the mod developer.");
                        Debug.Log($"[ModUtils/SPL/Error]: Exception details: {ex.ToString()} (ST: {ex.StackTrace})");
                        Debug.Log($"[ModUtils/SPL/Error]: Method: {handler.Method.Name}, type: {handler.Method.ReflectedType.Name}, assembly: {handler.Method.ReflectedType.Assembly.FullName}");

                    }
                }
            }
        }

        /// <summary>
        /// Prints a string on the log if the DEVELOPER_LOG is enabled.
        /// </summary>
        /// <param name="str">The string to be printed on log</param>
        internal static void DevLog(string str)
        {
            if (DEVELOPER_LOG)
                Debug.Log("[ModUtils/Dev/SPL]: " + str);
        }

        internal static void DevLog(Part p, string str)
        {
            if(p.Mod != null)
            {
                if(p.Mod.Settings.EnableDeveloperLog)
                {
                    Debug.Log($"[ModUtils/Dev/SPL]: {str} (part: part_{p.Prefab.name}, mod: {p.Mod.Name})");
                }
            }
        }

        /// <summary>
        /// Shows an error on log, then generates an exception
        /// </summary>
        /// <param name="str">The error to show</param>
        /// <exception cref="Exception">Generic exception to stop execution</exception>
        internal static void SplError(string str)
        {
            Debug.LogError("[ModUtils/SPL/Error]: " + str);
            throw new Exception("ModUtils exception");
        }
        
        // Compatibility for older versions
        [Obsolete("Use ModUtils.GetPlayer() instead!")]
        public static GameObject GetPlayer() { return ModUtils.GetPlayer(); }
        
        [Obsolete("Use ModUtils.GetPlayerTools() instead!")]
        public static tools GetPlayerTools() { return ModUtils.GetPlayerTools(); }
    }
}
