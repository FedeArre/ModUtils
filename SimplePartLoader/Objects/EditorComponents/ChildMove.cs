using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

internal class ChildMove : MonoBehaviour
{
    [Header("Basic settings")]
    public string ChildName = "";
    public bool StartsWith = false;
    public bool EndsWith = false;

    [Header("Position")]
    public bool Move = true;
    public Vector3 NewPosition = Vector3.zero;

    [Header("Rotation")]
    public bool Rotate = false;
    public Vector3 NewRotation = Vector3.zero;
}
