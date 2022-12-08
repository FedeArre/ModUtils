using SimplePartLoader.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
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
                Debug.LogError("[ModUtils/CommonFixes]: Invalid part was passed on");
                return;
            }

            FixPart(p.Prefab, type, p.Mod.Settings.EnableDeveloperLog);
        }

        public static void FixPart(GameObject prefab, FixType type, bool printData = true)
        {
            if (!prefab)
            {
                Debug.LogError("[ModUtils/CommonFixes]: Invalid part was passed on");
                return;
            }

            try
            {
                switch (type)
                {
                    case FixType.CarLights:
                        CarLightsFix(prefab, printData);
                        break;

                    case FixType.DriverPassangerSeat:
                        SeatFix(prefab, printData);
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
                }
            }
            catch(Exception ex)
            {
                Debug.LogError("[ModUtils/CommonFixes/Error]: A fatal error occured while applying a CommonFix");
                Debug.LogError("[ModUtils/CommonFixes/Error]: Part name: " + prefab.name);
                Debug.LogError("[ModUtils/CommonFixes/Error]: Fix: " + type);
                Debug.LogError("[ModUtils/CommonFixes/Error]: Exception: " + ex.Message);
                Debug.LogError("[ModUtils/CommonFixes/Error]: StackTrace: " + ex.StackTrace);
            }
            
        }

        // 1 function per FixType
        internal static void CarLightsFix(GameObject prefab, bool printData)
        {
            foreach (CarLight cl in prefab.GetComponentsInChildren<CarLight>())
            {
                cl.gameObject.AddComponent<LightFix>();
                if (printData)
                    Debug.Log($"[ModUtils/CommonFix/Light]: Adding light fix to {prefab} ({cl.gameObject.name})");
            }
        }

        internal static void RadiatorFix(GameObject prefab, bool printData)
        {
            if(printData)
                Debug.Log($"[ModUtils/CommonFix/Radiator]: Applying radiator fix to {prefab}");
            
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
            if(printData)
                Debug.Log($"[ModUtils/CommonFix/FuelTank]: Applying fuel tank fix to {prefab}");
            
            Transform FuelTankContainer = prefab.transform.Find("FuelContainer");
            Transform FuelTankCup = prefab.transform.Find("FuelReservuarCUP/FuelReservuarCUP");
            FLUID FuelTankComponent = FuelTankContainer.GetComponent<FLUID>();
            PickupCup FuelTankCupComponent = FuelTankCup.GetComponent<PickupCup>();

            FuelTankComponent.Container = FuelTankComponent;
            FuelTankCupComponent.Fluid = FuelTankContainer.gameObject;
        }

        internal static void BrakeCylinderFix(GameObject prefab, bool printData)
        {
            if (printData)
                Debug.Log($"[ModUtils/CommonFix/BrakeCylinder]: Applying brake cylinder fix to {prefab}");

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
            if (printData)
                Debug.Log($"[ModUtils/CommonFix/Dipstick]: Applying dipstick fix to {prefab}");

            Transform Dipstick = prefab.transform.Find("Dipstick/Dipstick");
            if(!Dipstick)
            {
                Debug.LogError("[ModUtils/CommonFix/Dipstick]: Dipstick not found. Make sure to be passing the cylinder block as parameter!");
                return;
            }
            
            Transform DipstickLevel = Dipstick.GetChild(0);
            Dipstick.GetComponent<PickupCup>().DipstickOil = DipstickLevel.gameObject;
        }

        internal static void CylinderHeadCoverFix(GameObject prefab, bool printData)
        {
            if (printData)
                Debug.Log($"[ModUtils/CommonFix/CylinderHeadCover]: Applying cylinder head cover fix to {prefab}");

            Transform CylinderHeadContainer = prefab.transform.Find("OilFluidContainerHead");
            Transform CylinderHeadCup = prefab.transform.Find("OilReservuarCUP/OilReservuarCUP");
            FLUID OilCylinderHeadComponent = CylinderHeadContainer.GetComponent<FLUID>();
            PickupCup OilCupCylinderHead = CylinderHeadCup.GetComponent<PickupCup>();

            OilCylinderHeadComponent.Container = OilCylinderHeadComponent;
            OilCupCylinderHead.Fluid = CylinderHeadContainer.gameObject;
        }

        internal static void OilpanFix(GameObject prefab, bool printData)
        {
            if (printData)
                Debug.Log($"[ModUtils/CommonFix/Oilpan]: Applying oilpan fix to {prefab}");

            Transform OilpanContainer = prefab.transform.Find("OilFluidContainer");
            Transform OilpanCup = prefab.transform.Find("OilReservuarSCREW/OilReservuarSCREW");
            FLUID OilpanComponent = OilpanContainer.GetComponent<FLUID>();
            PickupCup OilpanCupComponent = OilpanCup.GetComponent<PickupCup>();

            OilpanComponent.Container = OilpanComponent;
            OilpanCupComponent.Fluid = OilpanContainer.gameObject;
        }

        internal static void SeatFix(GameObject prefab, bool printData)
        {
            if (printData)
                Debug.Log($"[ModUtils/CommonFix/Seat]: Applying seat fix to {prefab}");

            Transform Seat = prefab.transform.Find("SitDrive");
            Seat.GetComponent<Sit>().seat = prefab;
        }
        
        internal static void Windows(GameObject prefab, bool printData)
        {
            if (printData)
                Debug.Log($"[ModUtils/CommonFix/Windows]: Applying windows fix to {prefab}");

            foreach (RVP.ShatterPart shatterComp in prefab.GetComponentsInChildren<RVP.ShatterPart>())
            {
                Transform t = shatterComp.transform.Find("ShatterParticles");
                if (t)
                {
                    shatterComp.shatterParticles = t.GetComponent<ParticleSystem>();
                    Debug.Log("[ModUtils/CommonFix/Windows]: RVP.ShatterPart (windows) fix applied to " + shatterComp.name);
                }
                else
                {
                    Debug.Log("[ModUtils/CommonFix/Windows]: RVP.ShatterPart (windows) fix could not be applied to " + shatterComp.name);
                }
            }
        }
        internal static void Cluster(GameObject prefab, bool printData)
        {
            if (printData)
                Debug.Log($"[ModUtils/CommonFix/Cluster]: Applying cluster fix to {prefab}");

            CarProperties clusterProps = prefab.GetComponent<CarProperties>();

            Transform batLight = prefab.transform.Find("BatLight");
            Transform high = prefab.transform.Find("High");
            Transform left = prefab.transform.Find("Left");
            Transform right = prefab.transform.Find("Right");

            if(batLight)
                clusterProps.ClusterBat = batLight.gameObject;
            else if (printData)
                Debug.Log("[ModUtils/CommonFix/Cluster]: BatLight not found at prefab " + prefab);

            if (high)
                clusterProps.ClusterHigh = high.gameObject;
            else if (printData)
                Debug.Log("[ModUtils/CommonFix/Cluster]: High not found at prefab " + prefab);
            
            if (left)
                clusterProps.ClusterL = left.gameObject;
            else if (printData)
                Debug.Log("[ModUtils/CommonFix/Cluster]: Left not found at prefab " + prefab);
            
            if (right)
                clusterProps.ClusterR = right.gameObject;
            else if (printData)
                Debug.Log("[ModUtils/CommonFix/Cluster]: Right not found at prefab " + prefab);
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
        Cluster
    }
}
