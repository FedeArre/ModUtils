using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

internal class MarkAsBoltnut : MonoBehaviour
{
    public string OtherObjectName;
    
    public bool MatchTypeToBolt;
    public bool AffectsGrandParent1;
    public bool AffectsGrandParent2;
    public bool AffectsGrandParent3;
    public bool DisallowDistantBreaking;
    public bool NotImportant;
    public bool ChildrenHaveToBeRemoved;
    public bool DontDisableRenderer;
}

