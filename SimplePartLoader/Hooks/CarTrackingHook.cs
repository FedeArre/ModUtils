using HarmonyLib;
using NWH.VehiclePhysics2;
using SimplePartLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


[HarmonyPatch(typeof(InCarChecker), "OnDestroy")]
internal class CarTrackingDestroyHook
{
    static void Prefix(InCarChecker __instance)
    {
        if (!__instance.GetComponent<MainCarProperties>())
            return;

        if (ModUtils.Cars.Contains(__instance.gameObject))
        {
            ModUtils.Cars.Remove(__instance.gameObject);
        }
    }
}

[HarmonyPatch(typeof(MainCarProperties), "Start")]
internal class CarTrackingCreateHook
{
    static void Prefix(MainCarProperties __instance)
    {
        if(!ModUtils.Cars.Contains(__instance.gameObject))
        {
            ModUtils.Cars.Add(__instance.gameObject);
        }
    }
}