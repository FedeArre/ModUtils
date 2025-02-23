using NWH.VehiclePhysics2.Powertrain.Wheel;
using NWH.VehiclePhysics2.Powertrain;
using NWH.VehiclePhysics2;
using NWH.WheelController3D;
using SimplePartLoader.CarGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using NWH.NPhysics;

namespace SimplePartLoader.Features.CarGenerator.CarBases
{
    internal class TrailerLong : ICarBase
    {
        public GameObject GetCar() => (GameObject)cachedResources.Load("TrailerCar");

        public VehicleType VehType()
        {
            return VehicleType.Trailer;
        }

        public void ForceTemplateExceptions(BuildingExceptions exceptions)
        {
            exceptions.ExceptionList["Winch"] = "none";
        }

        public void PostBuild(GameObject objective, Car car)
        {
            // Bone target fix
            foreach (MyBoneSCR scr in objective.GetComponentsInChildren<MyBoneSCR>())
            {
                if (CustomLogger.DebugEnabled)
                    CustomLogger.AddLine("CarDebug", $"Bone found at {scr.name} ({Utils.Functions.GetTransformPath(scr.transform)})");

                if (scr.transform.childCount != 0)
                {
                    if (scr.LocalStrtetchTarget != null)
                    {
                        Transform newBone = null;
                        foreach (Transform t in scr.transform)
                        {
                            if (t.name == scr.LocalStrtetchTarget.name)
                            {
                                newBone = t;
                                break;
                            }
                        }

                        if (newBone)
                        {
                            scr.LocalStrtetchTarget = newBone;
                        }
                    }
                }
            }

            // ModUtils LAD template
            if (car.carGeneratorData.DisableModUtilsTemplateSetup)
                return;

            // All fluid fixes
            Transform engine = objective.transform.Find("EngineTranny/CylinderBlock/CylinderBlock");
            if (engine)
            {
                engine.name = "CylinderBlock";

                CommonFixes.FixPart(engine.gameObject, FixType.Dipstick);
                CommonFixes.FixPart(engine.Find("OilPan06/OilPan06").gameObject, FixType.Oilpan);
                CommonFixes.FixPart(engine.Find("CylinderHead06/CylinderHead06/CylinderHeadCover06/CylinderHeadCover06").gameObject, FixType.CylinderHeadCover);

                FLUID OilContainerComponent = engine.Find("OilPan06/OilPan06/OilFluidContainer").GetComponent<FLUID>();

                OilContainerComponent.FluidSize = 2f;
                OilContainerComponent.Condition = 1f;

                OilContainerComponent.transform.parent.GetComponent<CarProperties>().FluidSize = 2f;
                OilContainerComponent.transform.parent.GetComponent<CarProperties>().FluidCondition = 1f;
            }

            Transform radiator = objective.transform.Find("Frontpanel06/Frontpanel06/Radiator06/Radiator06");
            if (radiator)
            {
                CommonFixes.FixPart(radiator.gameObject, FixType.Radiator);
                FLUID CoolantFluid = radiator.Find("CoolantFluidContainer").GetComponent<FLUID>();

                CoolantFluid.FluidSize = 3f;
                CoolantFluid.Condition = 1f;
                CoolantFluid.transform.parent.GetComponent<CarProperties>().FluidSize = 3f;
                CoolantFluid.transform.parent.GetComponent<CarProperties>().FluidCondition = 1f;
            }

            Transform gasTank = objective.transform.Find("FloorTrunk06/FloorTrunk06/GasTank06/GasTank06");
            if (gasTank)
            {
                CommonFixes.FixPart(gasTank.gameObject, FixType.FuelTank);

                FLUID FuelTankComponent = gasTank.Find("FuelContainer").GetComponent<FLUID>();

                FuelTankComponent.Condition = 1;
                FuelTankComponent.FluidSize = 25f;
                FuelTankComponent.transform.parent.GetComponent<CarProperties>().FluidSize = 25f;
                FuelTankComponent.transform.parent.GetComponent<CarProperties>().FluidCondition = 1f;
            }

            Transform brakeFluidContainer = objective.transform.Find("Firewall06/Firewall06/BrakeMasterCylinder06/BrakeMasterCylinder06");
            if (brakeFluidContainer)
            {
                CommonFixes.FixPart(brakeFluidContainer.gameObject, FixType.BrakeCylinder);
                FLUID BrakeFluidComponent = brakeFluidContainer.Find("BrakeFluidContainer").GetComponent<FLUID>();

                BrakeFluidComponent.FluidSize = BrakeFluidComponent.ContainerSize;
                BrakeFluidComponent.Condition = 1f;
                BrakeFluidComponent.transform.parent.GetComponent<CarProperties>().FluidSize = BrakeFluidComponent.ContainerSize - 0.01f;
                BrakeFluidComponent.transform.parent.GetComponent<CarProperties>().FluidCondition = 1f;
            }

            // Window lifts
            //FL
            Transform WindowLiftHandleLeft = objective.transform.Find("Firewall06/Firewall06/DoorFL06/DoorFL06/WindowLiftFL06/WindowLiftFL06/WIndowHandle.003");
            Transform WindowLiftTransparentLeft = objective.transform.Find("Firewall06/Firewall06/DoorFL06/DoorFL06/WindowLiftFL06/WindowLiftFL06/WindowFL06");

            if (WindowLiftHandleLeft && WindowLiftTransparentLeft)
                WindowLiftHandleLeft.GetComponent<WindowLift>().Window = WindowLiftTransparentLeft.gameObject;

            // FR
            Transform WindowLiftHandleRight = objective.transform.Find("Firewall06/Firewall06/DoorFR06/DoorFR06/WindowLiftFR06/WindowLiftFR06/WIndowHandle");
            Transform WindowLiftTransparentRight = objective.transform.Find("Firewall06/Firewall06/DoorFR06/DoorFR06/WindowLiftFR06/WindowLiftFR06/WindowFR06");

            if (WindowLiftHandleRight && WindowLiftTransparentRight)
                WindowLiftHandleRight.GetComponent<WindowLift>().Window = WindowLiftTransparentRight.gameObject;

            // RL
            Transform WindowLiftHandleRLeft = objective.transform.Find("LBpillar06/LBpillar06/DoorRL06/DoorRL06/WindowLiftRL06/WindowLiftRL06/WIndowHandle.002");
            Transform WindowLiftTransparentRLeft = objective.transform.Find("LBpillar06/LBpillar06/DoorRL06/DoorRL06/WindowLiftRL06/WindowLiftRL06/WindowRL06");

            if (WindowLiftHandleRLeft && WindowLiftTransparentRLeft)
                WindowLiftHandleRLeft.GetComponent<WindowLift>().Window = WindowLiftTransparentRLeft.gameObject;

            // RR
            Transform WindowLiftHandleRRight = objective.transform.Find("RBpillar06/RBpillar06/DoorRR06/DoorRR06/WindowLiftRR06/WindowLiftRR06/WIndowHandle.001");
            Transform WindowLiftTransparentRRight = objective.transform.Find("RBpillar06/RBpillar06/DoorRR06/DoorRR06/WindowLiftRR06/WindowLiftRR06/WindowRR06");

            if (WindowLiftHandleRRight && WindowLiftTransparentRRight)
                WindowLiftHandleRRight.GetComponent<WindowLift>().Window = WindowLiftTransparentRRight.gameObject;
        }

