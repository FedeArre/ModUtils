using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimplePartLoader.Features.UI.Saving
{
    internal class UISettingsWrapper
    {
        public List<ModWrapper> ModWrappers = new List<ModWrapper>();

        public UISettingsWrapper() 
        { 
            ModWrappers = new List<ModWrapper>();
        }
    }

    internal class ModWrapper
    {
        public string ModId;
        public List<SettingWrapper> Settings;

        internal ModWrapper()
        {
            Settings = new List<SettingWrapper>();
        }
    }

    internal class SettingWrapper
    {
        public string Id;
        public object Value;

        public SettingWrapper(string id, object value)
        {
            Id = id;
            Value = value;
        }
    }
}
