using SimplePartLoader.CarGen;
using SimplePartLoader.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SimplePartLoader.Features.StartOptionBuilder
{
    internal class MainStartOptionBuilder
    {
        internal static List<StartOption> StartOptions = new List<StartOption>();

        internal static void StartBuilder()
        {
            foreach(var startOption in StartOptions)
            {
                // Lookup part reference of the original
                GameObject originalPart = PartLookup(startOption.PartToCopy, startOption);

                if(originalPart is null)
                {
                    CustomLogger.AddLine("StartOptionBuilder", $"Failed to find part {startOption.PartToCopy} on start option builder");
                    continue;
                }

                // Copy part into the prefab
                CarBuilding.CopyPartIntoTransform(originalPart, startOption.Prefab.transform);

                // Now, recursively copy all children up to 10 levels deep. 
                RecursiveBuild(startOption, 0);

                // Apply fixes
                if(startOption.Settings.ApplyTransparentReferenceFix)
                    CarBuilding.UpdateTransparentsReferences(startOption.Prefab, null); // Is fine - Car is just used to report issues, and it checks if is null before reporting them
                
                if(startOption.Settings.ApplyVisualObjectFix)
                    CarBuilding.UpdateVisualObjects(startOption.Prefab);

                if(startOption.Settings.ApplyRenamedPrefabNameCorrectionFix)
                {
                    foreach (Partinfo partinfo in startOption.Prefab.GetComponentsInChildren<Partinfo>())
                    {
                        partinfo.HingePivot = null;

                        if (!String.IsNullOrEmpty(partinfo.RenamedPrefab))
                            partinfo.gameObject.name = partinfo.RenamedPrefab;
                    }
                }
                

                if(startOption.Settings.ApplyPaintingFixes)
                {
                    foreach (Partinfo pi in startOption.Prefab.GetComponentsInChildren<Partinfo>())
                    {
                        CarProperties cp = pi.GetComponent<CarProperties>();
                        if (!cp)
                            continue;

                        if (cp.Paintable && cp.Washable && cp.MeshRepairable)
                        {
                            CarGenPainting.EnableFullSupport(cp.gameObject, PaintingSystem.PartPaintResolution.Low);
                        }
                        else if (cp.Paintable && !cp.Washable && cp.MeshRepairable)
                        {
                            CarGenPainting.EnablePaintOnly(cp.gameObject, PaintingSystem.PartPaintResolution.Low);
                        }
                        else if (cp.Paintable && cp.Washable && !cp.MeshRepairable)
                        {
                            CarGenPainting.EnablePaintAndDirt(cp.gameObject, PaintingSystem.PartPaintResolution.Low);
                        }
                        else if (!cp.Paintable && cp.Washable)
                        {
                            CarGenPainting.EnableDirtOnly(cp.gameObject, PaintingSystem.PartPaintResolution.Low);
                        }
                    }
                }

                if(startOption.Settings.ApplyAttachFixes)
                {
                    foreach (Partinfo partinfo in startOption.Prefab.GetComponentsInChildren<Partinfo>())
                    {
                        partinfo.fixedImportantBolts = 0f;
                        partinfo.fixedwelds = 0f;
                        partinfo.attachedwelds = 0f;
                        partinfo.ImportantBolts = 0f;
                        partinfo.tightnuts = 0f;
                        partinfo.attachedbolts = 0f;

                        if (!String.IsNullOrEmpty(partinfo.RenamedPrefab))
                            partinfo.gameObject.name = partinfo.RenamedPrefab;
                    }

                    foreach (HexNut hexNut in startOption.Prefab.GetComponentsInChildren<HexNut>())
                    {
                        hexNut.tight = true;
                        hexNut.gameObject.transform.parent.GetComponent<Partinfo>().attachedbolts += 1f;
                        hexNut.gameObject.transform.parent.GetComponent<Partinfo>().tightnuts += 1f;
                    }

                    foreach (FlatNut flatNut in startOption.Prefab.GetComponentsInChildren<FlatNut>())
                    {
                        flatNut.tight = true;

                        if (!flatNut.gameObject.transform.parent.GetComponent<Partinfo>())
                        {
                            //car.ReportIssue($"Flatnut error (Parent does not have Partinfo) detected in {flatNut.transform.parent.name}!");
                            continue;
                        }

                        flatNut.gameObject.transform.parent.GetComponent<Partinfo>().attachedbolts += 1f;
                        flatNut.gameObject.transform.parent.GetComponent<Partinfo>().tightnuts += 1f;
                    }

                    foreach (BoltNut boltNut in startOption.Prefab.GetComponentsInChildren<BoltNut>())
                    {
                        boltNut.ReStart();
                        boltNut.tight = true;

                        if (!boltNut.gameObject.transform.parent.GetComponent<Partinfo>())
                        {
                            //car.ReportIssue($"Boltnut error 1 (Parent does not have Partinfo) detected in {boltNut.transform.parent.name}!");
                            continue;
                        }

                        boltNut.gameObject.transform.parent.GetComponent<Partinfo>().ImportantBolts += 1f;
                        boltNut.gameObject.transform.parent.GetComponent<Partinfo>().fixedImportantBolts += 1f;

                        if (!boltNut.otherobject)
                        {
                            if (string.IsNullOrEmpty(boltNut.otherobjectNameL) || string.IsNullOrEmpty(boltNut.otherobjectNameR))
                            {
                                boltNut.otherobjectNameL = boltNut.otherobjectName;
                                boltNut.otherobjectNameR = boltNut.otherobjectName;
                                boltNut.ReStart();

                                if (!boltNut.otherobject)
                                {
                                    //car.ReportIssue($"Boltnut error 2 (Missing otherobject) detected in {boltNut.transform.parent.name} - Otherobject should be {boltNut.otherobjectName}!");
                                    continue;
                                }
                            }
                            else
                            {
                                //car.ReportIssue($"Boltnut error 2 (Missing otherobject) detected in {boltNut.transform.parent.name} - Otherobject should be {boltNut.otherobjectName}!");
                                continue;
                            }
                        }

                        if (!boltNut.otherobject.GetComponent<Partinfo>())
                        {
                            //.ReportIssue($"Boltnut error 3 (Otherobject missing partinfo) detected in {boltNut.transform.parent.name} - Otherobject is {boltNut.otherobjectName}!");
                            continue;
                        }

                        boltNut.otherobject.GetComponent<Partinfo>().fixedImportantBolts += 1f;
                        boltNut.otherobject.GetComponent<Partinfo>().ImportantBolts += 1f;
                    }

                    foreach (WeldCut weldCut in startOption.Prefab.GetComponentsInChildren<WeldCut>())
                    {
                        weldCut.ReStart();
                        weldCut.welded = true;

                        if (CustomLogger.DebugEnabled)
                        {
                            Debug.Log($"[ModUtils/StartOptionBuilder/AttachDebug]: Weld object ${weldCut.gameObject?.name}, parent is {weldCut.transform.parent.name}");
                        }

                        weldCut.gameObject.transform.parent.GetComponent<Partinfo>().fixedwelds += 1f;
                        weldCut.gameObject.transform.parent.GetComponent<Partinfo>().attachedwelds += 1f;

                        if (!weldCut.otherobject)
                        {
                            //.ReportIssue($"Weldcut error 1 (Missing otherobject) detected in {weldCut.transform.parent.name} - Otherobject should be {weldCut.otherobjectName}!");
                            continue;
                        }

                        if (!weldCut.otherobject.GetComponent<Partinfo>())
                        {
                            //.ReportIssue($"Weldcut error 2 (Otherobject missing partinfo) detected in {weldCut.transform.parent.name} - Otherobject is {weldCut.otherobjectName}!");
                            continue;
                        }

                        weldCut.otherobject.GetComponent<Partinfo>().fixedwelds += 1f;
                        weldCut.otherobject.GetComponent<Partinfo>().attachedwelds += 1f;
                    }
                }

                GameObject.DontDestroyOnLoad(startOption.Prefab);

                try
                {
                    startOption.PostBuildFunction?.Invoke();
                }
                catch(Exception ex)
                {
                    CustomLogger.AddLine("StartOptionBuilder", $"An unhandled error occured on the PostBuild function of the StartOption {startOption.PartToCopy} (mod: {startOption.LoadedBy.Name}). Details:");
                    CustomLogger.AddLine("StartOptionBuilder", ex);
                }
            }
        }

        internal static GameObject PartLookup(string name, StartOption startOption)
        {
            GameObject foundPart = null;

            // Even faster lookup, priorize mod-loaded stuff first!
            foreach (Part partObj in startOption.LoadedBy.Parts)
            {
                GameObject part = partObj.Prefab;

                if (part == null)
                    continue;

                if (part.name == name)
                {
                    foundPart = part;

                    if (startOption.Exceptions.ContainsKey(name))
                    {
                        CarProperties carProps = part.GetComponent<CarProperties>();
                        if (carProps.PrefabName != startOption.Exceptions[name])
                        {
                            foundPart = null;
                            continue;
                        }
                    }

                    if (foundPart)
                        break;

                }
            }

            if (foundPart)
                return foundPart;

            // Slow lookup by Partinfo RenamedPrefab. Only happens if part was not found yet (looking on mod parts only)
            foreach (Part partObj in startOption.LoadedBy.Parts)
            {
                GameObject part = partObj.Prefab;
                if (part == null)
                    continue;

                Partinfo pi = part.GetComponent<Partinfo>();
                if (pi.RenamedPrefab == name)
                {
                    foundPart = part;

                    if (startOption.Exceptions.ContainsKey(name))
                    {
                        CarProperties carProps = part.GetComponent<CarProperties>();
                        if (carProps.PrefabName != startOption.Exceptions[name])
                        {
                            foundPart = null;
                            continue;
                        }
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

                    if (startOption.Exceptions.ContainsKey(name))
                    {
                        CarProperties carProps = part.GetComponent<CarProperties>();
                        if (carProps.PrefabName != startOption.Exceptions[name])
                        {
                            foundPart = null;
                            continue;
                        }
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

                    if (startOption.Exceptions.ContainsKey(name))
                    {
                        CarProperties carProps = part.GetComponent<CarProperties>();
                        if (carProps.PrefabName != startOption.Exceptions[name])
                        {
                            foundPart = null;
                            continue;
                        }
                    }

                    if (foundPart)
                        break;
                }
            }

            return foundPart;
        }

        internal static void RecursiveBuild(StartOption startOption, int iterationCount)
        {
            bool callAgain = false;
            if (iterationCount > 10)
            {
                CustomLogger.AddLine("StartOptionBuilder", $"Recursive build of start option on " + startOption.PartToCopy + " reached 10 iterations, aborting.");
                return;
            }

            foreach (transparents t in startOption.Prefab.GetComponentsInChildren<transparents>())
            {
                if (!IsTransparentEmpty(t) || t.name == "Hook" || !t.GetComponent<MeshFilter>()) // Hook causes recursive loop, we can evade it - Mesh Filter check for some old stuff that isnt anymore around like ignition coil, is disabled just by that.
                    continue;

                GameObject part = PartLookup(t.name, startOption);

                if (t.name == t.transform.parent.name)
                {
                    // TODO: Issues!
                    //startOption.ReportIssue($"Car generation prevented infinite loop for {t.name}");
                    continue;
                }

                if (!part)
                {
                    if (CustomLogger.DebugEnabled)
                        CustomLogger.AddLine("StartOptionBuilder", "Part lookup could not find part for " + t.name);

                    continue;
                }

                SPL_Part splPart = part.GetComponent<SPL_Part>();
                if (splPart && splPart.Mod != null && splPart.Mod.Mod != null && splPart.Mod != startOption.LoadedBy)
                {
                    // TODO: Issues!
                    //startOption.ReportIssue($"Car generation prevented part fitting for {t.name} because part was from other mod ({splPart.Mod.Mod.ID})");
                    continue;
                }

                CarBuilding.CopyPartIntoTransform(part, t.transform);
                callAgain = true;
            }

            if (callAgain)
            {
                RecursiveBuild(startOption, iterationCount + 1);
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

    }
}
