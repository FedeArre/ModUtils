using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using SimplePartLoader;
using UnityEngine;

[HarmonyPatch(typeof(CarProperties), nameof(CarProperties.SetMesh))]
internal class CustomEngineHook
{
    static void Postfix(CarProperties __instance)
    {
        if (CustomMeshHandler.Meshes.Count == 0) return;
        if (!__instance.PrefabName.Contains("UpperHose") && !__instance.PrefabName.Contains("LowerHose") && __instance.PrefabName != "FuelLine" && __instance.PrefabName != "WiresMain06") return;

        Transform car = __instance.transform.root;
        if (car.tag != "Vehicle") return;

        Transform engine = car.transform.Find("EngineTranny/CylinderBlock/CylinderBlock");
        if (engine == null) return;
        
        CustomEngine ce = engine.GetComponent<CustomEngine>();
        if(ce == null) return;

        // We know is a custom engine by now, so we need to look up that mesh
        CustomMeshes cm = CustomMeshHandler.GetCustomMeshByEngineName(ce.Name);
        if(cm == null)
        {
            CustomLogger.AddLine("CustomMeshes", $"Error in setup, CustomEngine {ce.Name} found but CustomMesh was not found for it!");
            return;
        }

        string carName = car.GetComponent<MainCarProperties>().CarName;

        if(__instance.PrefabName == "FuelLine") // Fuel lines
        {
            bool flag = false;

            if(cm.FuelLines.Count != 0)
            {
                foreach (var pair in cm.FuelLines)
                {
                    if (pair.Key == carName)
                    {
                        __instance.gameObject.GetComponent<MeshFilter>().mesh = pair.Value.Mesh;
                        __instance.gameObject.GetComponent<MeshCollider>().sharedMesh = pair.Value.Mesh;

                        flag = true;

                        if (pair.Value.Materials != null)
                        {
                            __instance.gameObject.GetComponent<MeshRenderer>().materials = pair.Value.Materials;
                        }
                        break;
                    }
                }
            }

            if(!flag && cm.FuelLineFallbackMesh)
            {
                __instance.gameObject.GetComponent<MeshFilter>().mesh = cm.FuelLineFallbackMesh;
                __instance.gameObject.GetComponent<MeshCollider>().sharedMesh = cm.FuelLineFallbackMesh;
            }
        }
        else if (__instance.PrefabName == "WiresMain06") // Battery Wires
        {
            bool flag = false;
            if (cm.BatteryWires.Count != 0)
            {
                foreach (var pair in cm.BatteryWires)
                {
                    if (pair.Key == carName)
                    {
                        __instance.gameObject.GetComponent<MeshFilter>().mesh = pair.Value.Mesh;
                        __instance.gameObject.GetComponent<MeshCollider>().sharedMesh = pair.Value.Mesh;
                        flag = true;

                        if (pair.Value.Materials != null)
                        {
                            __instance.gameObject.GetComponent<MeshRenderer>().materials = pair.Value.Materials;
                        }
                        break;
                    }
                }

            }

            if (!flag && cm.BatteryWireFallbackMesh)
            {
                __instance.gameObject.GetComponent<MeshFilter>().mesh = cm.BatteryWireFallbackMesh;
                __instance.gameObject.GetComponent<MeshCollider>().sharedMesh = cm.BatteryWireFallbackMesh;
            }
        }
        else if (__instance.PrefabName.Contains("LowerHose")) // Radiator lower hose
        {
            bool flag = false;
            if (cm.RadiatorLowerHoses.Count != 0)
            {
                foreach (var pair in cm.RadiatorLowerHoses)
                {
                    if (pair.Key == carName)
                    {
                        __instance.gameObject.GetComponent<MeshFilter>().mesh = pair.Value.Mesh;
                        __instance.gameObject.GetComponent<MeshCollider>().sharedMesh = pair.Value.Mesh;
                        flag = true;

                        if (pair.Value.Materials != null)
                        {
                            __instance.gameObject.GetComponent<MeshRenderer>().materials = pair.Value.Materials;
                        }
                        break;
                    }
                }
            }

            if (!flag && cm.LowerHoseFallbackMesh)
            {
                __instance.gameObject.GetComponent<MeshFilter>().mesh = cm.LowerHoseFallbackMesh;
                __instance.gameObject.GetComponent<MeshCollider>().sharedMesh = cm.LowerHoseFallbackMesh;
            }
        }
        else if (__instance.PrefabName.Contains("UpperHose")) // Radiator upper hose
        {
            bool flag = false;

            if(cm.RadiatorUpperHoses.Count != 0)
            {
                foreach (var pair in cm.RadiatorUpperHoses)
                {
                    if (pair.Key == carName)
                    {
                        __instance.gameObject.GetComponent<MeshFilter>().mesh = pair.Value.Mesh;
                        __instance.gameObject.GetComponent<MeshCollider>().sharedMesh = pair.Value.Mesh;
                        flag = true;

                        if(pair.Value.Materials != null)
                        {
                            __instance.gameObject.GetComponent<MeshRenderer>().materials = pair.Value.Materials;
                        }
                        break;
                    }
                }
            }

            if (!flag && cm.UpperHoseFallbackMesh)
            {
                __instance.gameObject.GetComponent<MeshFilter>().mesh = cm.UpperHoseFallbackMesh;
                __instance.gameObject.GetComponent<MeshCollider>().sharedMesh = cm.UpperHoseFallbackMesh;
            }
        }
    }
}
