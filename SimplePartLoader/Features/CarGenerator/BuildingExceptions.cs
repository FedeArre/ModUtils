using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimplePartLoader.CarGen
{
    internal class BuildingExceptions
    {
        internal Dictionary<string, string> ExceptionList = new Dictionary<string, string>();
        internal List<string> ForceIgnore = new List<string>();

        public void AddException(string partName, string prefabName, bool forceFittingIgnoringParent = false)
        {
            ExceptionList.Add(partName, prefabName);

            if(forceFittingIgnoringParent)
                ForceIgnore.Add(partName);
        }

        public bool IgnoringStatusForPart(string partName)
        {
            if(ForceIgnore.Contains(partName))
                return true;

            return false;
        }
    }
}
