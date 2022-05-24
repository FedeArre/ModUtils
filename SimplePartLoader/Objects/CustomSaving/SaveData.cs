using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SimplePartLoader
{
    public class SaveData : MonoBehaviour
    {
        public Dictionary<string, object> Data = new Dictionary<string, object>();
        internal string PartName;

        void Start()
        {
            CarProperties carProps = this.GetComponent<CarProperties>();
            if(!carProps)
            {
                Debug.Log("[SPL]: Invalid save data attachment! Object: " + this);
                GameObject.Destroy(this);
                return;
            }

            PartName = carProps.PrefabName;

        }
    }
}
