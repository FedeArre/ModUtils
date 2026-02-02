using HarmonyLib;
using SimplePartLoader;
using UnityEngine;

[HarmonyPatch(typeof(Partinfo), "Fall")]
internal class ExtendedCallsPartinfoFailHook
{
    static void Postfix(Partinfo __instance)
    {
        ExtendedCallsHookUtils.SyncAttachment(__instance);
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
        Debug.Log($"SyncAttachment called for component: {component?.GetType().Name}");
        Debug.Log($"Component GameObject: {component?.gameObject.name}");

        if (component == null)
            return;

        // If it has extended calls, we continue
        var extendedCalls = component.gameObject.GetComponent<PartExtendedCalls>();
        Debug.Log("EC: " + (extendedCalls != null ? "Found" : "Not Found"));
        if (extendedCalls == null)
            return;

        Debug.Log("Found PartExtendedCalls component.");

        // Check if the part is attached or not
        bool isAttached = extendedCalls.transform.parent.GetComponent<transparents>();
        Debug.Log("Is Attached: " + isAttached);
        Debug.Log(extendedCalls.transform.parent ? "Parent: " + extendedCalls.transform.parent.name : "No Parent");

        if (isAttached)
        {
            extendedCalls.NotifyAttached(component.gameObject);
        }
        else
        {
            extendedCalls.NotifyDetached(component.gameObject);
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
