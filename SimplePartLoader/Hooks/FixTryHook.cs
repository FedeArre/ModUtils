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
    public static Func<bool> ExternalCollisionSetup = null;

    static void Postfix(MainCarProperties __instance)
    {
        if (ExternalCollisionSetup != null && ExternalCollisionSetup())
            return;
        if (!__instance.transform.parent)
        {
            Functions.IgnoreCollisionsBetweenAll(__instance.GetComponentsInChildren<Collider>());
        }
    }
}
