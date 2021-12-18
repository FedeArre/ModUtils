using System;
using System.Collections.Generic;
using System.Text;
using SimplePartLoader;
using UnityEngine;

namespace SPL_Tester
{
    public class Class1 : Mod
    {
        // Looking for docs? https://fedearre.github.io/my-garage-modding-docs/
        public override string ID => "Your mod's ID! (has to be unique)";
        public override string Name => "Your mod's name";
        public override string Author => "Your name";
        public override string Version => "1.0";

        public Class1()
        {
            AssetBundle bundle = AssetBundle.LoadFromMemory(Properties.Resources.spoiler_example);
            Part p = SPL.LoadPart(bundle, "Spoiler");
            p.SetupTransparent("TrunkDoor06", new Vector3(0f, 0f, 0f), Quaternion.Euler(0f, 0f, 0f));
            bundle.Unload(false);
        }
    }
}