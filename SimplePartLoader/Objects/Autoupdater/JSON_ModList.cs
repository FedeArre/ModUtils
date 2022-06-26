using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autoupdater
{
    internal class JSON_ModList
    {
        public List<JSON_Mod> mods { get; }

        public JSON_ModList()
        {
            mods = new List<JSON_Mod>();
        }
    }
}
