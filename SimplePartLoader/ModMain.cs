
using UnityEngine;

namespace SimplePartLoader
{
    public class ModMain : Mod
    {
        // Looking for docs? https://fedearre.github.io/my-garage-modding-docs/
        public override string ID => "SimplePartLoader";
        public override string Name => "SimplePartLoader";
        public override string Author => "Federico Arredondo";
        public override string Version => "dev";

        public static bool IsTransparentEditingEnabled = false;

        public override void OnLoad()
        {
            PartManager.OnLoadCalled();
        }
    }
}