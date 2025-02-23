using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[DisallowMultipleComponent]
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

    public Dictionary<string, CustomMesh> FuelLines;
    public Dictionary<string, CustomMesh> BatteryWires;
    public Dictionary<string, CustomMesh> RadiatorUpperHoses;
    public Dictionary<string, CustomMesh> RadiatorLowerHoses;

    public void DoInternalConversion()
    {
        FuelLines = new Dictionary<string, CustomMesh>();
        BatteryWires = new Dictionary<string, CustomMesh>();
        RadiatorUpperHoses = new Dictionary<string, CustomMesh>();
        RadiatorLowerHoses = new Dictionary<string, CustomMesh>();

        foreach (CustomMesh cm in GetComponents<CustomMesh>())
        {
            switch (cm.Type)
            {
                case MeshType.FuelLine:
                    FuelLines[cm.CarName] = cm;
                    break;
                case MeshType.BatteryWire:
                    BatteryWires[cm.CarName] = cm;
                    break;
                case MeshType.RadiatorUpperHose:
                    RadiatorUpperHoses[cm.CarName] = cm;
                    break;
                case MeshType.RadiatorLowerHose:
                    RadiatorLowerHoses[cm.CarName] = cm;
                    break;
            }
        }
    }
}

public class CustomMesh : MonoBehaviour
{
    public string CarName;
    public MeshType Type = MeshType.FuelLine;
    public Mesh Mesh = null;
    public Material[] Materials = null;
}

public enum MeshType
{
    FuelLine,
    BatteryWire,
    RadiatorUpperHose,
    RadiatorLowerHose
}

