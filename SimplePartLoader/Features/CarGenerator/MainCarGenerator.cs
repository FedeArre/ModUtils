using PaintIn3D;
using SimplePartLoader.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SimplePartLoader.CarGen
{
    internal class MainCarGenerator
    {
        internal static List<Car> RegisteredCars = new List<Car>();
        internal static Hashtable AvailableBases = new Hashtable();
        
        internal static void BaseSetup()
        {
            AvailableBases[CarBase.Chad] = new Chad();
        }

        internal static void StartCarGen()
        {
            foreach (Car car in RegisteredCars)
            {
                ICarBase baseData = (ICarBase)AvailableBases[car.carGeneratorData.BaseCarToUse];
                
                // First, clone our base to our empty.
                CarBuilding.CopyCarToPrefab(baseData.GetCar(), car.emptyCarPrefab);
                CarBuilding.CopyCarToPrefab(baseData.GetCar(), car.carPrefab);

                // Then, destroy all transparents requested by user
                foreach (string transparent in car.carGeneratorData.TransparentsToDelete)
                {
                    CarGenUtils.DeleteRootTransparent(car.emptyCarPrefab, transparent);
                    CarGenUtils.DeleteRootTransparent(car.carPrefab, transparent);
                }

                // Add custom transparents to our car
                CarBuilding.AttachPrefabChilds(car.emptyCarPrefab, car.transparentsObject);
                CarBuilding.AttachPrefabChilds(car.carPrefab, car.transparentsObject);
                
                CarBuilding.UpdateTransparentsReferences(car.emptyCarPrefab);
                CarBuilding.UpdateTransparentsReferences(car.carPrefab);

                // We setup the car information already (Makes debugging easier)
                car.emptyCarPrefab.name = car.carGeneratorData.CarName;
                car.carPrefab.name = car.carGeneratorData.CarName;

                MainCarProperties mcpEmpty = car.emptyCarPrefab.GetComponent<MainCarProperties>();
                mcpEmpty.CarName = car.carGeneratorData.CarName;
                mcpEmpty.CarPrice = car.carGeneratorData.CarPrice;
                mcpEmpty.PREFAB = car.emptyCarPrefab;

                MainCarProperties mcp = car.carPrefab.GetComponent<MainCarProperties>();
                mcp.CarName = car.carGeneratorData.CarName;
                mcp.CarPrice = car.carGeneratorData.CarPrice;
                mcp.PREFAB = car.emptyCarPrefab;

                // Base setup
                baseData.SetupTemplate(car.emptyCarPrefab, car);
                baseData.SetupTemplate(car.carPrefab, car);
                car.OnSetupCarTemplate?.Invoke(car.emptyCarPrefab);
                car.OnSetupCarTemplate?.Invoke(car.carPrefab);

                if (!car.carGeneratorData.DisableModUtilsTemplateSetup)
                    baseData.ForceTemplateExceptions(car.exceptionsObject);

                // Now, we can build our car
                BuildCar(car);

                // Saving setup
                if (Saver.modParts.ContainsKey(car.carGeneratorData.CarName))
                {
                    Debug.LogError("[ModUtils/CarGen/Error]: Name collision detected! " + car.carGeneratorData.CarName);
                }

                Saver.modParts.Add(car.carGeneratorData.CarName, car.emptyCarPrefab);
                
                GameObject.DontDestroyOnLoad(car.emptyCarPrefab);
                GameObject.DontDestroyOnLoad(car.carPrefab);
            }
        }

        internal static void AddCars()
        {
            GameObject CarsParent = GameObject.Find("CarsParent");
            CarList CarsComp = CarsParent.GetComponent<CarList>();
            
            foreach (Car car in RegisteredCars)
            {
                Array.Resize(ref CarsComp.Cars, CarsComp.Cars.Length + 1);
                CarsComp.Cars[CarsComp.Cars.Length - 1] = car.carPrefab;
            }
        }

        internal static void BuildCar(Car car)
        {
            // Build our car
            CarGenUtils.RecursiveCarBuild(car);

            if (car.carGeneratorData.TransparentReferenceUpdate)
                CarBuilding.UpdateTransparentsReferences(car.carPrefab);

            if (car.carGeneratorData.BoneTargetTransformFix)
            {
                foreach (MyBoneSCR scr in car.carPrefab.GetComponentsInChildren<MyBoneSCR>())
                {
                    if (scr.thisTransform != null)
                    {
                        scr.thisTransform = scr.transform;
                    }
                }
            }

            // Post build
            ICarBase baseData = (ICarBase)AvailableBases[car.carGeneratorData.BaseCarToUse];

            // Force part name already so is easy to work on
            if(car.carGeneratorData.EnableAttachFix)
            {
                foreach (Partinfo partinfo in car.carPrefab.GetComponentsInChildren<Partinfo>())
                {
                    if (!String.IsNullOrEmpty(partinfo.RenamedPrefab))
                        partinfo.gameObject.name = partinfo.RenamedPrefab;
                }
            }

            baseData.PostBuild(car.carPrefab, car);
            car.OnPostBuild?.Invoke(car.carPrefab);

            // Attach fix
            if(car.carGeneratorData.EnableAttachFix)
            {
                MainCarProperties mcp = car.carPrefab.GetComponent<MainCarProperties>();
                foreach (Partinfo partinfo in car.carPrefab.GetComponentsInChildren<Partinfo>())
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

                foreach (CarProperties carProps in car.carPrefab.transform.GetComponentsInChildren<CarProperties>())
                {
                    carProps.MainProperties = mcp;
                }

                foreach (HexNut hexNut in car.carPrefab.GetComponentsInChildren<HexNut>())
                {
                    hexNut.tight = true;
                    hexNut.gameObject.transform.parent.GetComponent<Partinfo>().attachedbolts += 1f;
                    hexNut.gameObject.transform.parent.GetComponent<Partinfo>().tightnuts += 1f;
                }

                foreach (FlatNut flatNut in car.carPrefab.GetComponentsInChildren<FlatNut>())
                {
                    flatNut.tight = true;
                    
                    if (!flatNut.gameObject.transform.parent.GetComponent<Partinfo>())
                    {
                        Debug.LogError($"[ModUtils/CarGen/AttachFix/Error]: Flatnut error (Parent does not have Partinfo) detected in {flatNut.transform.parent.name}!");
                    }

                    flatNut.gameObject.transform.parent.GetComponent<Partinfo>().attachedbolts += 1f;
                    flatNut.gameObject.transform.parent.GetComponent<Partinfo>().tightnuts += 1f;
                }

                foreach (BoltNut boltNut in car.carPrefab.GetComponentsInChildren<BoltNut>())
                {
                    boltNut.ReStart();
                    boltNut.tight = true;

                    if (!boltNut.gameObject.transform.parent.GetComponent<Partinfo>())
                    {
                        Debug.LogError($"[ModUtils/CarGen/AttachFix/Error]: Boltnut error 1 (Parent does not have Partinfo) detected in {boltNut.transform.parent.name}!");
                        continue;
                    }
                    boltNut.gameObject.transform.parent.GetComponent<Partinfo>().ImportantBolts += 1f;
                    boltNut.gameObject.transform.parent.GetComponent<Partinfo>().fixedImportantBolts += 1f;
                    if (car.EnableDebug && !boltNut.otherobject)
                    {
                        Debug.LogError($"[ModUtils/CarGen/AttachFix/Error]: Boltnut error 2 (Missing otherobject) detected in {boltNut.transform.parent.name} - Otherobject should be {boltNut.otherobjectName}!");
                        continue;
                    }
                    
                    if (!boltNut.otherobject.GetComponent<Partinfo>())
                    {
                        Debug.LogError($"[ModUtils/CarGen/AttachFix/Error]: Boltnut error 3 (Otherobject missing partinfo) detected in {boltNut.transform.parent.name} - Otherobject is {boltNut.otherobjectName}!");
                        continue;
                    }
                    
                    boltNut.otherobject.GetComponent<Partinfo>().fixedImportantBolts += 1f;
                    boltNut.otherobject.GetComponent<Partinfo>().ImportantBolts += 1f;
                }

                foreach (WeldCut weldCut in car.carPrefab.GetComponentsInChildren<WeldCut>())
                {
                    weldCut.ReStart();
                    weldCut.welded = true;
                    Debug.Log(weldCut.gameObject.name);
                    Debug.Log(weldCut.transform.parent.name);
                    weldCut.gameObject.transform.parent.GetComponent<Partinfo>().fixedwelds += 1f;
                    weldCut.gameObject.transform.parent.GetComponent<Partinfo>().attachedwelds += 1f;

                    if (!weldCut.otherobject)
                    {
                        Debug.LogError($"[ModUtils/CarGen/AttachFix/Error]: Weldcut error 1 (Missing otherobject) detected in {weldCut.transform.parent.name} - Otherobject should be {weldCut.otherobjectName}!"); 
                        continue;
                    }
                    
                    if (!weldCut.otherobject.GetComponent<Partinfo>())
                    {
                        Debug.LogError($"[ModUtils/CarGen/AttachFix/Error]: Weldcut error 2 (Otherobject missing partinfo) detected in {weldCut.transform.parent.name} - Otherobject is {weldCut.otherobjectName}!");
                        continue;
                    }
                    
                    weldCut.otherobject.GetComponent<Partinfo>().fixedwelds += 1f;
                    weldCut.otherobject.GetComponent<Partinfo>().attachedwelds += 1f;
                }
            }

            if(car.carGeneratorData.EnableAutomaticPartCount)
            {
                int partCount = 0;
                CarProperties[] componentsInChildren = car.carPrefab.GetComponentsInChildren<CarProperties>();
                for (int i = 0; i < componentsInChildren.Length; i++)
                {
                    if (componentsInChildren[i].SinglePart)
                    {
                        partCount++;
                    }
                }
                
                car.carPrefab.GetComponent<MainCarProperties>().PartsCount = partCount;
                car.emptyCarPrefab.GetComponent<MainCarProperties>().PartsCount = partCount;
            }

            if (car.carGeneratorData.FixLights)
            {
                CommonFixes.CarLightsFix(car.carPrefab, car.EnableDebug);
                CommonFixes.Windows(car.carPrefab, car.EnableDebug);
            }

            if (car.carGeneratorData.EnableAutomaticPainting)
            {
                foreach(Partinfo pi in car.carPrefab.GetComponentsInChildren<Partinfo>())
                {
                    CarProperties cp = pi.GetComponent<CarProperties>();
                    if (!cp)
                        continue;
                    
                    if(cp.Paintable && cp.Washable && cp.MeshRepairable)
                    {
                        CarGenPainting.EnableFullSupport(cp.gameObject, PaintingSystem.PartPaintResolution.Low);
                    }
                    else if(cp.Paintable && !cp.Washable && cp.MeshRepairable)
                    {
                        CarGenPainting.EnablePaintOnly(cp.gameObject, PaintingSystem.PartPaintResolution.Low);
                    }
                    else if(cp.Paintable && cp.Washable && !cp.MeshRepairable)
                    {
                        CarGenPainting.EnablePaintAndDirt(cp.gameObject, PaintingSystem.PartPaintResolution.Low);
                    }
                    else if(!cp.Paintable && cp.Washable)
                    {
                        CarGenPainting.EnableDirtOnly(cp.gameObject, PaintingSystem.PartPaintResolution.Low);
                    }
                }
            }

            // Since painting generally crashes the creation, this will help developers
            if (car.EnableDebug)
            {
                foreach (CarProperties carProps in car.carPrefab.GetComponentsInChildren<CarProperties>())
                {
                    if (carProps.Paintable && !carProps.GetComponent<P3dPaintableTexture>())
                    {
                        Debug.LogWarning("[ModUtils/CarGen/PostBuild/Warning]: CarProperties.Paintable set to true but missing P3D support on " + carProps.name);
                    }
                }

                foreach(Partinfo pi in car.carPrefab.GetComponentsInChildren<Partinfo>())
                {
                    if (pi.fixedwelds != 0)
                        Debug.Log($"[ModUtils/CarGen/PostBuild/Debug]: {pi} ({pi.name}) has {pi.fixedwelds} WeldCuts");

                    if (pi.fixedImportantBolts != 0)
                        Debug.Log($"[ModUtils/CarGen/PostBuild/Debug]: {pi} ({pi.name}) has {pi.fixedImportantBolts} BoltNuts");
                }
            }
        }
    }
}
