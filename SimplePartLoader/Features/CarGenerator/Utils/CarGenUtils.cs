using SimplePartLoader.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SimplePartLoader.CarGen
{
    public class CarGenUtils
    {
        public static void DeleteRootTransparent(GameObject go, string transparentToDel)
        {
            Transform t = go.transform.Find(transparentToDel);
            if (!t)
            {
                Debug.LogError("[ModUtils/CarGenUtils/Error]: Invalid transparent delete - " + transparentToDel);
            }
            else
            {
                GameObject.DestroyImmediate(t.gameObject);
            }
        }

        public static Transform LookupValidTransform(GameObject root, string name, bool onlyRootPart = false)
        {
            foreach(Transform t in root.transform.GetComponentsInChildren<Transform>())
            {
                if(t.name == name)
                {
                    if(!t.GetComponent<transparents>())
                    {
                        if (onlyRootPart && !t.GetComponent<Partinfo>())
                            continue;
                        
                        return t;
                    }
                }
            }
            
            return null;
        }

        internal static void RecursiveCarBuild(Car car)
        {
            bool callAgain = false;

            foreach (transparents t in car.carPrefab.GetComponentsInChildren<transparents>())
            {
                if (!IsTransparentEmpty(t))
                    continue;

                bool isParentCustom = t.transform.parent.GetComponent<SPL_Part>();
                if (t.transform.parent.tag == "Vehicle" || t.transform.parent.parent.tag == "Vehicle") // transparent -> car (first check for common parts, second for engine tranny / susp)
                {
                    isParentCustom = true;
                }

                GameObject part = PartLookup(t.name, isParentCustom, car.exceptionsObject, t.Type);

                if (!part)
                {
                    if (car.EnableDebug)
                        Debug.LogWarning("[ModUtils/CarGen/PartLookup/Warning]: Part lookup could not find part for " + t.name);
                    
                    continue;
                }

                CarBuilding.CopyPartIntoTransform(part, t.transform);
                callAgain = true;
            }

            if (callAgain)
            {
                RecursiveCarBuild(car);
            }
        }
        
        internal static bool IsTransparentEmpty(transparents t)
        {
            foreach (Transform t2 in t.GetComponentsInChildren<Transform>())
            {
                if (t2 == t.transform)
                    continue;

                if (t2.GetComponent<CarProperties>() && !t2.name.ToLower().Contains("pivot") && !t2.name.ToLower().Contains("wheelcont"))
                    return false;
            }

            return true;
        }

        internal static GameObject PartLookup(string name, bool parentIsCustom, BuildingExceptions exceptions, int type)
        {
            GameObject foundPart = null;

            // Hardcoded exceptions
            if (name == "Spacer")
                return null;
            
            // Fast lookup, only by GameObject name (Works for almost all parts)
            foreach(GameObject part in PartManager.gameParts)
            {
                if (part == null)
                    continue;

                if (part.name == name)
                {
                    foundPart = part;
                    
                    if(exceptions.ExceptionList.ContainsKey(name))
                    {
                        CarProperties carProps = part.GetComponent<CarProperties>();
                        if(carProps.PrefabName != exceptions.ExceptionList[name])
                        {
                            foundPart = null;
                            continue;
                        }
                    }

                    // TODO: Consider if this two are a good idea
                    if (foundPart.GetComponent<SPL_Part>() && !parentIsCustom)
                    {
                        foundPart = null;
                        continue;
                    }
                    
                    if(foundPart.GetComponent<CarProperties>().Type != type)
                    {
                        foundPart = null;
                        continue;
                    }
                    
                    if (foundPart)
                        break;

                }
            }

            if (foundPart)
                return foundPart;
            
            // Slow lookup by Partinfo RenamedPrefab. Only happens if part was not found yet.
            foreach (GameObject part in PartManager.gameParts)
            {
                if (part == null)
                    continue;

                Partinfo pi = part.GetComponent<Partinfo>();
                if (pi.RenamedPrefab == name)
                {
                    foundPart = part;

                    if (exceptions.ExceptionList.ContainsKey(name))
                    {
                        CarProperties carProps = part.GetComponent<CarProperties>();
                        if (carProps.PrefabName != exceptions.ExceptionList[name])
                        {
                            Debug.Log($"EXCEPTION HIT: {carProps.PrefabName} - {exceptions.ExceptionList[name]}");

                            foundPart = null;
                            continue;
                        }
                    }
                    
                    // TODO: Consider if this two are a good idea
                    if (foundPart.GetComponent<SPL_Part>() && !parentIsCustom)
                    {
                        foundPart = null;
                        continue;
                    }

                    if (foundPart)
                        break;
                }
            }

            return foundPart;
        }

        public static void SetPrivatePropertyValue<T>(T obj, string propertyName, object newValue)
        {
            if (String.IsNullOrEmpty(propertyName) || obj == null || newValue == null)
                return;
            
            foreach (FieldInfo fi in obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
            {
                if (fi.Name.ToLower().Contains(propertyName.ToLower()))
                {
                    fi.SetValue(obj, newValue);
                    break;
                }
            }
        }
    }
}
