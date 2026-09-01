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

        internal Dictionary<string, string> PathExceptionList = new Dictionary<string, string>();

        internal List<string> ForceIgnore = new List<string>();

        internal List<string> PathForceIgnore = new List<string>();

        public void AddException(string partName, string prefabName, bool forceFittingIgnoringParent = false)
        {
            ExceptionList.Add(partName, prefabName);

            if(forceFittingIgnoringParent)
                ForceIgnore.Add(partName);
        }

        public void AddPathException(string path, string prefabName, bool forceFittingIgnoringParent = false)
        {
            path = NormalizePath(path);
            PathExceptionList.Add(path, prefabName);

            if(forceFittingIgnoringParent)
                PathForceIgnore.Add(path);
        }

        public void ForceExceptionListReset() { ForceIgnore.Clear(); PathForceIgnore.Clear(); }

        public bool TryGetException(string partName, string path, out string prefabName)
        {
            if (TryGetPathException(path, out prefabName))
                return true;

            return ExceptionList.TryGetValue(partName, out prefabName);
        }

        public bool IgnoringStatusForPart(string partName)
        {
            if(ForceIgnore.Contains(partName))
                return true;

            return false;
        }

        public bool IgnoringStatusForPart(string partName, string path)
        {
            if (ContainsPath(PathForceIgnore, path))
                return true;

            return IgnoringStatusForPart(partName);
        }

        // We ensure no ending on /
        private static string NormalizePath(string path)
        {
            return string.IsNullOrEmpty(path) ? path : path.Trim('/');
        }

        private bool TryGetPathException(string path, out string prefabName)
        {
            path = NormalizePath(path);
            if (!string.IsNullOrEmpty(path) && PathExceptionList.TryGetValue(path, out prefabName))
                return true;

            int rootSeparator = string.IsNullOrEmpty(path) ? -1 : path.IndexOf('/');
            if (rootSeparator >= 0 && PathExceptionList.TryGetValue(path.Substring(rootSeparator + 1), out prefabName))
                return true;

            prefabName = null;
            return false;
        }

        private static bool ContainsPath(List<string> paths, string path)
        {
            path = NormalizePath(path);
            if (!string.IsNullOrEmpty(path) && paths.Contains(path))
                return true;

            int rootSeparator = string.IsNullOrEmpty(path) ? -1 : path.IndexOf('/');
            return rootSeparator >= 0 && paths.Contains(path.Substring(rootSeparator + 1));
        }
    }
}
