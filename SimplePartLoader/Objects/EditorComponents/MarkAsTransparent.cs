using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

internal class MarkAsTransparent : MonoBehaviour
{
    [Tooltip("Used for differenciating same named transparents on the same part")]
    public int SavePosition = 0;

    [Tooltip("Type of the transparent, this type has to match the type on the CarProperties Type (of the object that gets attached) to be able to be bolted in")]
    public int Type = 0;
}
