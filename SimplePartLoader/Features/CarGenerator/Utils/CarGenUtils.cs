using SimplePartLoader.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static EVP.VehicleAudio;
using static PaintIn3D.P3dCoordCopier;

namespace SimplePartLoader.CarGen
{
    public class CarGenUtils
    {
        public static void DeleteRootTransparent(GameObject go, string transparentToDel, Car c)
        {
            Transform t = go.transform.Find(transparentToDel);
            if (!t)
            {
                c.ReportIssue("Invalid transparent delete - " + transparentToDel);
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

        internal static void RecursiveCarBuild(Car car, int iterationCount)
        {
            bool callAgain = false;
            if(iterationCount > 30)
            {
                CustomLogger.AddLine("CarGenerator", $"Recursive build on " + car.carGeneratorData.CarName + " car reached 30 iterations, aborting.");
                return;
            }

            foreach (transparents t in car.carPrefab.GetComponentsInChildren<transparents>())
            {
                if (!IsTransparentEmpty(t) || t.name == "Hook" || !t.GetComponent<MeshFilter>()) // Hook causes recursive loop, we can evade it - Mesh Filter check for some old stuff that isnt anymore around like ignition coil, is disabled just by that.
                    continue;

                bool isParentCustom = t.transform.parent.GetComponent<SPL_Part>();
                if (t.transform.parent.tag == "Vehicle" || t.transform.parent.parent.tag == "Vehicle") // transparent -> car (first check for common parts, second for engine tranny / susp)
                {
                    isParentCustom = true;
                }

                GameObject part = PartLookup(t.name, isParentCustom, car, t.Type);
                
                if(t.name == t.transform.parent.name)
                {
                    car.ReportIssue($"Car generation prevented infinite loop for {t.name}");
                    continue;
                }

                if (!part)
                {
                    if (CustomLogger.DebugEnabled)
                        CustomLogger.AddLine("CarDebug", "Part lookup could not find part for " + t.name);

                    continue;
                }

                SPL_Part splPart = part.GetComponent<SPL_Part>();
                if (splPart && splPart.Mod != null && splPart.Mod.Mod != null && splPart.Mod != car.loadedBy)
                {
                    if(!car.OtherModBuildingExceptions.Contains(splPart.Mod.Mod.ID))
                    {
                        car.ReportIssue($"Car generation prevented part fitting for {t.name} because part was from other mod ({splPart.Mod.Mod.ID})");
                        continue;
                    }
                }

                if(!string.IsNullOrEmpty(car.AutomaticFitToCar))
                {
                    Partinfo pi = part.GetComponent<Partinfo>();
                    bool flag = false;
                    foreach(string s in pi.FitsToCar)
                    {
                        if(s == car.AutomaticFitToCar)
                        {
                            flag = true;
                            break;
                        }
                    }

                    if(!flag && !car.FitToCarExceptions.Contains(pi.RenamedPrefab))
                    {
                        Array.Resize(ref pi.FitsToCar, pi.FitsToCar.Length + 1);
                        pi.FitsToCar[pi.FitsToCar.Length - 1] = car.AutomaticFitToCar;
                    }
                }

                CarBuilding.CopyPartIntoTransform(part, t.transform);
                callAgain = true;
            }

            if (callAgain)
            {
                RecursiveCarBuild(car, iterationCount+1);
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

        internal static GameObject PartLookup(string name, bool parentIsCustom, Car car, int type)
        {
            GameObject foundPart = null;

            BuildingExceptions exceptions = car.exceptionsObject;

            // Hardcoded exceptions
            if (name == "Spacer")
                return null;

            // Even faster lookup, priorize mod-loaded stuff first!
            foreach (Part partObj in car.loadedBy.Parts)
            {
                GameObject part = partObj.Prefab;

                if (part == null)
                    continue;

                if (part.name == name)
                {
                    foundPart = part;

                    if (exceptions.ExceptionList.ContainsKey(name))
                    {
                        CarProperties carProps = part.GetComponent<CarProperties>();
                        if (carProps.PrefabName != exceptions.ExceptionList[name])
                        {
                            foundPart = null;
                            continue;
                        }
                    }

                    if ((foundPart.GetComponent<SPL_Part>() && !parentIsCustom) && !exceptions.IgnoringStatusForPart(name))
                    {
                        foundPart = null;
                        continue;
                    }

                    if (foundPart.GetComponent<CarProperties>().Type != type && !exceptions.IgnoringStatusForPart(name))
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

            // Slow lookup by Partinfo RenamedPrefab. Only happens if part was not found yet (looking on mod parts only)
            foreach (Part partObj in car.loadedBy.Parts)
            {
                GameObject part = partObj.Prefab;
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
                            foundPart = null;
                            continue;
                        }
                    }

                    if (foundPart.GetComponent<SPL_Part>() && !parentIsCustom && !exceptions.IgnoringStatusForPart(name))
                    {
                        foundPart = null;
                        continue;
                    }

                    if (foundPart)
                        break;
                }
            }

            // Fast lookup, only by GameObject name (Works for almost all parts)
            foreach (GameObject part in PartManager.gameParts)
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
                    if ((foundPart.GetComponent<SPL_Part>() && !parentIsCustom) && !exceptions.IgnoringStatusForPart(name))
                    {
                        foundPart = null;
                        continue;
                    }
                    
                    if(foundPart.GetComponent<CarProperties>().Type != type && !exceptions.IgnoringStatusForPart(name))
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
                            foundPart = null;
                            continue;
                        }
                    }
                    
                    if (foundPart.GetComponent<SPL_Part>() && !parentIsCustom && !exceptions.IgnoringStatusForPart(name))
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
