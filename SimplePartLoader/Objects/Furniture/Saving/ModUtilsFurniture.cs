using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SimplePartLoader.Objects.Furniture.Saving
{
    internal class ModUtilsFurniture : MonoBehaviour
    {
        public string PrefabName;
        public SimplePartLoader.Furniture furnitureRef;

        void Start()
        {
            furnitureRef = (SimplePartLoader.Furniture) FurnitureManager.Furnitures[PrefabName];
            if(furnitureRef == null)
            {
                Debug.Log("[ModUtils/Furniture/Error]: Could not find reference for furniture with prefab name " + PrefabName);
            }
        }
    }
}
