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
        private bool CullShader = false;
        private PaintingSystem.PartPaintResolution PaintRes = PaintingSystem.PartPaintResolution.Low;
        private string[] FitsToCar;
        private string[] FitsToEngine;
        private string PrefixPrefabName;

        public ModInstance Mod
        {
            get { return modInstance; }
        }

        [Obsolete("This feature has been removed - Please use Mod Status Report!")]
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

        public string[] AutomaticFitsToCar
        {
            get { return FitsToCar; }
            set { FitsToCar = value; }   
        }
        public string[] AutomaticFitsToEngine
        {
            get { return FitsToEngine; }
            set { FitsToEngine = value; }
        }

        public string PrefabNamePrefix
        {
            get { return PrefixPrefabName; }
            set { PrefixPrefabName = value; }
        }

        public PaintingSystem.PartPaintResolution PaintResolution
        {
            get { return PaintRes; }
            set { PaintRes = value; }
        }

        public bool UseBackfaceShader
        {
            get { return CullShader; }
            set { CullShader = value; }
        }

        internal ModSettings(ModInstance _modInstance)
        {
            modInstance = _modInstance;
        }
    }
}
