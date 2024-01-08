using SimplePartLoader.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SimplePartLoader
{
    public class CommonFixes
    {
        public static void FixPart(Part p, FixType type)
        {
            if(p == null)
            {
                CustomLogger.AddLine("CommonFixes", $"Invalid part was passed on");
                return;
            }

            FixPart(p.Prefab, type);
        }

        public static void FixPart(GameObject prefab, FixType type, bool printData = true)
        {
            if (!prefab)
            {
                CustomLogger.AddLine("CommonFixes", $"Invalid part was passed on");
                return;
            }

            try
            {
                switch (type)
                {
                    case FixType.CarLights:
                        CarLightsFix(prefab, printData);
                        break;

                    case FixType.Oilpan:
                        OilpanFix(prefab, printData);
                        break;

                    case FixType.Radiator:
                        RadiatorFix(prefab, printData);
                        break;

                    case FixType.CylinderHeadCover:
                        CylinderHeadCoverFix(prefab, printData);
                        break;

                    case FixType.BrakeCylinder:
                        BrakeCylinderFix(prefab, printData);
                        break;

                    case FixType.Windows:
                        Windows(prefab, printData);
                        break;

                    case FixType.Dipstick:
                        DipstickFix(prefab, printData);
                        break;

                    case FixType.FuelTank:
                        FuelTankFix(prefab, printData);
                        break;

                    case FixType.Cluster:
                        Cluster(prefab, printData);
                        break;

                    case FixType.MyBoneSCR:
                        Bones(prefab);
                        break;
                }
            }
            catch(Exception ex)
            {
                CustomLogger.AddLine("CommonFixes", ex);
            }
            
        }

        // 1 function per FixType
        internal static void CarLightsFix(GameObject prefab, bool printData)
        {
            foreach (CarLight cl in prefab.GetComponentsInChildren<CarLight>())
            {
                cl.gameObject.AddComponent<LightFix>();
            }
        }

        internal static void RadiatorFix(GameObject prefab, bool printData)
        {
            Transform CoolantContainer = prefab.transform.Find("CoolantFluidContainer");
            Transform CoolantFluidLevel = prefab.transform.Find("VisualFLuid");
            Transform CoolantCupTransform = prefab.transform.Find("CoolantFluidReservuarCUP/CoolantFluidReservuarCUP");
            FLUID CoolantFluid = CoolantContainer.GetComponent<FLUID>();
            PickupCup CoolantCup = CoolantCupTransform.GetComponent<PickupCup>();

            CoolantFluid.Container = CoolantFluid;
            CoolantFluid.VisualFluid = CoolantFluidLevel.gameObject;
            CoolantCup.Fluid = CoolantContainer.gameObject;
        }

        internal static void FuelTankFix(GameObject prefab, bool printData)
        {
            Transform FuelTankContainer = prefab.transform.Find("FuelContainer");
            Transform FuelTankCup = prefab.transform.Find("FuelReservuarCUP/FuelReservuarCUP");
            FLUID FuelTankComponent = FuelTankContainer.GetComponent<FLUID>();
            PickupCup FuelTankCupComponent = FuelTankCup.GetComponent<PickupCup>();

            FuelTankComponent.Container = FuelTankComponent;
            FuelTankCupComponent.Fluid = FuelTankContainer.gameObject;
        }

        internal static void BrakeCylinderFix(GameObject prefab, bool printData)
        {
            Transform BrakeFluidContainer = prefab.transform.Find("BrakeFluidContainer");
            Transform BrakeFluidVisual = prefab.transform.Find("VisualFluid");
            Transform BrakeFluidCup = prefab.transform.Find("BrakeFluidReservuarCUP/BrakeFluidReservuarCUP");
            PickupCup BrakeCupComponent = BrakeFluidCup.GetComponent<PickupCup>();
            FLUID BrakeFluidComponent = BrakeFluidContainer.GetComponent<FLUID>();

            BrakeFluidComponent.Container = BrakeFluidComponent;
            BrakeFluidComponent.VisualFluid = BrakeFluidVisual.gameObject;
            BrakeCupComponent.Fluid = BrakeFluidComponent.gameObject;
        }

        internal static void DipstickFix(GameObject prefab, bool printData)
        {
            Transform Dipstick = prefab.transform.Find("Dipstick/Dipstick");
            if(!Dipstick)
            {
                CustomLogger.AddLine("CommonFixes", $"Dipstick not found. Make sure to be passing the cylinder block as parameter! - " + prefab.name);
                return;
            }
            
            Transform DipstickLevel = Dipstick.GetChild(0);
            Dipstick.GetComponent<PickupCup>().DipstickOil = DipstickLevel.gameObject;
        }

        internal static void CylinderHeadCoverFix(GameObject prefab, bool printData)
        {
            Transform CylinderHeadContainer = prefab.transform.Find("OilFluidContainerHead");
            Transform CylinderHeadCup = prefab.transform.Find("OilReservuarCUP/OilReservuarCUP");
            FLUID OilCylinderHeadComponent = CylinderHeadContainer.GetComponent<FLUID>();
            PickupCup OilCupCylinderHead = CylinderHeadCup.GetComponent<PickupCup>();

            OilCylinderHeadComponent.Container = OilCylinderHeadComponent;
            OilCupCylinderHead.Fluid = CylinderHeadContainer.gameObject;
        }

        internal static void OilpanFix(GameObject prefab, bool printData)
        {
            Transform OilpanContainer = prefab.transform.Find("OilFluidContainer");
            Transform OilpanCup = prefab.transform.Find("OilReservuarSCREW/OilReservuarSCREW");
            FLUID OilpanComponent = OilpanContainer.GetComponent<FLUID>();
            PickupCup OilpanCupComponent = OilpanCup.GetComponent<PickupCup>();

            OilpanComponent.Container = OilpanComponent;
            OilpanCupComponent.Fluid = OilpanContainer.gameObject;
        }

        internal static void Windows(GameObject prefab, bool printData)
        {
            foreach (RVP.ShatterPart shatterComp in prefab.GetComponentsInChildren<RVP.ShatterPart>())
            {
                Transform t = shatterComp.transform.Find("ShatterParticles");
                if (t)
                {
                    shatterComp.shatterParticles = t.GetComponent<ParticleSystem>();
                }
            }
        }
        internal static void Cluster(GameObject prefab, bool printData)
        {
            CarProperties clusterProps = prefab.GetComponent<CarProperties>();

            Transform batLight = prefab.transform.Find("BatLight");
            Transform high = prefab.transform.Find("High");
            Transform left = prefab.transform.Find("Left");
            Transform right = prefab.transform.Find("Right");

            if(batLight)
                clusterProps.ClusterBat = batLight.gameObject;
            else if (printData)
                CustomLogger.AddLine("CommonFixes", $"BatLight not found at prefab " + prefab);

            if (high)
                clusterProps.ClusterHigh = high.gameObject;
            else if (printData)
                CustomLogger.AddLine("CommonFixes", $"High not found at prefab " + prefab);
            
            if (left)
                clusterProps.ClusterL = left.gameObject;
            else if (printData)
                CustomLogger.AddLine("CommonFixes", $"Left not found at prefab " + prefab);
            
            if (right)
                clusterProps.ClusterR = right.gameObject;
            else if (printData)
                CustomLogger.AddLine("CommonFixes", $"Right not found at prefab " + prefab);
        }

        internal static void Bones(GameObject prefab)
        {
            foreach (MyBoneSCR scr in prefab.GetComponentsInChildren<MyBoneSCR>())
            {
                if (scr.transform.childCount != 0 && scr.transform.GetChild(0).name.Contains("Pivot"))
                {
                    scr.LocalStrtetchTarget = scr.transform.GetChild(0);
                    scr.targetTransform = null;
                }
            }

            foreach (MyBoneSCR scr in prefab.GetComponentsInChildren<MyBoneSCR>())
            {
                if (scr.thisTransform != null)
                {
                    if (!scr.thisTransform.root != prefab.transform)
                    {
                        scr.thisTransform = scr.transform;
                    }
                }
            }
        }
    }

    public enum FixType
    {
        CarLights = 1,
        Radiator,
        FuelTank,
        BrakeCylinder,
        Dipstick,
        CylinderHeadCover,
        Oilpan,
        DriverPassangerSeat,
        Windows,
        Cluster,
        MyBoneSCR
    }
}
