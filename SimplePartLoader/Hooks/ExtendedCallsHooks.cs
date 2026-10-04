using HarmonyLib;
using SimplePartLoader;
using System;
using System.Collections;
using UnityEngine;

[HarmonyPatch(typeof(Partinfo), "Fall")]
internal class ExtendedCallsPartinfoFailHook
{
    static void Postfix(Partinfo __instance, ref IEnumerator __result)
    {
        if (__result != null && __instance && __instance.GetComponent<PartExtendedCalls>())
            __result = TrackAttachment(__instance, __result);
    }

    private static IEnumerator TrackAttachment(Partinfo instance, IEnumerator routine)
    {
        try
        {
            while (routine.MoveNext())
            {
                ExtendedCallsHookUtils.SyncAttachment(instance);
                yield return routine.Current;
            }

            ExtendedCallsHookUtils.SyncAttachment(instance);
        }
        finally
        {
            (routine as IDisposable)?.Dispose();
        }
    }
}

[HarmonyPatch(typeof(Pickup), "BRAKE2")]
internal class ExtendedCallsPickupBrake2Hook
{
    static void Postfix(Pickup __instance)
    {
        ExtendedCallsHookUtils.SyncAttachment(__instance);
    }
}

[HarmonyPatch(typeof(Pickup), "FitInPlace", new[] { typeof(GameObject) })]
internal class ExtendedCallsPickupFitInPlaceHook
{
    static void Postfix(Pickup __instance)
    {
        ExtendedCallsHookUtils.SyncAttachment(__instance);
    }
}

[HarmonyPatch(typeof(Pickup), "OnMouseDowns")]
internal class ExtendedCallsPickupOnMouseDowns0Hook
{
    static void Postfix(Pickup __instance)
    {
        ExtendedCallsHookUtils.SyncAttachment(__instance);
    }
}

[HarmonyPatch(typeof(Pickup), "RemovingContinue")]
internal class ExtendedCallsPickupRemovingContinueHook
{
    static void Postfix(Pickup __instance)
    {
        ExtendedCallsHookUtils.SyncAttachment(__instance);
    }
}

[HarmonyPatch(typeof(PickupItems), "TakeInHand")]
internal class ExtendedCallsPickupItemsTakenInHandHook
{
    static void Postfix(PickupItems __instance)
    {
        ExtendedCallsHookUtils.SyncAttachment(__instance);
    }
}

[HarmonyPatch(typeof(PickupWindow), "Attach", new[] { typeof(GameObject) })]
internal class ExtendedCallsPickupWindowAttachHook
{
    static void Postfix(PickupWindow __instance)
    {
        ExtendedCallsHookUtils.SyncAttachment(__instance);
    }
}

[HarmonyPatch(typeof(RemoveWindow), "RemovingContinue")]
internal class ExtendedCallsRemoveWindowRemovingContinueHook
{
    static void Postfix(RemoveWindow __instance)
    {
        ExtendedCallsHookUtils.SyncAttachment(__instance);
    }
}

internal static class ExtendedCallsHookUtils
{
    internal static void SyncAttachment(Component component)
    {
        if (component == null)
            return;

        // If it has extended calls, we continue
        var extendedCalls = component.gameObject.GetComponent<PartExtendedCalls>();
        if (extendedCalls == null)
            return;

        // Check if the part is attached or not
        Transform parent = extendedCalls.transform.parent;
        bool isAttached = parent && parent.GetComponent<transparents>();

        try
        {
            if (isAttached)
            {
                extendedCalls.NotifyAttached(component.gameObject);
            }
            else
            {
                extendedCalls.NotifyDetached(component.gameObject);
            }
        }
        catch (Exception ex)
        {
            CustomLogger.AddLine("ExtendedCalls", ex);
        }
    }

    private static Transform FindChildByName(Transform parent, string childName)
    {
        if (parent == null || string.IsNullOrEmpty(childName))
            return null;

        foreach (Transform child in parent.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == childName)
                return child;
        }

        return null;
    }
}
