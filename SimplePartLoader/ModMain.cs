
using System.Collections;
using UnityEngine;

namespace SimplePartLoader
{
    public class ModMain : Mod
    {
        // Looking for docs? https://fedearre.github.io/my-garage-modding-docs/
        public override string ID => "SimplePartLoader";
        public override string Name => "SimplePartLoader";
        public override string Author => "Federico Arredondo";
        public override string Version => "v1.2.0";

        public ModMain()
        {
            Debug.LogError("SimplePartLoader is loading - Version: " + Version);
            Debug.LogError("Developed by Federico Arredondo - www.github.com/FedeArre");
        }

        public override void OnLoad()
        {
            PartManager.OnLoadCalled();
        }
    }
}