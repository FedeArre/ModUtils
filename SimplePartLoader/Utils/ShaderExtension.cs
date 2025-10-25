using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SimplePartLoader
{
    public static class ShaderExtension
    {
        public static void MakeDoubleSided(this Material material)
        {
            if (material is null || material.shader?.name != "Universal Render Pipeline/Lit")
                return;

            material.SetFloat("_Cull", 0);
        }

        public static void MakeFrontSided(this Material material)
        {
            if (material is null || material.shader?.name != "Universal Render Pipeline/Lit")
                return;

            material.SetFloat("_Cull", 2);
        }
    }
}
