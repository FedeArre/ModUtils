using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class CarGenerator : MonoBehaviour
{
    public string CarName = "";
    public int CarPrice = 0;
    public CarBase BaseCarToUse = CarBase.Chad;

    [Header("Custom meshes - Fuel line")]
    public Mesh Inline4FuelLine;
    public Mesh V8EngineFuelLine;
    public Mesh Inline6FuelLine;
    public Mesh Inline6DieselFuelLine;

    [Header("Custom meshes - Brake line")]
    public bool EnableCustomBrakeLine;
    public Mesh BrakeLineMesh;
    public Vector3 FrontLeftPivot;
    public Vector3 FrontRightPivot;
    public Vector3 RearLeftPivot;
    public Vector3 RearRightPivot;

    [Header("Custom meshes - Battery wires")]
    public Mesh Inline4BatteryWires;
    public Mesh V8EngineBatteryWires;
    public Mesh Inline6BatteryWires;
    public Mesh Inline6DieselBatteryWires;

    [Header("Fixes & settings (Don't touch if you don't know what are you doing!)")]
    public bool EnableAttachFix = true;
    public bool EnableAutomaticPartCount = true;
    public bool TransparentReferenceUpdate = true;
    public bool BoneTargetTransformFix = true;
    public bool FixLights = true;
    public bool DisableModUtilsTemplateSetup = false;
    public bool EnableAutomaticPainting = true;
    public bool EnableAutomaticFluidSetup = true;
    public bool EnableAWD = false;
    public bool Disable_NWH_Rebuild = false;

    [Header("Spawn settings")]
    public bool SpawnOnJobs = false;
    public bool SpawnRuined = false;

    [Header("Transparent remover")]
    public bool RemoveOriginalTransparents = false;
    public bool DontRemoveBatteryWires = true;
    public bool DontRemoveFuelLine = true;
    public bool DontRemoveBrakeLine = true;
    public List<string> TransparentExceptions = new List<string>();
}

public enum CarBase
{
    Chad,
    LAD,
    Wolf
}