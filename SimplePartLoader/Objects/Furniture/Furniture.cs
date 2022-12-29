using SimplePartLoader.Objects.Furniture.Saving;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SimplePartLoader
{
    public class Furniture
    {
        private GameObject ObjectPrefab;

        // Furniture settings
        private string InternalName;
        private string DisplayName;
        private float FurniturePrice;
        private bool FurnitureBehaviour;
        private bool UsingHandPickup;

        // Internal stuff
        private ModInstance Owner;
        
        public string PrefabName
        {
            get { return InternalName; }
            internal set { InternalName = value; }
        }

        public string Name
        {
            get { return DisplayName; }
            set { DisplayName = value; }
        }

        public GameObject Prefab
        {
            get { return ObjectPrefab; }
            internal set { ObjectPrefab = value; }
        }

        public ModInstance Mod
        {
            get { return Owner; }
            internal set { Owner = value; }
        }
        
        public float Price
        {
            get { return FurniturePrice; }
            set { FurniturePrice = value; }
        }
        
        public bool BehaveAsFurniture
        {
            get { return FurnitureBehaviour; }
            set { FurnitureBehaviour = value; }
        }

        public bool HandPickup
        {
            get { return UsingHandPickup; }
            set { UsingHandPickup = value; }
        }
        
        public Furniture(GameObject go, FurnitureGenerator fg)
        {
            ObjectPrefab = go;

            FurnitureBehaviour = fg.EnableFurnitureBehaviour;
            UsingHandPickup = fg.AllowHandPicking;
            FurniturePrice = fg.Price;
            Name = fg.DisplayName;
            PrefabName = fg.PrefabName;

            ObjectPrefab.AddComponent<ModUtilsFurniture>().PrefabName = PrefabName;
            ObjectPrefab.AddComponent<Rigidbody>().isKinematic = true;
            ObjectPrefab.layer = LayerMask.NameToLayer("Items");

            GameObject.Destroy(ObjectPrefab.GetComponent<FurnitureGenerator>());

            if(UsingHandPickup)
                ObjectPrefab.name = "MODUTILS_FURNITURE_H_" + PrefabName;
            else if (FurnitureBehaviour)
                ObjectPrefab.name = "MODUTILS_FURNITURE_F" + PrefabName;
            else
                ObjectPrefab.name = "MODUTILS_FURNITURE_C" + PrefabName;

        }

        public SaleFurniture CreateSaleItem(Vector3 position, Vector3 rotation)
        {
            SaleFurniture sf = new SaleFurniture();
            sf.Furniture = this;
            sf.Pos = position;
            sf.Rot = rotation;

            FurnitureManager.SaleItems.Add(sf);

            return sf;
        }
    }
}
