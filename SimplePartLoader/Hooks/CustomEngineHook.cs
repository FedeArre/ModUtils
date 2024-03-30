﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using SimplePartLoader;
using UnityEngine;
using static PaintIn3D.P3dSeamFixer;

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
            foreach(var pair in cm.FuelLines)
            {
                if(pair.CarName == carName)
                {
                    __instance.gameObject.GetComponent<MeshFilter>().mesh = pair.Mesh;
                    __instance.gameObject.GetComponent<MeshCollider>().sharedMesh = pair.Mesh;
                    flag = true;
                    break;
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
            foreach (var pair in cm.BatteryWires)
            {
                if (pair.CarName == carName)
                {
                    __instance.gameObject.GetComponent<MeshFilter>().mesh = pair.Mesh;
                    __instance.gameObject.GetComponent<MeshCollider>().sharedMesh = pair.Mesh;
                    flag = true;
                    break;
                }
            }

            if (!flag && cm.BatteryWireFallbackMesh)
            {
                __instance.gameObject.GetComponent<MeshFilter>().mesh = cm.FuelLineFallbackMesh;
                __instance.gameObject.GetComponent<MeshCollider>().sharedMesh = cm.FuelLineFallbackMesh;
            }
        }
        else if (__instance.PrefabName.Contains("LowerHose")) // Radiator lower hose
        {
            bool flag = false;
            foreach (var pair in cm.RadiatorLowerHoses)
            {
                if (pair.CarName == carName)
                {
                    __instance.gameObject.GetComponent<MeshFilter>().mesh = pair.Mesh;
                    __instance.gameObject.GetComponent<MeshCollider>().sharedMesh = pair.Mesh;
                    flag = true;
                    break;
                }
            }

            if (!flag && cm.LowerHoseFallbackMesh)
            {
                __instance.gameObject.GetComponent<MeshFilter>().mesh = cm.FuelLineFallbackMesh;
                __instance.gameObject.GetComponent<MeshCollider>().sharedMesh = cm.FuelLineFallbackMesh;
            }
        }
        else if (__instance.PrefabName.Contains("UpperHose")) // Radiator upper hose
        {
            bool flag = false;
            foreach (var pair in cm.RadiatorUpperHoses)
            {
                if (pair.CarName == carName)
                {
                    __instance.gameObject.GetComponent<MeshFilter>().mesh = pair.Mesh;
                    __instance.gameObject.GetComponent<MeshCollider>().sharedMesh = pair.Mesh;
                    flag = true;
                    break;
                }
            }

            if (!flag && cm.UpperHoseFallbackMesh)
            {
                __instance.gameObject.GetComponent<MeshFilter>().mesh = cm.FuelLineFallbackMesh;
                __instance.gameObject.GetComponent<MeshCollider>().sharedMesh = cm.FuelLineFallbackMesh;
            }
        }
    }
}
