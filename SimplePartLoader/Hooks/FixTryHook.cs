using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using SimplePartLoader;
using UnityEngine;
using static PaintIn3D.P3dSeamFixer;

[HarmonyPatch(typeof(MainCarProperties), nameof(MainCarProperties.PreventChildCollisions))]
internal class FixTryHook
{
    static void Postfix(MainCarProperties __instance)
    {
        if (!__instance.transform.parent)
        {
            Collider[] componentsInChildren = __instance.GetComponentsInChildren<Collider>();
            Collider[] array = componentsInChildren;
            for (int i = 0; i < array.Length; i++)
            {
                for (int j = i + 1; j < array.Length; j++)
                {
                    if (!(array[i] == array[j]))
                    {
                        Physics.IgnoreCollision(array[i], array[j], true);
                    }
                }
            }
        }
    }
}
