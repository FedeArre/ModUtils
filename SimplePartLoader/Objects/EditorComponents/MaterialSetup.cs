using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

internal class MaterialSetup : MonoBehaviour
{
    [Header("Painting system")]
    public PaintTypes SupportType = PaintTypes.FullPaintingSupport;

    [Header("Game materials")]
    public bool EnableChromeStationSupport = true;
    public bool SetPartToBlackMaterial = false;
}

public enum PaintTypes
{
    FullPaintingSupport = 1,
    OnlyPaint,
    OnlyPaintAndRust,
    OnlyDirt,
    OnlyPaintAndDirt,
    DontAdd
}