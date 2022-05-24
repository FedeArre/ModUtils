using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SimplePartLoader
{
    internal class SaveData : MonoBehaviour
    {
        public Dictionary<string, object> Data = new Dictionary<string, object>();
        public string PartName;
    }
}
