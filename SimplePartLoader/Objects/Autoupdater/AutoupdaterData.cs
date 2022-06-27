using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autoupdater
{
    public abstract class AutoupdaterData
    {
        public abstract string ModName { get; }
        public abstract string ModId { get; }
        public abstract string ModVersion { get; }
    }
}
