using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SimplePartLoader
{
    public class BuildableMaterial
    {
        public string PrefabName { get; internal set; }
        public Material UsedMaterial { get; internal set; }
        public GameObject Item { get; internal set; }
        public Vector3 Position { get; internal set; }
        public Vector3 Rotation { get; internal set; }
        internal ModInstance loadedBy { get; set; }
        
        public BuildableMaterial(string prefabName, Material usedMaterial, ModInstance modInstance, Vector3 shopPosition, Vector3 shopRotation, GameObject item)
        {
            PrefabName = prefabName;
            UsedMaterial = usedMaterial;
            Position = shopPosition;
            Rotation = shopRotation;
            loadedBy = modInstance;
            Item = item;
        }
    }
}
