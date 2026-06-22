using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using NWH.WheelController3D;
using SimplePartLoader;
using SimplePartLoader.Utils;
using UnityEngine;

[HarmonyPatch(typeof(MainCarProperties), nameof(CheckDr))]
internal class WCLayerHook
{
    static void Postfix(MainCarProperties __instance)
    {
        if (!__instance.transform.parent)
        {
            Functions.IgnoreCollisionsBetweenAll(__instance.GetComponentsInChildren<Collider>());
        }
    }
}
