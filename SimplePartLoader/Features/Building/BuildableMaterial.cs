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
        internal ModInstance loadedBy { get; set; }

        public BuildableMaterial(string prefabName, Material usedMaterial, ModInstance modInstance)
        {
            PrefabName = prefabName;
            UsedMaterial = usedMaterial;
            loadedBy = modInstance;
        }
    }
}
