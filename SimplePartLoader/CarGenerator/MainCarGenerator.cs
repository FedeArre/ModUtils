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
            GameObject CarsParent = GameObject.Find("CarsParent");
            CarList CarsComp = CarsParent.GetComponent<CarList>();
            
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
                }

                // Add custom transparents to our car
                CarBuilding.AttachPrefabChilds(car.emptyCarPrefab, car.transparentsObject);
                CarBuilding.UpdateTransparentsReferences(car.emptyCarPrefab);

                // Base setup
                baseData.SetupTemplate(car.emptyCarPrefab, car);
                baseData.SetupTemplate(car.carPrefab, car);
                car.OnSetupCarTemplate?.Invoke(car.emptyCarPrefab);
                car.OnSetupCarTemplate?.Invoke(car.carPrefab);

                // Now, we can build our car
                BuildCar(car);

                // Last, inject our car into the game
                Array.Resize(ref CarsComp.Cars, CarsComp.Cars.Length + 1);
                CarsComp.Cars[CarsComp.Cars.Length - 1] = car.carPrefab;
            }
        }

        internal static void BuildCar(Car car)
        {
            // Build our car
            CarGenUtils.RecursiveCarBuild(car);

            if(car.carGeneratorData.TransparentReferenceUpdate)
                CarBuilding.UpdateTransparentsReferences(car.carPrefab);

            if(car.carGeneratorData.BoneTargetTransformFix)
            {
                foreach (MyBoneSCR scr in car.carPrefab.GetComponentsInChildren<MyBoneSCR>())
                {
                    if (scr.thisTransform != null)
                    {
                        if (scr.thisTransform.root != car.carPrefab.transform)
                        {
                            scr.thisTransform = scr.transform;
                        }
                    }
                }
            }
            
            // Post build
            ICarBase baseData = (ICarBase)AvailableBases[car.carGeneratorData.BaseCarToUse];

            baseData.PostBuild(car.carPrefab, car);
            car.OnPostBuild?.Invoke(car.carPrefab);

            if(car.EnableDebug)
            {
                foreach (CarProperties carProps in car.carPrefab.GetComponentsInChildren<CarProperties>())
                {
                    if (carProps.Paintable && !carProps.GetComponent<P3dPaintableTexture>())
                    {
                        Debug.LogWarning("[ModUtils/CarGen/PostBuild/Warning]: CarProperties.Paintable set to true but missing P3D support on " + carProps.name);
                    }
                }
            }
        }
    }
}
