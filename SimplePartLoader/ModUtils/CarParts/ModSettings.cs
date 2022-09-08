using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimplePartLoader
{
    public class ModSettings
    {
        private ModInstance modInstance;
        private bool DeveloperLog = false;
        private bool Immediate = false;
        private bool Cloning = true;
        
        public ModInstance Mod
        {
            get { return modInstance; }
        }
        
        public bool EnableDeveloperLog
        {
            get { return DeveloperLog; }
            set { DeveloperLog = value; }
        }

        public bool EnableImmediateDestroys
        {
            get { return Immediate; }
            set { Immediate = value; }
        }
        
        public bool PreciseCloning
        {
            get { return Cloning; }
            set { Cloning = value; }
        }
        
        internal ModSettings(ModInstance _modInstance)
        {
            modInstance = _modInstance;
        }
    }
}
