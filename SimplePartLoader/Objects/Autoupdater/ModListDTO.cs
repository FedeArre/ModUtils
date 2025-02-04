using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Autoupdater.Objects
{
    internal class ModListDTO
    {
        public List<ModDTO> mods { get; set; }
        public string GameVersion { get; set; }
        public int BuildId { get; set; }
        public ulong SteamId { get; set; }

        public ModListDTO(int buildId)
        {
            mods = new List<ModDTO>();
            GameVersion = Application.version;
            BuildId = buildId;
            SteamId = 0;
        }
    }
}
