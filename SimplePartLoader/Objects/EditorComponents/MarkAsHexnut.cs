using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

internal class MarkAsHexnut : MonoBehaviour
{
    [Tooltip("Disable the renderer when not having the wrench on the hand?")]
    public bool DontDisableRenderer = false;

    [Tooltip("Used on suspension hubs, this type has to match the type on the CarProperties Type to be able to be bolted in")]
    public int Type = 0;
}
