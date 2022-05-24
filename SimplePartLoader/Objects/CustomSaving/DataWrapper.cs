using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimplePartLoader
{
    internal class DataWrapper
    {
        public List<SavedData> Data = new List<SavedData>();
    }

    internal class SavedData
    {
        public string PrefabName { get; set; }
        public int ObjectNumber { get; set; }
        public Dictionary<string, object> Data { get; set; }
    }
}
