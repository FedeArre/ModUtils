using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

internal class CustomMeshes : MonoBehaviour
{
    [Header("Basic settings")]
    [Tooltip("The engine name")]
    public string EngineName = "";

    [Header("Fallbacks")]
    public Mesh FuelLineFallbackMesh = null;
    public Mesh BatteryWireFallbackMesh = null;
    public Mesh UpperHoseFallbackMesh = null;
    public Mesh LowerHoseFallbackMesh = null;

    [Header("Meshes")]
    public StringMeshPair[] FuelLines;
    public StringMeshPair[] BatteryWires;
    public StringMeshPair[] RadiatorUpperHoses;
    public StringMeshPair[] RadiatorLowerHoses;
}

[Serializable]
public struct StringMeshPair
{
    public string CarName;
    public Mesh Mesh;
}
