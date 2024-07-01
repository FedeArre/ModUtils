using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class BuildableGenerator : MonoBehaviour
{
    [Header("Basic settings")]
    public string PrefabName = "";
    public BuildType Type = BuildType.Roof;

    [Header("Door settings")]
    public Transform OpenPosition = null;
    public Transform ClosedPosition = null;
}

public enum BuildType
{
    Roof,
    Wall,
    Door
}