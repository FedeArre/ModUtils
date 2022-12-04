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

        public void AddException(string partName, string prefabName)
        {
            ExceptionList.Add(partName, prefabName);
        }
    }
}
