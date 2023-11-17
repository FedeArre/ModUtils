using PaintIn3D;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SimplePartLoader.CarGen
{
    public class EmulatedJunkyard
    {
        public static void SpawnCar(GameObject car)
        {
            Debug.Log($"[ModUtils/EmulatedJunkyard]: Emulated junkyard - Spawning {car.name}");

            // Ignore CS0618 warning (This is game code copy)
#pragma warning disable CS0618
            UnityEngine.Random.seed = DateTime.Now.Millisecond + UnityEngine.Random.Range(0, 999999);
#pragma warning restore CS0618

            GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(car, new Vector3(UnityEngine.Random.Range(0.1f, 10f), UnityEngine.Random.Range(-99f, -70f), UnityEngine.Random.Range(0.1f, 10f)), Quaternion.Euler((float)UnityEngine.Random.Range(0, 360), (float)UnityEngine.Random.Range(0, 360), (float)UnityEngine.Random.Range(0, 360)));
            EmulateCreatingJunkyard(gameObject);
        }

        internal static void EmulateCreatingJunkyard(GameObject car)
        {
            Debug.Log("[ModUtils/EmulatedJunkyard]: Object instanciate done, setting mileage & color & start options.");
            
            if(!car.GetComponent<Rigidbody>())
                Debug.Log($"[ModUtils/EmulatedJunkyard]: CAR DOES NOT HAVE RIGIDBODY - THIS IS CRITICAL!");

            MainCarProperties mcp = car.GetComponent<MainCarProperties>();
            mcp.Mileage = (float)UnityEngine.Random.Range(1, 900000);
            mcp.Color = UnityEngine.Random.ColorHSV();
            mcp.StartOptions = UnityEngine.Random.Range(1000000000, 1999999999);

            Debug.Log("[ModUtils/EmulatedJunkyard]: SetCarOptions");
            mcp.SetCarOptions();

            Debug.Log("[ModUtils/EmulatedJunkyard]: NumberPlate stuff");

            mcp.NumberParent = GameObject.Find("NumberPlates").GetComponent<NumberPlateManager>();
            mcp.NumberParent.CreateRandomNumber();
            if (mcp.OriginalInterior >= 2)
            {
                mcp.OriginalColor = mcp.Color;
            }
            else
            {
                mcp.OriginalColor = UnityEngine.Random.ColorHSV();
            }

            Debug.Log($"[ModUtils/EmulatedJunkyard]: Starting part iteration. Spawner will now iterate over all CarProperties");
            foreach (CarProperties carProperties in car.GetComponentsInChildren<CarProperties>())
            {
                Debug.Log($"[ModUtils/EmulatedJunkyard]: Loop is now on {carProperties.name}");

                if (carProperties.Cluster && !mcp.Bike)
                {
                    Debug.Log("[ModUtils/EmulatedJunkyard]: Object is cluster (Cluster true - not on bike)");
                    carProperties.ClusterMileage = mcp.Mileage;
                    carProperties.MileageText.text = carProperties.ClusterMileage.ToString("F0");
                }

                carProperties.Condition = UnityEngine.Random.Range(0.01f, 1f);
                if (carProperties.EngineOil != null)
                {
                    Debug.Log("[ModUtils/EmulatedJunkyard]: EngineOil is not null - Setting condition");
                    carProperties.EngineOil.Condition = UnityEngine.Random.Range(0.01f, 0.5f);
                }

                if (carProperties.Paintable && carProperties.gameObject.GetComponent<P3dPaintableTexture>())
                {
                    Debug.Log("[ModUtils/EmulatedJunkyard]: Part is paintable and GO has P3dpaintabletexture, setting color on it");
                    if (UnityEngine.Random.Range(0, 25) < 3)
                    {
                        Debug.Log("[ModUtils/EmulatedJunkyard]: Random color");
                        carProperties.gameObject.GetComponent<P3dPaintableTexture>().Color = UnityEngine.Random.ColorHSV();
                    }
                    else
                    {
                        Debug.Log("[ModUtils/EmulatedJunkyard]: MCP color");
                        carProperties.gameObject.GetComponent<P3dPaintableTexture>().Color = mcp.Color;
                    }
                }

                if (carProperties.Washable)
                {
                    Debug.Log("[ModUtils/EmulatedJunkyard]: Part is washable, setting it as dirty. If this crashes your part does not have the proper paint setup");
                    Material[] materials = carProperties.gameObject.GetComponent<Renderer>().materials;
                    materials[1] = mcp.DirtyMaterial;
                    carProperties.gameObject.GetComponent<Renderer>().materials = materials;
                }

                if (carProperties.Tire)
                {
                    Debug.Log("[ModUtils/EmulatedJunkyard]: Part is a tire. This wont cause issues");
                    if (carProperties.Condition < 0.1f)
                    {
                        carProperties.PartIsOld = true;
                    }
                    carProperties.TirePressure = UnityEngine.Random.Range(0.1f, 4f);
                    if (carProperties.TirePressure > 2f)
                    {
                        carProperties.TirePressure = 2f;
                    }
                }

                if (carProperties.Interior)
                {
                    Debug.Log("[ModUtils/EmulatedJunkyard]: Interior part, setting up part color from car");
                    carProperties.OriginalInterior = mcp.OriginalInterior;
                }

                if (carProperties.NumberPlate)
                {
                    Debug.Log("[ModUtils/EmulatedJunkyard]: Part is NumberPlate, trying to set it up.");
                    Material[] materials2 = carProperties.gameObject.GetComponent<Renderer>().materials;
                    materials2[2] = mcp.NumberParent.M1;
                    materials2[3] = mcp.NumberParent.M2;
                    materials2[4] = mcp.NumberParent.M3;
                    materials2[5] = mcp.NumberParent.M4;
                    materials2[6] = mcp.NumberParent.M5;
                    materials2[7] = mcp.NumberParent.M6;
                    carProperties.gameObject.GetComponent<Renderer>().materials = materials2;
                    carProperties.One = mcp.NumberParent.M1;
                    carProperties.Two = mcp.NumberParent.M2;
                    carProperties.Three = mcp.NumberParent.M3;
                    carProperties.Four = mcp.NumberParent.M4;
                    carProperties.Five = mcp.NumberParent.M5;
                    carProperties.Six = mcp.NumberParent.M6;
                }
            }

            Debug.Log($"[ModUtils/EmulatedJunkyard]: Part 1 finished, starting part 2 (WaitCreating2)");

            foreach (CarProperties carProperties in car.GetComponentsInChildren<CarProperties>())
            {
                Debug.Log($"[ModUtils/EmulatedJunkyard]: Loop is now at {carProperties.name}. Single part {carProperties.SinglePart} - JunkSpawnChance: {carProperties.JunkSpawnChance}");

                if (carProperties.SinglePart)
                {
                    if (carProperties.JunkSpawnChance == 1)
                    {
                        Debug.Log($"[ModUtils/EmulatedJunkyard]: SinglePart & JunkyardSpawnChance 1, trying to delete part");
                        if (carProperties.gameObject.GetComponent<Pickup>() && !carProperties.Tire)
                        {
                            carProperties.gameObject.GetComponent<Pickup>().BRAKE2();
                        }
                        if (carProperties.gameObject.GetComponent<PickupDoor>())
                        {
                            carProperties.gameObject.GetComponent<PickupDoor>().BRAKE2();
                        }
                        UnityEngine.Object.Destroy(carProperties.transform.gameObject);
                    }
                    if (carProperties.Condition >= 0.6f && carProperties.JunkSpawnChance == 2)
                    {
                        Debug.Log($"[ModUtils/EmulatedJunkyard]: JunkSpawnChance is 2 and Condition is >= 0.6, trying to delete part");
                        
                        if (carProperties.gameObject.GetComponent<Pickup>() && !carProperties.Tire)
                        {
                            carProperties.gameObject.GetComponent<Pickup>().BRAKE2();
                        }
                        if (carProperties.gameObject.GetComponent<PickupDoor>())
                        {
                            carProperties.gameObject.GetComponent<PickupDoor>().BRAKE2();
                        }
                        UnityEngine.Object.Destroy(carProperties.transform.gameObject);
                    }
                    if (carProperties.Condition >= 0.8f && carProperties.JunkSpawnChance == 3)
                    {
                        Debug.Log($"[ModUtils/EmulatedJunkyard]: JunkSpawnChance is 3 and Condition is >= 0.8, trying to delete part");
                        
                        if (carProperties.gameObject.GetComponent<Pickup>() && !carProperties.Tire)
                        {
                            carProperties.gameObject.GetComponent<Pickup>().BRAKE2();
                        }
                        if (carProperties.gameObject.GetComponent<PickupDoor>())
                        {
                            carProperties.gameObject.GetComponent<PickupDoor>().BRAKE2();
                        }
                        UnityEngine.Object.Destroy(carProperties.transform.gameObject);
                    }
                    if (carProperties.Condition <= 0.4f && carProperties.JunkSpawnChance == 4 && carProperties.OldMaterial)
                    {
                        Debug.Log($"[ModUtils/EmulatedJunkyard]: JunkSpawnChance is 4, condition is <= 0.4 and has OldMaterial setup. Trying to make it old part. This crashes if part does not have renderer");

                        carProperties.Condition = 0.15f;
                        carProperties.PartIsOld = true;
                        carProperties.gameObject.GetComponent<Renderer>().sharedMaterial = carProperties.OldMaterial;
                    }

                    if (carProperties.Tintable && !carProperties.RuinedMaterial)
                    {
                        Debug.Log($"[ModUtils/EmulatedJunkyard]: Part is tintable and does not have RuinedMaterial setup on it. Setting condition to 1.00");

                        carProperties.Condition = 1f;
                    }
                    if (carProperties.CantRust && !carProperties.MeshLittleDamaged)
                    {
                        Debug.Log($"[ModUtils/EmulatedJunkyard]: Part can't rust and MeshLittleDamaged is false, setting condition to 1.00");

                        carProperties.Condition = 1f;
                    }
                }

                Debug.Log($"[ModUtils/EmulatedJunkyard]: Second part of per-part setup is finished. Trying to reset wheel controllers");
                mcp.ResetWheelControllers();

                Debug.Log($"[ModUtils/EmulatedJunkyard]: WCs were reset, now trying to determinate starting price - stage1");
                int allparts = 0;
                int existingparts = 0;
                mcp.CarPriceStart = 0f;

                foreach (transparents transparents in mcp.GetComponentsInChildren<transparents>())
                {
                    if (!transparents.NotImportantPart)
                    {
                        allparts++;
                    }
                    if (!transparents.NotImportantPart && transparents.HaveAttached)
                    {
                        existingparts++;
                    }
                }

                Debug.Log($"[ModUtils/EmulatedJunkyard]: Stage2 of start price, allparts: {allparts} | existingparts {existingparts}");
                P3dChangeCounter[] targetLis = mcp.GetComponentsInChildren<P3dChangeCounter>();
                foreach (P3dChangeCounter p3dChangeCounter in targetLis)
                {
                    Debug.Log($"[ModUtils/EmulatedJunkyard]: Enabling counter {p3dChangeCounter.name}, threshold is {p3dChangeCounter.Threshold} (0.1 is paint)");

                    p3dChangeCounter.enabled = true;
                    if (p3dChangeCounter.gameObject.GetComponent<CarProperties>().Paintable && p3dChangeCounter.Threshold == 0.1f)
                    {
                        p3dChangeCounter.changeDirty = true;
                        p3dChangeCounter.Color = mcp.Color;
                    }
                }
                Debug.Log($"[ModUtils/EmulatedJunkyard]: Stage3, calculating floats (paint ratios)");
                float CleanRatio = 0.1f;
                float NoRustRatio = 0.1f;
                float PaintRatio = 0.1f;
                float CleanRatioParts = 0f;
                float NoRustRatioParts = 0f;
                float PaintRatioParts = 0f;
                float PaintGoodParts = 0f;
                float RustGoodParts = 0f;
                foreach (P3dChangeCounter p3dChangeCounter2 in targetLis)
                {
                    float num = 1f - p3dChangeCounter2.Ratio;
                    Debug.Log($"[ModUtils/EmulatedJunkyard]: Ratio is {num}, paintable {p3dChangeCounter2.gameObject.GetComponent<CarProperties>().Paintable}, washable {p3dChangeCounter2.gameObject.GetComponent<CarProperties>().Washable}, threshold is {p3dChangeCounter2.Threshold}");
                    if (p3dChangeCounter2.gameObject.GetComponent<CarProperties>().Washable && p3dChangeCounter2.Threshold == 0.7f)
                    {
                        if ((double)num > 0.6)
                        {
                            CleanRatio += 1f;
                        }
                        CleanRatioParts += 1f;
                        p3dChangeCounter2.gameObject.GetComponent<CarProperties>().CleanRatio = num;
                    }
                    if (p3dChangeCounter2.gameObject.GetComponent<CarProperties>().Paintable && p3dChangeCounter2.Threshold == 0.5f)
                    {
                        NoRustRatio += num;
                        NoRustRatioParts += 1f;
                        p3dChangeCounter2.gameObject.GetComponent<CarProperties>().NoRustRatio = num;
                        if (num > 0.95f)
                        {
                            RustGoodParts += 1f;
                        }
                    }
                    if (p3dChangeCounter2.gameObject.GetComponent<CarProperties>().Paintable && p3dChangeCounter2.Threshold == 0.1f)
                    {
                        PaintRatio += num;
                        PaintRatioParts += 1f;
                        p3dChangeCounter2.gameObject.GetComponent<CarProperties>().PaintRatio = num;
                        if (num > 0.9f)
                        {
                            PaintGoodParts += 1f;
                        }
                    }
                    p3dChangeCounter2.enabled = false;
                }
                float DamagedBodyPanels = 0f;
                float AllBodyPanels = 0f;
                float AverageCondition = 0f;

                if (CleanRatioParts > 0f)
                {
                    CleanRatio /= CleanRatioParts;
                }
                else
                {
                    CleanRatio = 0.9f;
                }
                if (NoRustRatioParts > 0f)
                {
                    NoRustRatio = RustGoodParts / NoRustRatioParts;
                }
                else
                {
                    NoRustRatio = 0.9f;
                }
                if (PaintRatioParts > 0f)
                {
                    PaintRatio = PaintGoodParts / PaintRatioParts;
                }
                else
                {
                    PaintRatio = 0.9f;
                }

                Debug.Log($"[ModUtils/EmulatedJunkyard]: Stage 4, ratios were calculated. Starting per part calculation");
                foreach (CarProperties carProps in mcp.GetComponentsInChildren<CarProperties>())
                {
                    Debug.Log($"[ModUtils/EmulatedJunkyard]: Now at part {carProps.name} (SinglePart: {carProps.SinglePart})");
                    if (carProps.SinglePart)
                    {
                        float conditCount = 0f;
                        if (carProps.MeshRepairable && (mcp.Owner == "Client" || carProps.Owner != "Client"))
                        {
                            if (carProps.MeshDamaged)
                            {
                                DamagedBodyPanels += 1f;
                            }
                            AllBodyPanels += 1f;
                            if (carProps.Ruined || carProps.NoRustRatio < 0.9f)
                            {
                                conditCount += 0f;
                            }
                            else if (carProps.MeshDamaged || carProps.NoRustRatio < 0.95f)
                            {
                                conditCount += 0.1f;
                            }
                            else
                            {
                                conditCount += 1f;
                            }
                        }
                        if (!carProps.MeshRepairable && (mcp.Owner == "Client" || carProps.Owner != "Client"))
                        {
                            if (carProps.Condition < 0.4f && carProps.Condition >= 0.1f && (carProps.WornMesh || carProps.WornMaterial))
                            {
                                conditCount += 0.1f;
                            }
                            else if (carProps.Condition <= 0.1f && (carProps.RuinedMesh || carProps.RuinedMaterial || carProps.WornMesh || carProps.WornMaterial))
                            {
                                conditCount += 0f;
                            }
                            else if (carProps.PartIsOld)
                            {
                                conditCount += 0f;
                            }
                            else
                            {
                                conditCount += 1f;
                            }
                        }
                        carProps.ConditionDebug = conditCount;
                        AverageCondition += conditCount;
                    }
                }
                if (existingparts >= allparts)
                {
                    AverageCondition /= (float)existingparts;
                }
                else
                {
                    AverageCondition /= mcp.PartsCount;
                }
                mcp.CarPriceStart = mcp.CarPrice * AverageCondition * (NoRustRatio * NoRustRatio);
                mcp.CarPriceStart = mcp.CarPriceStart / 2f + mcp.CarPriceStart / 2f * PaintRatio;
                mcp.CarPriceStart = mcp.CarPriceStart / 50f * 49f + mcp.CarPriceStart / 50f * CleanRatio;
                if (AllBodyPanels > 1f)
                {
                    mcp.CarPriceStart = mcp.CarPriceStart / 3f + mcp.CarPriceStart / 3f * 2f * ((AllBodyPanels - DamagedBodyPanels) / AllBodyPanels);
                }

                Debug.Log($"[ModUtils/EmulatedJunkyard]: Value calculated was {mcp.CarPriceStart}");
                if (float.IsNaN(mcp.CarPriceStart))
                {
                    mcp.CarPriceStart = 250;
                    Debug.Log($"[ModUtils/EmulatedJunkyard]: NaN value detected, game fallbacks to 20 in this case...");
                }

                Debug.Log("[ModUtils/EmulatedJunkyard]: The spawn of the car was succesful on this case. Remember to try various times since spawns are random!");
            }
        }
    }
}
