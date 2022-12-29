using SimplePartLoader.Objects.Furniture.Saving;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SimplePartLoader.Objects.Furniture
{
    internal class CustomFurnitureSaleItem : MonoBehaviour
    {
        public GameObject Prefab;
        public float Price;
        public Transform SpawnSpot;
        public string Name;
        public string Tip;

        public void Buy()
        {
            if(tools.money >= Price)
            {
                tools.money -= Price;
                ModUtils.PlayCashSound();
                
                GameObject obj = GameObject.Instantiate(Prefab, SpawnSpot.position, SpawnSpot.rotation);
                obj.name = Prefab.name;
                obj.GetComponent<ModUtilsFurniture>().OnBuy();
            }
        }
    }
}
