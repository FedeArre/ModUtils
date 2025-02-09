using NWH.NPhysics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SimplePartLoader.CarGen
{
    public class TrailerGenerator
    {
        internal static List<TrailerSaleItemObject> SaleItems = new List<TrailerSaleItemObject>();

        public static TrailerSaleItemObject RegisterSaleItem(string name, float price, Car trailer, Action<GameObject> onSaleItemCreation)
        {
            ICarBase baseData = (ICarBase)MainCarGenerator.AvailableBases[trailer.carGeneratorData.BaseCarToUse];

            if (baseData.VehType() != VehicleType.Trailer)
            {
                CustomLogger.AddLine("TrailerGenerator", $"{trailer.carGeneratorData.CarName} is not a trailer, sale item can not be created");
                return null;
            }

            TrailerSaleItemObject tsio = new TrailerSaleItemObject()
            {
                Name = name,
                Price = price,
                Trailer = trailer,
                OnSaleItemCreation = onSaleItemCreation
            };

            SaleItems.Add(tsio);

            return tsio;
        }

        internal static void GenerateSaleItems()
        {
            var spawnSpot = GameObject.Find("UnloadablesMain/shop/Shop/ItemSpawnOut");
            foreach (TrailerSaleItemObject tsio in SaleItems)
            {
                GameObject saleItem = GameObject.Instantiate(tsio.Trailer.carPrefab);
                tsio.SaleItem = saleItem;

                try
                {
                    SaleItem si = saleItem.AddComponent<SaleItem>();
                    si.Price = tsio.Price;
                    si.name = tsio.Name;
                    si.SpawnSpot = spawnSpot;
                    si.Item = tsio.Trailer.BuiltCarPrefab;

                    saleItem.layer = LayerMask.NameToLayer("Items");
                    saleItem.tag = "Item";

                    GameObject.DestroyImmediate(saleItem.GetComponent<MainTrailerProperties>());
                    GameObject.DestroyImmediate(saleItem.GetComponent<Rigidbody>());
                    GameObject.DestroyImmediate(saleItem.GetComponent<NRigidbody>());

                    tsio.OnSaleItemCreation?.Invoke(saleItem);
                }
                catch (Exception ex)
                {
                    CustomLogger.AddLine("TrailerGenerator", $"Error while invoking OnSaleItemCreation for {tsio.Name}");
                    CustomLogger.AddLine("TrailerGenerator", ex);
                }
            }
        }

        public class TrailerSaleItemObject
        {
            public string Name { get; set; }
            public float Price { get; set; }
            public Car Trailer { get; set; }
            public GameObject SaleItem { get; set; }
            public Action<GameObject> OnSaleItemCreation { get; set; }
        }
    }
}