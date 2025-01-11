using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SimplePartLoader
{
    public class SPL_Part : MonoBehaviour
    {
        // Empty - Just used to identify if a part was added by SPL!
        internal ModInstance Mod { get; set; }
    }

    public class SPL_StartOption : MonoBehaviour
    {
        // Empty - Just used to identify if a start option was added by SPL!
        internal ModInstance Mod { get; set; }
    }
}
