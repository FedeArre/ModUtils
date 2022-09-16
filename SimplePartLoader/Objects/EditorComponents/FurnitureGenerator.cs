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
    [Tooltip("Set it as furniture? Makes only movable with move tool and freezes after a bit")]
    public bool EnableFurnitureBehaviour = true;
    [Tooltip("Only for parts that have furniture behaviour. Allows to move with hand")]
    public bool AllowHandPicking = false;
}

