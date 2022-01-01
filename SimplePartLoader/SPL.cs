using System;
using UnityEngine;
using SimplePartLoader.Utils;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

namespace SimplePartLoader
{
    public class SPL
    {
        public delegate void FirstLoadDelegate();
        public static event FirstLoadDelegate FirstLoad;

        public static bool DEVELOPER_LOG = false;

        /// <summary>
        /// Adds a prefab as a car part into the game
        /// </summary>
        /// <param name="bundle">The bundle in which the prefab is located. Has to be loaded!</param>
        /// <param name="prefabName">The name of the prefab to be loaded</param>
        /// <exception cref="Exception">An exception will be thrown if the bundle or prefabName are invalid, if the prefab already exists or if essential components are missing</exception>
        /// <returns></returns>
        public static Part LoadPart(AssetBundle bundle, string prefabName)
        {
            // Safety checks
            if (!bundle)
                throw new Exception("Tried to create a part without valid AssetBundle");

            if (String.IsNullOrWhiteSpace(prefabName))
                throw new Exception("Tried to create a part without prefab name");

            if (Saver.modParts.ContainsKey(prefabName))
                throw new Exception($"Tried to create an already existing prefab ({prefabName})");

            GameObject prefab = bundle.LoadAsset<GameObject>(prefabName);
            if (!prefab)
                throw new Exception($"Tried to create a prefab but it was not found in the AssetBundle ({prefabName})");

            CarProperties prefabCarProp = prefab.GetComponent<CarProperties>();
            Partinfo prefabPartInfo = prefab.GetComponent<Partinfo>();

            if (!prefabCarProp || !prefabPartInfo)
                throw new Exception("An essential component is missing!");

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
            Transform[] childs = prefab.GetComponentsInChildren<Transform>();
            for(int i = 0; i < childs.Length; i++)
            {
                HexNut hx = childs[i].GetComponent<HexNut>();

                if (hx || childs[i].GetComponent<FlatNut>())
                {
                    childs[i].gameObject.AddComponent<CarProperties>();
                    childs[i].gameObject.AddComponent<DISABLER>();

                    childs[i].gameObject.layer = LayerMask.NameToLayer(hx ? "Bolts" : "FlatBolts"); // Add bolts if they have HexNut component or FlatBolts if has FlatNut component.
                    if (!childs[i].GetComponent<BoxCollider>())
                        childs[i].gameObject.AddComponent<BoxCollider>();
                }
            }

            Part p = new Part(prefab, prefabCarProp, prefabPartInfo);
            PartManager.modLoadedParts.Add(p);

            Saver.modParts.Add(p.CarProps.PrefabName, prefab);

            GameObject.DontDestroyOnLoad(prefab); // We make sure that our prefab is not deleted in the first scene change

            return p; // We provide the Part instance so the developer can setup the transparents
        }

        /// <summary>
        /// Allows to load a dummy part into the memory for getitng his properties later.
        /// </summary>
        /// <param name="bundle">The bundle in which the prefab is located. Has to be loaded!</param>
        /// <param name="prefabName">The name of the prefab to be loaded</param>
        /// <exception cref="Exception">An exception will be thrown if the bundle or prefabName are invalid or if the prefab already exists</exception>
        /// <returns></returns>
        public static Part LoadDummy(AssetBundle bundle, string prefabName)
        {
            // Safety checks
            if (!bundle)
                throw new Exception("Tried to create a part without valid AssetBundle");

            if (String.IsNullOrWhiteSpace(prefabName))
                throw new Exception("Tried to create a part without prefab name");

            if (Saver.modParts.ContainsKey(prefabName))
                throw new Exception($"Tried to create an already existing prefab ({prefabName})");

            GameObject prefab = bundle.LoadAsset<GameObject>(prefabName);
            if (!prefab)
                throw new Exception($"Tried to create a prefab but it was not found in the AssetBundle ({prefabName})");

            Part p = new Part(prefab, null, null);
            p.Name = prefabName;
            PartManager.dummyParts.Add(p);

            Saver.modParts.Add(prefabName, prefab);

            GameObject.DontDestroyOnLoad(prefab); // We make sure that our prefab is not deleted in the first scene change

            return p;
        }

        /// <summary>
        /// Allows to copy all the components (including Unity built-in components) from a car part of the game into a dummy part.
        /// </summary>
        /// <param name="p">The dummy part</param>
        /// <param name="partName">The name of the part that is going that provide the components to copy</param>
        /// <param name="carName">The car of which the part is from (LAD, LADCoupe or Chad)</param>
        public static void CopyFullPartToPrefab(Part p, string partName)
        {
            if(p == null)
            {
                Debug.LogError("[SPL]: Tried to do full copy into empty part");
                return;
            }

            // We first delete all the components from our part.
            foreach (Component comp in p.Prefab.GetComponents<Component>())
            {
                if (!(comp is Transform))
                {
                    GameObject.Destroy(comp);
                }
            }

            // Then we look up for the car part and store it
            GameObject carPart = GetCarPart(partName);

            if (!carPart)
            {
                Debug.LogError($"[SPL] Car part was not found on CopyFullPartToPrefab! {partName}");
                return;
            }

            p.Prefab.layer = carPart.layer;

            // Now we copy all the components from the car part into the prefab
            foreach (Component comp in carPart.GetComponents<Component>())
            {
                if (!(comp is Transform))
                {
                    p.Prefab.AddComponent(comp.GetType()).GetCopyOf(comp);
                    Debug.LogError("Now copying base component " + comp);
                }
            }

            Debug.LogError("printing all parts before doing it - PART " + carPart.name);
            Transform[] t = carPart.GetComponentsInChildren<Transform>();
            foreach(Transform t2 in t)
            {
                Debug.LogError($"{t2.name} - parent is {t2.parent}");
            }
            AttachPrefabChilds(p.Prefab, carPart);

            p.CarProps = p.Prefab.GetComponent<CarProperties>();
            p.PartInfo = p.Prefab.GetComponent<Partinfo>();

            p.CarProps.PREFAB = p.Prefab;
            p.CarProps.PrefabName = p.Name;

            Debug.LogError("Finished " + p.Name);
        }

