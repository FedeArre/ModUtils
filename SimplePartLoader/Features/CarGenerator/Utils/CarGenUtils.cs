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
            InternalLog(() =>
                $"[PartLookup] START name='{name}', parentIsCustom={parentIsCustom}, type={type}, car={(car != null ? car.carGeneratorData.CarName : "NULL")}"
            );

            GameObject foundPart = null;

            if (car == null)
            {
                InternalLogError(() => "[PartLookup] car is NULL -> returning null");
                return null;
            }

            BuildingExceptions exceptions = car.exceptionsObject;

            if (exceptions == null)
            {
                InternalLogError(() => "[PartLookup] exceptionsObject is NULL -> returning null (would NRE later)");
                return null;
            }

            InternalLog(() =>
                $"[PartLookup] car.loadedBy={(car.loadedBy != null ? car.loadedBy.Name : "NULL")}"
            );

            if (car.loadedBy == null)
            {
                InternalLogError(() => "[PartLookup] car.loadedBy is NULL -> returning null");
                return null;
            }

            if (car.loadedBy.Parts == null)
            {
                InternalLogError(() => "[PartLookup] car.loadedBy.Parts is NULL -> returning null");
                return null;
            }

            InternalLog(() => $"[PartLookup] loadedBy.Parts count={car.loadedBy.Parts.Count}");

            // Even faster lookup, priorize mod-loaded stuff first!
            InternalLog(() => "[PartLookup] PASS 1: loadedBy.Parts by GameObject.name");
            foreach (Part partObj in car.loadedBy.Parts)
            {
                if (partObj == null)
                {
                    InternalLogWarning(() => "[PartLookup] PASS 1: partObj is NULL -> continue");
                    continue;
                }

                GameObject part = partObj.Prefab;

                if (part == null)
                {
                    InternalLogWarning(() => $"[PartLookup] PASS 1: '{partObj}' Prefab is NULL -> continue");
                    continue;
                }

                InternalLog(() => $"[PartLookup] PASS 1: checking part='{part.name}' vs name='{name}'");

                if (part.name == name)
                {
                    InternalLog(() => $"[PartLookup] PASS 1: NAME MATCH found candidate='{part.name}'");
                    foundPart = part;

                    bool hasExceptionKey = exceptions.ExceptionList != null
                        && exceptions.ExceptionList.ContainsKey(name);

                    InternalLog(() => $"[PartLookup] PASS 1: exceptions has key for '{name}'? {hasExceptionKey}");

                    if (hasExceptionKey)
                    {
                        string expectedPrefabName = exceptions.ExceptionList[name];
                        CarProperties carProps = part.GetComponent<CarProperties>();

                        InternalLog(() =>
                            $"[PartLookup] PASS 1: Exception expected PrefabName='{expectedPrefabName}', CarProperties={(carProps ? "OK" : "NULL")}"
                        );

                        if (carProps == null)
                        {
                            InternalLogWarning(() =>
                                "[PartLookup] PASS 1: CarProperties is NULL while exception exists -> rejecting candidate"
                            );
                            foundPart = null;
                            continue;
                        }

                        InternalLog(() =>
                            $"[PartLookup] PASS 1: Candidate CarProperties.PrefabName='{carProps.PrefabName}'"
                        );

                        if (carProps.PrefabName != expectedPrefabName)
                        {
                            InternalLog(() =>
                                $"[PartLookup] PASS 1: Reject: PrefabName mismatch ({carProps.PrefabName} != {expectedPrefabName})"
                            );
                            foundPart = null;
                            continue;
                        }
                    }

                    bool ignoringStatus = exceptions.IgnoringStatusForPart(name);
                    bool hasSpl = foundPart.GetComponent<SPL_Part>() != null;

                    InternalLog(() =>
                        $"[PartLookup] PASS 1: has SPL_Part? {hasSpl}, parentIsCustom={parentIsCustom}, ignoringStatus={ignoringStatus}"
                    );

                    if ((hasSpl && !parentIsCustom) && !ignoringStatus)
                    {
                        InternalLog(() => "[PartLookup] PASS 1: Reject: SPL_Part but parentIsCustom=false and not ignoringStatus");
                        foundPart = null;
                        continue;
                    }

                    CarProperties propsForType = foundPart.GetComponent<CarProperties>();
                    InternalLog(() =>
                        $"[PartLookup] PASS 1: CarProperties for type check={(propsForType ? "OK" : "NULL")}"
                    );

                    if (propsForType == null)
                    {
                        InternalLogWarning(() => "[PartLookup] PASS 1: Reject: missing CarProperties for type check");
                        foundPart = null;
                        continue;
                    }

                    InternalLog(() => $"[PartLookup] PASS 1: Candidate Type={propsForType.Type} expected type={type}");

                    if (propsForType.Type != type && !ignoringStatus)
                    {
                        InternalLog(() => "[PartLookup] PASS 1: Reject: Type mismatch and not ignoringStatus");
                        foundPart = null;
                        continue;
                    }

                    if (foundPart)
                    {
                        InternalLog(() => $"[PartLookup] PASS 1: ACCEPT candidate='{foundPart.name}' -> break");
                        break;
                    }
                }
            }

            if (foundPart)
            {
                InternalLog(() => $"[PartLookup] RETURN after PASS 1: '{foundPart.name}'");
                return foundPart;
            }

            // Slow lookup by Partinfo RenamedPrefab. Only happens if part was not found yet (looking on mod parts only)
            InternalLog(() => "[PartLookup] PASS 2: loadedBy.Parts by Partinfo.RenamedPrefab");
            foreach (Part partObj in car.loadedBy.Parts)
            {
                if (partObj == null)
                {
                    InternalLogWarning(() => "[PartLookup] PASS 2: partObj is NULL -> continue");
                    continue;
                }

                GameObject part = partObj.Prefab;
                if (part == null)
                {
                    InternalLogWarning(() => $"[PartLookup] PASS 2: '{partObj}' Prefab is NULL -> continue");
                    continue;
                }

                Partinfo pi = part.GetComponent<Partinfo>();
                InternalLog(() =>
                    $"[PartLookup] PASS 2: part='{part.name}', Partinfo={(pi ? "OK" : "NULL")}, RenamedPrefab='{(pi ? pi.RenamedPrefab : "NULL")}'"
                );

                if (pi != null && pi.RenamedPrefab == name)
                {
                    InternalLog(() => $"[PartLookup] PASS 2: RENAMED MATCH found candidate='{part.name}'");
                    foundPart = part;

                    bool hasExceptionKey = exceptions.ExceptionList != null
                        && exceptions.ExceptionList.ContainsKey(name);

                    InternalLog(() => $"[PartLookup] PASS 2: exceptions has key for '{name}'? {hasExceptionKey}");

                    if (hasExceptionKey)
                    {
                        string expected = exceptions.ExceptionList[name];
                        CarProperties carProps = part.GetComponent<CarProperties>();

                        InternalLog(() =>
                            $"[PartLookup] PASS 2: Exception expected='{expected}', CarProperties={(carProps ? "OK" : "NULL")}"
                        );

                        if (carProps == null)
                        {
                            InternalLogWarning(() =>
                                "[PartLookup] PASS 2: CarProperties is NULL while exception exists -> rejecting candidate"
                            );
                            foundPart = null;
                            continue;
                        }

                        InternalLog(() =>
                            $"[PartLookup] PASS 2: Candidate PrefabName='{carProps.PrefabName}', name='{carProps.name}'"
                        );

                        if (carProps.PrefabName != expected && carProps.name != expected)
                        {
                            InternalLog(() =>
                                $"[PartLookup] PASS 2: Reject: exception mismatch (PrefabName='{carProps.PrefabName}', name='{carProps.name}', expected='{expected}')"
                            );
                            foundPart = null;
                            continue;
                        }
                    }

                    bool ignoringStatus = exceptions.IgnoringStatusForPart(name);
                    bool hasSpl = foundPart.GetComponent<SPL_Part>() != null;

                    InternalLog(() =>
                        $"[PartLookup] PASS 2: has SPL_Part? {hasSpl}, parentIsCustom={parentIsCustom}, ignoringStatus={ignoringStatus}"
                    );

                    if (hasSpl && !parentIsCustom && !ignoringStatus)
                    {
                        InternalLog(() => "[PartLookup] PASS 2: Reject: SPL_Part but parentIsCustom=false and not ignoringStatus");
                        foundPart = null;
                        continue;
                    }

                    if (foundPart)
                    {
                        InternalLog(() => $"[PartLookup] PASS 2: ACCEPT candidate='{foundPart.name}' -> break");
                        break;
                    }
                }
            }

            if (foundPart)
            {
                InternalLog(() => $"[PartLookup] RETURN after PASS 2: '{foundPart.name}'");
                return foundPart;
            }

            // Fast lookup, only by GameObject name (Works for almost all parts)
            InternalLog(() =>
                $"[PartLookup] PASS 3: PartManager.gameParts by GameObject.name, gameParts={(PartManager.gameParts != null ? PartManager.gameParts.Count.ToString() : "NULL")}"
            );

            if (PartManager.gameParts == null)
            {
                InternalLogError(() => "[PartLookup] PartManager.gameParts is NULL -> returning null");
                return null;
            }

            foreach (GameObject part in PartManager.gameParts)
            {
                if (part == null)
                {
                    InternalLogWarning(() => "[PartLookup] PASS 3: part is NULL -> continue");
                    continue;
                }

                InternalLog(() => $"[PartLookup] PASS 3: checking part='{part.name}' vs name='{name}'");

                if (part.name == name)
                {
                    InternalLog(() => $"[PartLookup] PASS 3: NAME MATCH found candidate='{part.name}'");
                    foundPart = part;

                    bool hasExceptionKey = exceptions.ExceptionList != null
                        && exceptions.ExceptionList.ContainsKey(name);

                    InternalLog(() => $"[PartLookup] PASS 3: exceptions has key for '{name}'? {hasExceptionKey}");

                    if (hasExceptionKey)
                    {
                        string expected = exceptions.ExceptionList[name];
                        CarProperties carProps = part.GetComponent<CarProperties>();

                        InternalLog(() =>
                            $"[PartLookup] PASS 3: Exception expected='{expected}', CarProperties={(carProps ? "OK" : "NULL")}"
                        );

                        if (carProps == null)
                        {
                            InternalLogWarning(() =>
                                "[PartLookup] PASS 3: CarProperties is NULL while exception exists -> rejecting candidate"
                            );
                            foundPart = null;
                            continue;
                        }

                        InternalLog(() =>
                            $"[PartLookup] PASS 3: Candidate PrefabName='{carProps.PrefabName}', name='{carProps.name}'"
                        );

                        if (carProps.PrefabName != expected && carProps.name != expected)
                        {
                            InternalLog(() =>
                                $"[PartLookup] PASS 3: Reject: exception mismatch (PrefabName='{carProps.PrefabName}', name='{carProps.name}', expected='{expected}')"
                            );
                            foundPart = null;
                            continue;
                        }
                    }

                    bool ignoringStatus = exceptions.IgnoringStatusForPart(name);
                    bool hasSpl = foundPart.GetComponent<SPL_Part>() != null;

                    InternalLog(() =>
                        $"[PartLookup] PASS 3: has SPL_Part? {hasSpl}, parentIsCustom={parentIsCustom}, ignoringStatus={ignoringStatus}"
                    );

                    if ((hasSpl && !parentIsCustom) && !ignoringStatus)
                    {
                        InternalLog(() => "[PartLookup] PASS 3: Reject: SPL_Part but parentIsCustom=false and not ignoringStatus");
                        foundPart = null;
                        continue;
                    }

                    CarProperties propsForType = foundPart.GetComponent<CarProperties>();
                    InternalLog(() =>
                        $"[PartLookup] PASS 3: CarProperties for type check={(propsForType ? "OK" : "NULL")}"
                    );

                    if (propsForType == null)
                    {
                        InternalLogWarning(() => "[PartLookup] PASS 3: Reject: missing CarProperties for type check");
                        foundPart = null;
                        continue;
                    }

                    InternalLog(() => $"[PartLookup] PASS 3: Candidate Type={propsForType.Type} expected type={type}");

                    if (propsForType.Type != type && !ignoringStatus)
                    {
                        InternalLog(() => "[PartLookup] PASS 3: Reject: Type mismatch and not ignoringStatus");
                        foundPart = null;
                        continue;
                    }

                    if (foundPart)
                    {
                        InternalLog(() => $"[PartLookup] PASS 3: ACCEPT candidate='{foundPart.name}' -> break");
                        break;
                    }
                }
            }

            if (foundPart)
            {
                InternalLog(() => $"[PartLookup] RETURN after PASS 3: '{foundPart.name}'");
                return foundPart;
            }

            // Slow lookup by Partinfo RenamedPrefab. Only happens if part was not found yet.
            InternalLog(() => "[PartLookup] PASS 4: PartManager.gameParts by Partinfo.RenamedPrefab");
            foreach (GameObject part in PartManager.gameParts)
            {
                if (part == null)
                {
                    InternalLogWarning(() => "[PartLookup] PASS 4: part is NULL -> continue");
                    continue;
                }

                Partinfo pi = part.GetComponent<Partinfo>();
                InternalLog(() =>
                    $"[PartLookup] PASS 4: part='{part.name}', Partinfo={(pi ? "OK" : "NULL")}, RenamedPrefab='{(pi ? pi.RenamedPrefab : "NULL")}'"
                );

                if (pi != null && pi.RenamedPrefab == name)
                {
                    InternalLog(() => $"[PartLookup] PASS 4: RENAMED MATCH found candidate='{part.name}'");
                    foundPart = part;

                    bool hasExceptionKey = exceptions.ExceptionList != null
                        && exceptions.ExceptionList.ContainsKey(name);

                    InternalLog(() => $"[PartLookup] PASS 4: exceptions has key for '{name}'? {hasExceptionKey}");

                    if (hasExceptionKey)
                    {
                        string expected = exceptions.ExceptionList[name];
                        CarProperties carProps = part.GetComponent<CarProperties>();

                        InternalLog(() =>
                            $"[PartLookup] PASS 4: Exception expected='{expected}', CarProperties={(carProps ? "OK" : "NULL")}"
                        );

                        if (carProps == null)
                        {
                            InternalLogWarning(() =>
                                "[PartLookup] PASS 4: CarProperties is NULL while exception exists -> rejecting candidate"
                            );
                            foundPart = null;
                            continue;
                        }

                        InternalLog(() =>
                            $"[PartLookup] PASS 4: Candidate PrefabName='{carProps.PrefabName}', name='{carProps.name}'"
                        );

                        if (carProps.PrefabName != expected && carProps.name != expected)
                        {
                            InternalLog(() =>
                                $"[PartLookup] PASS 4: Reject: exception mismatch (PrefabName='{carProps.PrefabName}', name='{carProps.name}', expected='{expected}')"
                            );
                            foundPart = null;
                            continue;
                        }
                    }

                    bool ignoringStatus = exceptions.IgnoringStatusForPart(name);
                    bool hasSpl = foundPart.GetComponent<SPL_Part>() != null;

                    InternalLog(() =>
                        $"[PartLookup] PASS 4: has SPL_Part? {hasSpl}, parentIsCustom={parentIsCustom}, ignoringStatus={ignoringStatus}"
                    );

                    if (hasSpl && !parentIsCustom && !ignoringStatus)
                    {
                        InternalLog(() => "[PartLookup] PASS 4: Reject: SPL_Part but parentIsCustom=false and not ignoringStatus");
                        foundPart = null;
                        continue;
                    }

                    if (foundPart)
                    {
                        InternalLog(() => $"[PartLookup] PASS 4: ACCEPT candidate='{foundPart.name}' -> break");
                        break;
                    }
                }
            }

            InternalLog(() => $"[PartLookup] END returning {(foundPart ? $"'{foundPart.name}'" : "null")}");
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

        private static void InternalLog(Func<string> message)
        {
            if (!ModMain.DetailedCarGenLog.Checked) return;

            Debug.Log(message());
        }

        private static void InternalLogWarning(Func<string> message)
        {
            if (!ModMain.DetailedCarGenLog.Checked) return;

            Debug.LogWarning(message());
        }

        private static void InternalLogError(Func<string> message)
        {
            if (!ModMain.DetailedCarGenLog.Checked) return;

            Debug.LogError(message());
        }
    }
}
