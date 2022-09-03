using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

internal class MarkAsBoltnut : MonoBehaviour
{
    public string OtherObjectName = "";
    
    public bool MatchTypeToBolt = false;
    public bool AffectsGrandParent1 = false;
    public bool AffectsGrandParent2 = false;
    public bool AffectsGrandParent3 = false;
    public bool DisallowDistantBreaking = false;
    public bool NotImportant = false;
    public bool ChildrenHaveToBeRemoved = false;
    public bool DontDisableRenderer = false;
}

