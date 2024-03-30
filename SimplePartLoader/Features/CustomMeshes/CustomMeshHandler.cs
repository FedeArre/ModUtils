using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimplePartLoader
{
    internal class CustomMeshHandler
    {
        internal static List<CustomMeshes> Meshes = new List<CustomMeshes>();

        public static bool IsEngineNameUsed(CustomMeshes cm)
        {
            return GetCustomMeshByEngineName(cm.EngineName) != null;
        }

        public static CustomMeshes GetCustomMeshByEngineName(string name)
        {
            foreach (CustomMeshes customMeshes in Meshes)
            {
                if (customMeshes.EngineName == name)
                {
                    return customMeshes;
                }
            }

            return null;
        }
    }
}