        public void SetupTemplate(GameObject objective, Car car)
        {
            // Fix WC on MTP
            MainTrailerProperties mtp = objective.GetComponent<MainTrailerProperties>();
            mtp.WCFL = objective.transform.Find("WheelControllerFL").GetComponent<WheelController>();
            mtp.WCFR = objective.transform.Find("WheelControllerFR").GetComponent<WheelController>();
            mtp.WCRR = objective.transform.Find("WheelControllerRR").GetComponent<WheelController>();
            mtp.WCRL = objective.transform.Find("WheelControllerRL").GetComponent<WheelController>();

            mtp.WCFL.parent = objective;
            mtp.WCFR.parent = objective;
            mtp.WCRL.parent = objective;
            mtp.WCRR.parent = objective;

            // Fix riser
            Transform Riser = objective.transform.Find("riser/hbrdown.001");
            Riser.GetComponent<Switch>().TrailerBed = objective.transform.Find("FRAMELONG.002");

            GameObject.Destroy(objective.transform.Find("riser/dbrup.001")?.gameObject ?? new GameObject()); // Unused game asset
            GameObject.Destroy(objective.transform.Find("Cube")?.gameObject ?? new GameObject()); // Unused game asset

            // Fix handle
            var handle = objective.transform.Find("TrailerHandle");
            handle.GetComponent<ConfigurableJoint>().connectedBody = objective.GetComponent<Rigidbody>();
            handle.GetComponent<PickupHand>().trailer = objective;

            // NRigidbody
            objective.AddComponent<NRigidbody>();

            // Bone fix
            bool leftDone = false;
            foreach (MyBoneSCR scr in objective.GetComponentsInChildren<MyBoneSCR>())
            {
                if (scr.name == "AxleTrailer")
                {
                    Transform NonRot1 = null, NonRot2 = null;

                    foreach (Transform child in objective.transform.GetComponentsInChildren<Transform>())
                    {
                        if (child.name == "NonROtVIsualANDrAxlePivot" && !leftDone)
                        {
                            if (NonRot1)
                                NonRot2 = child;
                            else
                                NonRot1 = child;
                        }

                        if(child.name == "NonROtVIsualANDrAxlePivotR" && leftDone)
                        {
                            if (NonRot1)
                                NonRot2 = child;
                            else
                                NonRot1 = child;
                        }

                        if (NonRot1 && NonRot2)
                            break;
                    }
                    scr.targetTransform = NonRot1;
                    scr.targetTransformB = NonRot2;
                    leftDone = true;
                }
            }

            foreach (MyBoneSCR scr in objective.GetComponentsInChildren<MyBoneSCR>())
            {
                if (scr.thisTransform != null)
                {
                    if (!scr.thisTransform.root != objective.transform)
                    {
                        scr.thisTransform = scr.transform;
                    }
                }
            }
        }
    }
}