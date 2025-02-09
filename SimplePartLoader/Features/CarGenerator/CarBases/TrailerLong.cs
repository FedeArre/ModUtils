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

            // Fix handle
            var handle = objective.transform.Find("TrailerHandle");
            handle.GetComponent<ConfigurableJoint>().connectedBody = objective.GetComponent<Rigidbody>();
            handle.GetComponent<PickupHand>().trailer = objective;

            // NRigidbody
            objective.AddComponent<NRigidbody>();

            // Bone fix
            /*foreach (MyBoneSCR scr in objective.GetComponentsInChildren<MyBoneSCR>())
            {
                // Control arms use all same formula for this.
                // The error reporter shows that there are issues with these but actually the targetTransform is calculated on runtime!
                // Same happens with shock absorbers & springs
                if (scr.transform.childCount != 0 && scr.transform.GetChild(0).name.Contains("Pivot"))
                {
                    scr.LocalStrtetchTarget = scr.transform.GetChild(0);
                    scr.targetTransform = null;
                }
                else if (scr.transform.name == "HandbbrakeCable")
                {
                    scr.LocalStrtetchTarget = scr.transform.parent.Find("DummyPivHbrak");
                }
                else if (scr.name == "RearAxle06")
                {
                    Transform NonRot1 = null, NonRot2 = null;

                    foreach (Transform child in objective.transform.Find("RearSusp").GetComponentsInChildren<Transform>())
                    {
                        if (child.name == "NonROtVIsualANDrAxlePivot")
                        {
                            if (NonRot1)
                                NonRot2 = child;
                            else
                                NonRot1 = child;
                        }

                        if (NonRot1 && NonRot2)
                            break;
                    }
                    scr.targetTransform = NonRot2;
                    scr.targetTransformB = NonRot1;
                }
                else if (scr.transform.name == "DriveShaft06")
                {
                    scr.targetTransform = objective.transform.Find("RearSusp/RearAxle06/Pivotdriveshaft");
                }
            }*/

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

            /*MainCarProperties mcp = objective.GetComponent<MainCarProperties>();

            // Front & rear suspension bone fixes
            Transform FrontSusp = objective.transform.Find("FrontSusp");
            Transform RearSusp = objective.transform.Find("RearSusp");
            Transform EnginneTranny = objective.transform.Find("EngineTranny");

            MyBoneSCR MyBoneComponentF = FrontSusp.GetComponent<MyBoneSCR>();
            MyBoneSCR MyBoneComponentR = RearSusp.GetComponent<MyBoneSCR>();
            MyBoneSCR MyBoneComponentEngine = EnginneTranny.GetComponent<MyBoneSCR>();

            MyBoneComponentF.thisTransform = FrontSusp;
            MyBoneComponentF.targetTransform = objective.transform.Find("FRSuspensionPosition");
            MyBoneComponentF.targetTransformB = objective.transform.Find("FLSuspensionPosition");

            MyBoneComponentR.thisTransform = RearSusp;
            MyBoneComponentR.targetTransform = objective.transform.Find("RRSuspensionPosition");
            MyBoneComponentR.targetTransformB = objective.transform.Find("RLSuspensionPosition");

            MyBoneComponentEngine.thisTransform = EnginneTranny;
            MyBoneComponentEngine.targetTransform = objective.transform.Find("FRSuspensionPosition");
            MyBoneComponentEngine.targetTransformB = objective.transform.Find("FLSuspensionPosition");

            // Custom brake line setup
            if (car.carGeneratorData.EnableCustomBrakeLine)
            {
                Transform brakeLine = objective.transform.Find("MainBrakeLine");
                if (brakeLine)
                {
                    transparents brakeLineTransparent = brakeLine.GetComponent<transparents>();
                    brakeLineTransparent.ChildrenMesh = car.carGeneratorData.BrakeLineMesh;
                    brakeLineTransparent.ChildrenMesh1 = car.carGeneratorData.BrakeLineMesh;
                    brakeLineTransparent.ChildrenMesh2 = car.carGeneratorData.BrakeLineMesh;

                    Transform brakePivotFR = brakeLine.Find("FRbrakePivot");
                    Transform brakePivotFL = brakeLine.Find("FLbrakePivot");
                    Transform brakePivotRR = brakeLine.Find("RRbrakePivot");
                    Transform brakePivotRL = brakeLine.Find("RLbrakePivot");

                    brakePivotFR.transform.localPosition = car.carGeneratorData.FrontRightPivot;
                    brakePivotFL.transform.localPosition = car.carGeneratorData.FrontLeftPivot;
                    brakePivotRR.transform.localPosition = car.carGeneratorData.RearRightPivot;
                    brakePivotRL.transform.localPosition = car.carGeneratorData.RearLeftPivot;
                }
            }

            // Custom wire setup
            Transform wiresMain = objective.transform.Find("WiresMain06");
            if (wiresMain)
            {
                transparents wiresMainTransparent = wiresMain.GetComponent<transparents>();
                if (car.carGeneratorData.Inline4BatteryWires)
                {
                    wiresMainTransparent.ChildrenMesh = car.carGeneratorData.Inline4BatteryWires;
                }

                if (car.carGeneratorData.V8EngineBatteryWires)
                {
                    wiresMainTransparent.ChildrenMesh1 = car.carGeneratorData.V8EngineBatteryWires;
                }

                if (car.carGeneratorData.Inline6BatteryWires)
                {
                    wiresMainTransparent.ChildrenMesh2 = car.carGeneratorData.Inline6BatteryWires;
                }

                if (car.carGeneratorData.Inline6DieselBatteryWires)
                {
                    wiresMainTransparent.ChildrenMesh3 = car.carGeneratorData.Inline6DieselBatteryWires;
                }
            }

            // Fuel line setup
            Transform fuelLine = objective.transform.Find("FuelLine");
            if (fuelLine)
            {
                transparents fuelLineTransparent = fuelLine.GetComponent<transparents>();

                if (car.carGeneratorData.Inline4FuelLine)
                {
                    fuelLineTransparent.ChildrenMesh = car.carGeneratorData.Inline4FuelLine;
                }

                if (car.carGeneratorData.V8EngineFuelLine)
                {
                    fuelLineTransparent.ChildrenMesh1 = car.carGeneratorData.V8EngineFuelLine;
                }

                if (car.carGeneratorData.Inline6FuelLine)
                {
                    fuelLineTransparent.ChildrenMesh2 = car.carGeneratorData.Inline6FuelLine;
                }

                if (car.carGeneratorData.Inline6DieselBatteryWires)
                {
                    fuelLineTransparent.ChildrenMesh3 = car.carGeneratorData.Inline6DieselBatteryWires;
                }
            }

            // Handbrake fix
            Transform Hbrake = objective.transform.Find("Hbrake");
            Hbrake.GetComponent<HingeJoint>().connectedBody = objective.transform.GetComponent<Rigidbody>();
            mcp.HandbrakeObject = Hbrake.gameObject;
            Hbrake.GetComponent<Rigidbody>().mass = 0.1f;
            Hbrake.GetComponent<MeshCollider>().isTrigger = true;

            // NWH vehicle controller creation
            if (!car.carGeneratorData.Disable_NWH_Rebuild)
                RecreateNWH(objective, mcp, car.carGeneratorData.EnableAWD);

            // Fix crash sound
            objective.GetComponent<RVP.VehicleDamage>().crashSnd = objective.transform.Find("CrashSound").GetComponent<AudioSource>();

            // Fixing particle system (Smoke)
            SMOKE smokeComponent = objective.transform.Find("ExhaustSmoke").GetComponent<SMOKE>();
            SMOKE smokeComponent2 = objective.transform.Find("ExhaustSmoke2").GetComponent<SMOKE>();
            smokeComponent.particleSystems = new List<ParticleSystem>();
            smokeComponent2.particleSystems = new List<ParticleSystem>();
            smokeComponent.particleSystems.Add(smokeComponent.GetComponent<ParticleSystem>());
            smokeComponent2.particleSystems.Add(smokeComponent2.GetComponent<ParticleSystem>());

            // Fixing RVP.SUSP
            Transform WheelControllerFR = objective.transform.Find("FrontSusp/Crossmemmber06/WheelContParentFR");
            Transform WheelControllerFL = objective.transform.Find("FrontSusp/Crossmemmber06/WheelContParentFL");

            WheelControllerFL.GetComponent<RVP.SUSP>().tr = WheelControllerFL;
            WheelControllerFR.GetComponent<RVP.SUSP>().tr = WheelControllerFR;

            
        }

        void RecreateNWH(GameObject objective, MainCarProperties mcp, bool enableAWD)
        {
            WheelController WheelFR, WheelFL, WheelRL, WheelRR;
            WheelComponent NWH_WheelFR, NWH_WheelFL, NWH_WheelRR, NWH_WheelRL;
            WheelGroup FrontGroup = new WheelGroup(), RearGroup = new WheelGroup();
            List<WheelComponent> WheelControllers = new List<WheelComponent>();
            List<WheelGroup> WheelGroups = new List<WheelGroup>();

            VehicleController vehController = objective.GetComponent<VehicleController>();
            Powertrain oldPowertrain = vehController.powertrain;
            Powertrain vehPowertrain = new Powertrain();
            TransmissionComponent newTransmission = new TransmissionComponent();
            ClutchComponent newClutch = new ClutchComponent();
            EngineComponent newEngine = new EngineComponent();
            Steering newSteering = new Steering();
            Steering oldSteering = vehController.steering;

            newSteering.linearity = oldSteering.linearity;
            newSteering.maximumSteerAngle = 44;
            newSteering.returnToCenter = true;
            newSteering.smoothing = 0.05f;
            newSteering.steeringWheelTurnRatio = 5;
            newSteering.useDirectInput = false;

            vehController.steering = newSteering;

            OutputSelector outputSelectorEngine = new OutputSelector();
            OutputSelector outputSelectorTransmission = new OutputSelector();
            OutputSelector outputSelectorClutch = new OutputSelector();
            outputSelectorEngine.name = "Engine";
            outputSelectorClutch.name = "Clutch";
            outputSelectorTransmission.name = "Transmission";

            CarGenUtils.SetPrivatePropertyValue(newClutch, "outputASelector", outputSelectorTransmission);
            CarGenUtils.SetPrivatePropertyValue(newEngine, "outputASelector", outputSelectorClutch);

            vehPowertrain.clutch = newClutch;
            vehPowertrain.differentials = new List<DifferentialComponent>();
            vehPowertrain.engine = newEngine;
            vehPowertrain.transmission = newTransmission;

            newClutch.baseEngagementRPM = 1200f;
            newClutch.clutchEngagement = 0f;
            newClutch.engagementSpeed = 2.5f;
            newClutch.finalEngagementRPM = 1201.434f;
            newClutch.fwdAcceleration = 0.0195f;
            newClutch.slipTorque = 5000;
            newClutch.variableEngagementRPMRange = 1400;
            newClutch.inertia = 0.04f;
            newClutch.name = "Clutch";
            newClutch.startSignal = false;
            newClutch.shiftSignal = false;
            newClutch.isAutomatic = true;
            newClutch.PID_Kd = 2;
            newClutch.PID_Ki = 0.8f;
            newClutch.PID_Kp = 8;

            newEngine.engineType = EngineComponent.EngineType.ICE;
            newEngine.engineLayout = EngineComponent.EngineLayout.Longitudinal;
            newEngine.idleRPM = 800;
            newEngine.lossTorque = 0f;
            newEngine.maxLossTorque = 100;
            newEngine.maxRPM = 8040;
            newEngine.minRPM = -50;
            newEngine.powerCurve = oldPowertrain.engine.powerCurve;
            newEngine.revLimiterEnabled = true;
            newEngine.revLimiterActive = false;
            newEngine.revLimiterRPM = 6700;
            newEngine.slipTorque = 5000;
            newEngine.stallingEnabled = true;
            newEngine.stallRPM = 300;
            newEngine.starterRPMLimit = 500;
            newEngine.starterRunTime = 1;
            newEngine.starterTorque = 60;
            newEngine.name = "Engine";
            newEngine.inertia = 0.23f;
            newEngine.componentInputIsNull = true;
            newEngine.autoStartOnThrottle = false;

            newTransmission.finalGearRatio = 3.8f;
            newTransmission.ignorePostShiftBanInManual = true;
            newTransmission.postShiftBan = 0.5f;
            newTransmission.revMatch = true;
            newTransmission.shiftCheckCooldown = 0.1f;
            newTransmission.shiftDuration = 0.2f;
            newTransmission.variableShiftIntensity = 0.3f;
            newTransmission.variableShiftPoint = true;
            newTransmission.name = "Transmission";
            newTransmission.inertia = 0.02f;

            vehController.powertrain = vehPowertrain;

            WheelFR = objective.transform.Find("FrontSusp/Crossmemmber06/WheelContParentFR/WheelControllerFR").GetComponent<WheelController>();
            WheelFL = objective.transform.Find("FrontSusp/Crossmemmber06/WheelContParentFL/WheelControllerFL").GetComponent<WheelController>();
            WheelRR = objective.transform.Find("RearSusp/WheelControllerRR").GetComponent<WheelController>();
            WheelRL = objective.transform.Find("RearSusp/WheelControllerRL").GetComponent<WheelController>();

            mcp.FRwhellcontroller = WheelFR.gameObject;
            mcp.FLwhellcontroller = WheelFL.gameObject;
            mcp.RRwhellcontroller = WheelRR.gameObject;
            mcp.RLwhellcontroller = WheelRL.gameObject;

            WheelFR.parent = objective;
            WheelFL.parent = objective;
            WheelRR.parent = objective;
            WheelRL.parent = objective;

            // Now we fix the list in VehicleController
            NWH_WheelFR = new NWH.VehiclePhysics2.Powertrain.WheelComponent();
            NWH_WheelFR.wheelController = WheelFR;
            NWH_WheelFR.wheelGroupSelector.index = 0;
            NWH_WheelFR.name = "WheelWheelControllerFR";

            NWH_WheelFL = new NWH.VehiclePhysics2.Powertrain.WheelComponent();
            NWH_WheelFL.wheelController = WheelFL;
            NWH_WheelFL.wheelGroupSelector.index = 0;
            NWH_WheelFL.name = "WheelWheelControllerFL";

            NWH_WheelRR = new NWH.VehiclePhysics2.Powertrain.WheelComponent();
            NWH_WheelRR.wheelController = WheelRR;
            NWH_WheelRR.wheelGroupSelector.index = 1;
            NWH_WheelRR.name = "WheelWheelControllerRR";

            NWH_WheelRL = new NWH.VehiclePhysics2.Powertrain.WheelComponent();
            NWH_WheelRL.wheelController = WheelRL;
            NWH_WheelRL.wheelGroupSelector.index = 1;
            NWH_WheelRL.name = "WheelWheelControllerRL";

            WheelControllers.Add(NWH_WheelRR);
            WheelControllers.Add(NWH_WheelRL);
            WheelControllers.Add(NWH_WheelFR);
            WheelControllers.Add(NWH_WheelFL);

            FrontGroup.name = "Front";
            FrontGroup.ackermanPercent = 0.14f;
            FrontGroup.brakeCoefficient = 1;
            FrontGroup.camberAtBottom = 1;
            FrontGroup.camberAtTop = -5;
            FrontGroup.steerCoefficient = 1;
            FrontGroup.trackWidth = 1.4559f;

            RearGroup.name = "Rear";
            RearGroup.ackermanPercent = 0;
            RearGroup.antiRollBarForce = 3000;
            RearGroup.brakeCoefficient = 0.5f;
            RearGroup.camberAtBottom = 1;
            RearGroup.camberAtTop = -5;
            RearGroup.handbrakeCoefficient = 2;
            RearGroup.steerCoefficient = 0;
            RearGroup.toeAngle = 0;
            RearGroup.trackWidth = 1.5f;

            WheelGroups.Add(FrontGroup);
            WheelGroups.Add(RearGroup);

            // Now we fix the powertrain too.
            vehPowertrain.wheelGroups = WheelGroups;
            vehPowertrain.wheels = WheelControllers;

            FrontGroup.FindBelongingWheels();
            RearGroup.FindBelongingWheels();

            // Differentials setup
            if (enableAWD)
            {
                DifferentialComponent RearDiff = new DifferentialComponent("Rear Differential", NWH_WheelRL, NWH_WheelRR);
                DifferentialComponent FrontDiff = new DifferentialComponent("Front Differential", NWH_WheelFL, NWH_WheelFR);
                DifferentialComponent TransferCase = new DifferentialComponent("TransferCase", FrontDiff, RearDiff);

                List<DifferentialComponent> Diffs = new List<DifferentialComponent>();
                Diffs.Add(RearDiff);
                Diffs.Add(TransferCase);
                Diffs.Add(FrontDiff);

                TransferCase.biasAB = 1;
                TransferCase.coastRamp = 0.5f;
                TransferCase.powerRamp = 1;
                TransferCase.preload = 10;
                TransferCase.slipTorque = 1000;
                TransferCase.stiffness = 1;
                TransferCase.inertia = 0.02f;
                TransferCase.input = vehPowertrain.transmission;
                TransferCase.differentialType = DifferentialComponent.Type.Locked;

                RearDiff.input = TransferCase;
                RearDiff.biasAB = 0.5f;
                RearDiff.coastRamp = 0.5f;
                RearDiff.powerRamp = 1;
                RearDiff.preload = 10;
                RearDiff.slipTorque = 10000;
                RearDiff.stiffness = 0.5f;

                FrontDiff.input = TransferCase;
                FrontDiff.biasAB = 0.5f;
                FrontDiff.coastRamp = 0.5f;
                FrontDiff.powerRamp = 1;
                FrontDiff.preload = 5;
                FrontDiff.slipTorque = 1200;
                FrontDiff.stiffness = 0.5f;
                FrontDiff.differentialType = DifferentialComponent.Type.Open;

                OutputSelector outputSelectorFR = new OutputSelector();
                OutputSelector outputSelectorFL = new OutputSelector();
                OutputSelector outputSelectorRL = new OutputSelector();
                OutputSelector outputSelectorRR = new OutputSelector();
                OutputSelector outputSelectorFrontDiff = new OutputSelector();
                OutputSelector outputSelectorRearDiff = new OutputSelector();
                OutputSelector outputSelectorTransfer = new OutputSelector();
                outputSelectorFR.name = "WheelWheelControllerFR";
                outputSelectorFL.name = "WheelWheelControllerFL";
                outputSelectorRL.name = "WheelWheelControllerRL";
                outputSelectorRR.name = "WheelWheelControllerRR";
                outputSelectorFrontDiff.name = "Front Differential";
                outputSelectorRearDiff.name = "Rear Differential";
                outputSelectorTransfer.name = "TransferCase";

                vehPowertrain.transmission.outputA = TransferCase;

                CarGenUtils.SetPrivatePropertyValue(newTransmission, "outputASelector", outputSelectorTransfer);
                CarGenUtils.SetPrivatePropertyValue(FrontDiff, "outputASelector", outputSelectorFL);
                CarGenUtils.SetPrivatePropertyValue(FrontDiff, "outputBSelector", outputSelectorFR);
                CarGenUtils.SetPrivatePropertyValue(RearDiff, "outputASelector", outputSelectorRL);
                CarGenUtils.SetPrivatePropertyValue(RearDiff, "outputBSelector", outputSelectorRR);
                CarGenUtils.SetPrivatePropertyValue(TransferCase, "outputASelector", outputSelectorFrontDiff);
                CarGenUtils.SetPrivatePropertyValue(TransferCase, "outputBSelector", outputSelectorRearDiff);
                CarGenUtils.SetPrivatePropertyValue(vehPowertrain.transmission, "outputASelector", outputSelectorTransfer);

                vehPowertrain.differentials = Diffs;

                mcp.AWD = true;
            }
            else
            {
                DifferentialComponent RearDiff = new DifferentialComponent("Rear Differential", NWH_WheelRL, NWH_WheelRR);
                List<DifferentialComponent> Diffs = new List<DifferentialComponent>();
                Diffs.Add(RearDiff);

                RearDiff.input = newTransmission;
                RearDiff.biasAB = 0.5f;
                RearDiff.coastRamp = 0.5f;
                RearDiff.powerRamp = 1;
                RearDiff.preload = 10;
                RearDiff.slipTorque = 10000;
                RearDiff.stiffness = 0.1f;

                OutputSelector outputSelectorRL = new OutputSelector();
                OutputSelector outputSelectorRR = new OutputSelector();
                OutputSelector outputSelectorRearDiff = new OutputSelector();

                outputSelectorRL.name = "WheelWheelControllerRL";
                outputSelectorRR.name = "WheelWheelControllerRR";
                outputSelectorRearDiff.name = "Rear Differential";

                CarGenUtils.SetPrivatePropertyValue(RearDiff, "outputASelector", outputSelectorRL);
                CarGenUtils.SetPrivatePropertyValue(RearDiff, "outputBSelector", outputSelectorRR);
                CarGenUtils.SetPrivatePropertyValue(vehPowertrain.transmission, "outputASelector", outputSelectorRearDiff);

                vehPowertrain.transmission.outputA = RearDiff;
                vehPowertrain.differentials = Diffs;
            }

            // Brakes & some other stuff
            Brakes vehBrakes = new Brakes();
            vehBrakes.HandbrakeWorkingOrder = true;
            vehBrakes.StartedToBrake = true;
            vehBrakes.maxTorque = 1f;
            vehBrakes.brakeOffThrottleStrength = 0.1f;
            vehBrakes.MainProp = mcp;
            vehBrakes.brakeWhileAsleep = false;
            vehBrakes.brakeWhileIdle = false;

            vehController.brakes = vehBrakes;

            FrontGroup.antiRollBarForce = 0.6f;
            RearGroup.antiRollBarForce = 1f;
        }*/
        }
    }
}