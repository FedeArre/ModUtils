using System;
using System.Collections.Generic;
using UnityEngine;

namespace SimplePartLoader
{
    public class StartOption
    {
        /// <summary>
        /// The prefab of the start option. Empty until game load.
        /// </summary>
        public GameObject Prefab { get; internal set; }

        /// <summary>
        /// The part name to copy from the prefab.
        /// </summary>
        public string PartToCopy { get; internal set; }

        /// <summary>
        /// The exceptions to use when part building happens. Key is the renamed prefab, value is the prefab name to force in
        /// </summary>
        internal Dictionary<string, string> Exceptions = new Dictionary<string, string>();

        /// <summary>
        /// Function to be called after the prefab is built, so fixes can be applied to it.
        /// </summary>
        internal Action PostBuildFunction { get; set; }

        /// <summary>
        /// The mod that registered this start option
        /// </summary>
        internal ModInstance LoadedBy { get; set; }

        /// <summary>
        /// The settings of the start option, used by the builder to know about what fixes to apply
        /// </summary>
        public StartOptionSettings Settings { get; set; } = new StartOptionSettings();

        internal StartOption(GameObject prefab, string partToCopy, Dictionary<string, string> exceptions, ModInstance loadedBy)
        {
            Prefab = prefab;
            Exceptions = exceptions;
            PartToCopy = partToCopy;
            LoadedBy = loadedBy;
        }

        /// <summary>
        /// Allows to add an exception on start option building
        /// </summary>
        /// <param name="prefabNameToForce">The prefab name to force in</param>
        /// <param name="renamedPrefab">The renamed prefab to have the exception</param>
        public void AddException(string renamedPrefab, string prefabNameToForce)
        {
            if(Exceptions.ContainsKey(renamedPrefab))
            {
                Exceptions[renamedPrefab] = prefabNameToForce;
            }
            else
            {
                Exceptions.Add(renamedPrefab, prefabNameToForce);
            }
        }

        /// <summary>
        /// Set a function to be called after the prefab is built
        /// </summary>
        /// <param name="postBuildFunction">The function to call</param>
        public void SetPostBuildFunction(Action postBuildFunction)
        {
            PostBuildFunction = postBuildFunction;
        }
    }

    public class StartOptionSettings
    {
        public bool ApplyAttachFixes = true;
        public bool ApplyPaintingFixes = true;
        public bool ApplyTransparentReferenceFix = true;
        public bool ApplyVisualObjectFix = true;
        public bool ApplyRenamedPrefabNameCorrectionFix = true;
    }
}
