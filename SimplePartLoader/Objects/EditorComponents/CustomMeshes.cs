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

    public Dictionary<string, Mesh> FuelLines;
    public Dictionary<string, Mesh> BatteryWires;
    public Dictionary<string, Mesh> RadiatorUpperHoses;
    public Dictionary<string, Mesh> RadiatorLowerHoses;

    public void DoInternalConversion()
    {
        FuelLines = new Dictionary<string, Mesh>();
        BatteryWires = new Dictionary<string, Mesh>();
        RadiatorUpperHoses = new Dictionary<string, Mesh>();
        RadiatorLowerHoses = new Dictionary<string, Mesh>();

        foreach (CustomMesh cm in GetComponents<CustomMesh>())
        {
            switch (cm.Type)
            {
                case MeshType.FuelLine:
                    FuelLines[cm.CarName] = cm.Mesh;
                    break;
                case MeshType.BatteryWire:
                    BatteryWires[cm.CarName] = cm.Mesh;
                    break;
                case MeshType.RadiatorUpperHose:
                    RadiatorUpperHoses[cm.CarName] = cm.Mesh;
                    break;
                case MeshType.RadiatorLowerHose:
                    RadiatorLowerHoses[cm.CarName] = cm.Mesh;
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
}

public enum MeshType
{
    FuelLine,
    BatteryWire,
    RadiatorUpperHose,
    RadiatorLowerHose
}

