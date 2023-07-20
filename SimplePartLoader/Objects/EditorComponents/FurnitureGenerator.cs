using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class FurnitureGenerator : MonoBehaviour
{
    [Header("Basic settings")]
    [Tooltip("Internal name that the furniture uses to identify itself internally")]
    public string PrefabName = "";
    [Tooltip("Display name that will be show to the player")]
    public string DisplayName = "";
    [Tooltip("Price of the furniture")]
    public float Price = 0f;

    [Header("Furniture settings")]
    [Tooltip("Type of freezing that the furniture will use")]
    public FreezeTypeEnum FreezeType = FreezeTypeEnum.NoFreeze;

    [Tooltip("If move tool is required to move the furniture")]
    public bool RequiresMoveTool;

    [Tooltip("Forces the furniture to freeze on trailers")]
    public bool FreezeOnTrailer;
    
    public enum FreezeTypeEnum
    {
        NoFreeze,
        WaitUntilStill,
        Instant
    }
}

