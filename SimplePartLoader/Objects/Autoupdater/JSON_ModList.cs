using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Autoupdater.Objects
{
    internal class JSON_ModList
    {
        public List<JSON_Mod> mods { get; }
        public string GameVersion { get; set; }

        public JSON_ModList()
        {
            mods = new List<JSON_Mod>();
            GameVersion = Application.version;
        }
    }
}
