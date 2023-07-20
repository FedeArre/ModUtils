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
        private bool UseMoveTool;
        private FurnitureGenerator.FreezeTypeEnum Type;
        private bool AttachOnTrailer;
        private string FurnitureTip;
        
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
        
        public bool MoveTool
        {
            get { return UseMoveTool; }
            set { UseMoveTool = value; }
        }

        public bool TrailerAttaching
        {
            get { return AttachOnTrailer; }
            set { AttachOnTrailer = value; }
        }
        public FurnitureGenerator.FreezeTypeEnum FreezeType
        {
            get { return Type; }
            set { Type = value; }
        }

        public string Tip
        {
            get { return FurnitureTip; }
            set { FurnitureTip = value; }
        }
        public Furniture(GameObject go, FurnitureGenerator fg)
        {
            ObjectPrefab = go;

            FreezeType = fg.FreezeType;
            MoveTool = fg.RequiresMoveTool;
            TrailerAttaching = fg.FreezeOnTrailer;

            FurniturePrice = fg.Price;
            Name = fg.DisplayName;
            PrefabName = fg.PrefabName;

            ObjectPrefab.AddComponent<ModUtilsFurniture>().PrefabName = PrefabName;
            ObjectPrefab.AddComponent<Rigidbody>().isKinematic = true;
            ObjectPrefab.layer = LayerMask.NameToLayer("Items");

            GameObject.Destroy(ObjectPrefab.GetComponent<FurnitureGenerator>());

            if(MoveTool)
                ObjectPrefab.name = "MODUTILS_FURNITURE_F" + PrefabName;
            else
                ObjectPrefab.name = "MODUTILS_FURNITURE_C" + PrefabName;

        }

        public SaleFurniture CreateSaleItem(Vector3 position, Vector3 rotation, SpawnSpot spot = null)
        {
            SaleFurniture sf = new SaleFurniture();
            sf.Furniture = this;
            sf.Pos = position;
            sf.Rot = rotation;
            sf.Spawn = spot;
            
            FurnitureManager.SaleItems.Add(sf);

            return sf;
        }
    }
}
