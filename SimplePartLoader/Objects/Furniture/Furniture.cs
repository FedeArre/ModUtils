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
        private string InternalName;
        private string DisplayName;
        private GameObject ObjectPrefab;

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

        public Furniture(GameObject go)
        {
            ObjectPrefab = go;
        }
    }
}
