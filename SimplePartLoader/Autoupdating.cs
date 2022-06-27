using Autoupdater;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimplePartLoader
{
    public class Autoupdating : AutoupdaterData
    {
        public override string ModName => "ModUtils";
        public override string ModId =>"ModUtils";
        public override string ModVersion => "v1.0.0";
    }
}