        internal static void AttachPrefabChilds(GameObject partToAttach, GameObject original)
        {
            Debug.LogError("Now attaching to " + partToAttach.name);

            // Now we also do the same for the childs of the object.
            for (int i = 0; i < original.transform.childCount; i++)
            {
                Debug.LogError("Attaching " + original.transform.GetChild(i).name);
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
                    if(comp is FLUID)
                    {
                        FieldInfo[] sourceFields = original.transform.GetChild(i).GetComponent<FLUID>().GetType().GetFields(BindingFlags.Public |
                        BindingFlags.NonPublic |
                        BindingFlags.Instance);

                        foreach (FieldInfo field in sourceFields)
                        {
                            Debug.LogError($"{field.Name} - {field.FieldType} - {field.GetValue(original.transform.GetChild(i).GetComponent<FLUID>())}");
                        }
                    }
                    if (comp is Transform || comp == null)
                        continue;

                    if(!childObject.GetComponent(comp.GetType()))
                    {
                        childObject.AddComponent(comp.GetType()).GetCopyOf(comp);

                        Debug.LogError("copying comp " + comp.GetType());
                    } 
                    else
                    {
                        CopyComponentData(childObject.GetComponent(comp.GetType()), original.transform.GetChild(i).GetComponent(comp.GetType()));

                        Debug.LogError("cloning comp " + comp.GetType());
                    }
                }

                Debug.LogError($"COUNT: {original.transform.GetChild(i).childCount}");
                if (original.transform.GetChild(i).childCount != 0)
                    AttachPrefabChilds(childObject, original.transform.GetChild(i).gameObject);
            }

        }

        public static void CopyComponentData(Component comp, Component other)
        {
            Type type = comp.GetType();

            List<Type> derivedTypes = new List<Type>();
            Type derived = type.BaseType;
            while (derived != null)
            {
                if (derived == typeof(MonoBehaviour))
                {
                    break;
                }
                derivedTypes.Add(derived);
                derived = derived.BaseType;
            }

            IEnumerable<PropertyInfo> pinfos = type.GetProperties(Extension.bindingFlags);

            foreach (Type derivedType in derivedTypes)
            {
                pinfos = pinfos.Concat(derivedType.GetProperties(Extension.bindingFlags));
            }

            pinfos = from property in pinfos
                     where !(type == typeof(Rigidbody) && property.Name == "inertiaTensor") // Special case for Rigidbodies inertiaTensor which isn't catched for some reason.
                     where !property.CustomAttributes.Any(attribute => attribute.AttributeType == typeof(ObsoleteAttribute))
                     select property;
            foreach (var pinfo in pinfos)
            {
                if (pinfo.CanWrite)
                {
                    if (pinfos.Any(e => e.Name == $"shared{char.ToUpper(pinfo.Name[0])}{pinfo.Name.Substring(1)}"))
                    {
                        continue;
                    }
                    try
                    {
                        pinfo.SetValue(comp, pinfo.GetValue(other, null), null);
                    }
                    catch { } // In case of NotImplementedException being thrown. For some reason specifying that exception didn't seem to catch it, so I didn't catch anything specific.
                }
            }

            IEnumerable<FieldInfo> finfos = type.GetFields(Extension.bindingFlags);

            foreach (var finfo in finfos)
            {

                foreach (Type derivedType in derivedTypes)
                {
                    if (finfos.Any(e => e.Name == $"shared{char.ToUpper(finfo.Name[0])}{finfo.Name.Substring(1)}"))
                    {
                        continue;
                    }
                    finfos = finfos.Concat(derivedType.GetFields(Extension.bindingFlags));
                }
            }

            foreach (var finfo in finfos)
            {
                finfo.SetValue(comp, finfo.GetValue(other));
            }

            finfos = from field in finfos
                     where field.CustomAttributes.Any(attribute => attribute.AttributeType == typeof(ObsoleteAttribute))
                     select field;
            foreach (var finfo in finfos)
            {
                finfo.SetValue(comp, finfo.GetValue(other));
            }
        }

        /// <summary>
        /// Internal usage only, gets a car part from his name and car
        /// </summary>
        /// <param name="partName">The name of the part</param>
        /// <param name="carName">The car of the part</param>
        /// <returns>The prefab of the part if exists, null otherwise</returns>
        internal static GameObject GetCarPart(string partName)
        {
            GameObject carPart = null, PartsParent = GameObject.Find("PartsParent");
            foreach (GameObject part in PartsParent.GetComponent<JunkPartsList>().Parts)
            {
                if (part.name == partName)
                {
                    if (!part.GetComponent<transparents>())
                    {
                        carPart = part;
                        break;
                    }
                }
            }

            return carPart;
        }

        /// <summary>
        /// Invokes the FirstLoad event if any script is suscribed to it.
        /// </summary>
        internal static void InvokeFirstLoadEvent()
        {
            if(FirstLoad != null)
            {
                FirstLoad?.Invoke();
            }
        }

        internal static void DevLog(string str)
        {
            if (DEVELOPER_LOG)
                Debug.Log("[SPL]: " + str);
        }
    }
}
