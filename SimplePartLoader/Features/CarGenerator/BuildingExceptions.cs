using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimplePartLoader.CarGen
{
    public class BuildingExceptions
    {
        internal Dictionary<string, string> ExceptionList = new Dictionary<string, string>()
        {
            { "Spacer", "disabled_by_default" },
            { "Jackstand", "disabled_by_default" }
        };

        internal List<string> ForceIgnore = new List<string>();

        public void AddException(string partName, string prefabName, bool forceFittingIgnoringParent = false)
        {
            ExceptionList.Add(partName, prefabName);

            if(forceFittingIgnoringParent)
                ForceIgnore.Add(partName);
        }

        public void ForceExceptionListReset() { ForceIgnore.Clear(); }

        public bool IgnoringStatusForPart(string partName)
        {
            if(ForceIgnore.Contains(partName))
                return true;

            return false;
        }
    }
}
