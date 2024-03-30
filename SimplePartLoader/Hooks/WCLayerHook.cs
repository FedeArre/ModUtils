using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using NWH.WheelController3D;
using SimplePartLoader;
using UnityEngine;

[HarmonyPatch(typeof(MainCarProperties), nameof(CheckDr))]
internal class WCLayerHook
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
