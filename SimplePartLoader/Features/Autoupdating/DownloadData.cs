using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimplePartLoader.Features.Autoupdating
{
    public class DownloadData
    {
        public string Mod_Name;
        public string Mod_Url;
        public string File_Name;

        public DownloadData(string Mod_Name, string Mod_Url, string File_Name)
        {
            this.Mod_Name = Mod_Name;
            this.Mod_Url = Mod_Url;
            this.File_Name = File_Name;
        }
    }
}
