﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabGenerator : MonoBehaviour
{
    [Header("Basic setup")]
    [Tooltip("The prefab name of your object - Has to be unique!")]
    public string PrefabName;
    [Tooltip("The name of the object that will be copied into your prefab")]
    public string CopiesFrom;

    [Header("Mesh change")]
    [Tooltip("If you are using a new mesh for the prefab")]
    public bool EnableMeshChange;
    [Tooltip("If the part should use the materials from the prefab, from the dummy original (the original part you are copying) or automatic setup for the painting system")]
    public MaterialSettingTypes UseMaterialsFrom = MaterialSettingTypes.Prefab;
    
    [Header("Car Properties")]
    [Tooltip("The part name that will be shown in-game")]
    public string PartName;

    [Header("Part info")]
    [Tooltip("The new price for your part. If you make it negative it will sum up to the part value, if is positive it will be set and the value will not change if set to 0")]
    public int NewPrice = 0;
    [Tooltip("Enables the part spawn on the junkyard")]
    public bool EnablePartOnJunkyard = false;
    [Tooltip("Enables the part appearing on the catalog")]
    public bool EnablePartOnCatalog = true;
    [Tooltip("The photo that will be shown on the catalog. At least 500x500")]
    public Texture2D CatalogImage;
    [Tooltip("Renamed prefab name for this object. Leave empty if you are not changing it")]
    public string RenamedPrefab;
    [Header("Other features")]
    [Tooltip("Enables the SimplePartLoader saving feature")]
    public bool SavingFeatureEnabled;
    [Tooltip("Change the attachment type of the part")]
    public AttachmentTypes AttachmentType = AttachmentTypes.Default;
    [Tooltip("Enables part to get chromed")]
    public bool EnableChromed = false;

    public enum AttachmentTypes
    {
        Default,
        Prytool,
        Hand,
        UseMarkedBolts,
        ForceUseMarkedBolts
    }

    public enum MaterialSettingTypes
    {
        Prefab,
        DummyOriginal,
        PaintingSetup
    }
}
