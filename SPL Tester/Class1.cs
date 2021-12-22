using System;
using System.Collections.Generic;
using System.Text;
using Assets.SimpleLocalization;
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

        public Class1() // This is the mod constructor - SimplePartLoader has to be used in the constructor of the mod only
        {
            AssetBundle bundle = AssetBundle.LoadFromMemory(Properties.Resources.spoiler_example);
            Part examplePart = SPL.LoadPart(bundle, "AwesomeSpoiler"); // "AwesomeSpoiler" is the name of the prefab.
            bundle.Unload(false);
        }

        public override void OnLoad()
        {
            foreach(var asd in LocalizationManager.Dictionary)
            {
                Debug.LogError(asd.Key);
            }
        }
    }
}