using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using SimplePartLoader;
using UnityEngine;
using static PaintIn3D.P3dSeamFixer;
using SimplePartLoader.Utils;

[HarmonyPatch(typeof(MainCarProperties), nameof(MainCarProperties.PreventChildCollisions))]
internal class FixTryHook
{
    static void Postfix(MainCarProperties __instance)
    {
        if (!__instance.transform.parent)
        {
            Functions.IgnoreCollisionsBetweenAll(__instance.GetComponentsInChildren<Collider>());
        }
    }
}
