using System;
using System.Collections.Generic;
using System.Text;
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

        public override void OnLoad()
        {
            PartManager.OnLoadCalled();
        }

        public override void Update()
        {
            PartManager.OnUpdateCalled();
        }
    }
}