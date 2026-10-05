using HarmonyLib;
using SimplePartLoader;
using System;
using System.Collections;
using UnityEngine;

[HarmonyPatch(typeof(OpenDoor), "LateClick")]
internal class OpenDoorLateClickHook
{
    static void Postfix(OpenDoor __instance, ref IEnumerator __result)
    {
        OpenDoorSettings settings = __instance.GetComponent<OpenDoorSettings>();
        if (settings && settings.IsValid && __result != null)
            __result = ApplyAfterClick(__instance, settings, __result);
    }

    private static IEnumerator ApplyAfterClick(OpenDoor door, OpenDoorSettings settings, IEnumerator routine)
    {
        try
        {
            while (routine.MoveNext())
                yield return routine.Current;

            if (door && settings && settings.IsValid && door.doorOpened)
                OpenDoorHookUtils.ApplyHinge(door, settings);
        }
        finally
        {
            (routine as IDisposable)?.Dispose();
        }
    }
}

[HarmonyPatch(typeof(OpenDoor), "waitsec")]
internal class OpenDoorWaitHook
{
    static bool Prefix(OpenDoor __instance, ref IEnumerator __result)
    {
        OpenDoorSettings settings = __instance.GetComponent<OpenDoorSettings>();
        if (!settings || !settings.IsValid)
            return true;

        __result = WaitForCustomDoor(__instance, settings);
        return false;
    }

    private static IEnumerator WaitForCustomDoor(OpenDoor door, OpenDoorSettings settings)
    {
        HingeJoint hinge = door.GetComponent<HingeJoint>();
        if (hinge)
        {
            yield return new WaitForSeconds(1f);
            hinge = door.GetComponent<HingeJoint>();
            if (hinge)
            {
                hinge.useSpring = false;
                if (hinge.angle <= 1f)
                {
                    door.doorOpened = false;
                    UnityEngine.Object.Destroy(hinge);
                    UnityEngine.Object.Destroy(door.GetComponent<Rigidbody>());
                    door.transform.position = door.transform.parent.position;
                    door.transform.rotation = door.transform.parent.rotation;
                }
                else if (door.doorOpened && settings && settings.IsValid && hinge.angle >= Mathf.Max(1f, settings.Angle - 10f))
                {
                    FixedJoint fixedJoint = door.GetComponent<FixedJoint>();
                    if (!fixedJoint)
                        fixedJoint = door.gameObject.AddComponent<FixedJoint>();

                    fixedJoint.connectedBody = door.transform.root.GetComponent<Rigidbody>();
                    fixedJoint.breakForce = 25000f;
                    hinge.breakForce = 60000f;
                    if (door.GetComponent<Partinfo>().Trunk)
                    {
                        fixedJoint.breakForce = 10000f;
                        hinge.breakForce = 30000f;
                    }
                }
            }
        }

        door.isRunning = false;
    }
}

[HarmonyPatch(typeof(OpenDoor), nameof(OpenDoor.BrakeOpen))]
internal class OpenDoorBrakeOpenHook
{
    static void Postfix(OpenDoor __instance)
    {
        OpenDoorHookUtils.ApplyIfCustomized(__instance);
    }
}

[HarmonyPatch(typeof(OpenDoor), nameof(OpenDoor.installed))]
internal class OpenDoorInstalledHook
{
    static void Postfix(OpenDoor __instance)
    {
        OpenDoorHookUtils.ApplyIfCustomized(__instance);
    }
}

[HarmonyPatch(typeof(OpenDoor), nameof(OpenDoor.MPdoorOperation))]
internal class OpenDoorMultiplayerHook
{
    static void Postfix(OpenDoor __instance, bool open)
    {
        if (open || !__instance.transform.parent || !__instance.transform.parent.GetComponent<transparents>())
            return;

        OpenDoorSettings settings = __instance.GetComponent<OpenDoorSettings>();
        if (settings && settings.IsValid)
            __instance.transform.rotation = __instance.transform.parent.rotation * Quaternion.AngleAxis(settings.Angle, settings.Axis.normalized);
    }
}

internal static class OpenDoorHookUtils
{
    internal static void ApplyIfCustomized(OpenDoor door)
    {
        OpenDoorSettings settings = door.GetComponent<OpenDoorSettings>();
        if (settings && settings.IsValid)
            ApplyHinge(door, settings);
    }

    internal static void ApplyHinge(OpenDoor door, OpenDoorSettings settings)
    {
        HingeJoint hinge = door.GetComponent<HingeJoint>();
        if (!hinge)
            return;

        hinge.axis = settings.Axis.normalized;
        JointLimits limits = hinge.limits;
        limits.min = 0f;
        limits.max = settings.Angle;
        hinge.limits = limits;
        hinge.useLimits = true;

        if (hinge.useSpring)
        {
            JointSpring spring = hinge.spring;
            spring.targetPosition = settings.Angle;
            hinge.spring = spring;
        }
    }
}
