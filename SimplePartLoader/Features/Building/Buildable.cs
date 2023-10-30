using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SimplePartLoader
{
    public class Buildable
    {
        public GameObject Prefab { get; internal set; }
        public string PrefabName { get; internal set; }
        public BuildableType Type { get; internal set; }
        internal ModInstance loadedBy { get; set; }

        internal Buildable(string prefabName, GameObject prefab, BuildableType type)
        {
            PrefabName = prefabName;
            Prefab = prefab;
            Type = type;
        }
    }

    // Must be the same order as the editor component!
    public enum BuildableType
    {
        ROOF,
        WALL,
        DOOR
    }
}
