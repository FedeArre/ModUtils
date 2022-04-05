
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
        public override string Version => "v1.3.0";

        Transform PlayerTransform;
        bool PlayerOnCar;

        public ModMain()
        {
            Debug.LogError("SimplePartLoader is loading - Version: " + Version);
            Debug.LogError("Developed by Federico Arredondo - www.github.com/FedeArre");
        }

        public override void OnLoad()
        {
            ModAPI.OnLoadCalled();
            PartManager.OnLoadCalled();

            PlayerTransform = ModAPI.GetPlayer().transform;
        }

        public override void Update()
        {
            if (PlayerTransform)
            {
                if(PlayerTransform.parent == null && PlayerOnCar)
                {
                    ModAPI.UpdatePlayerStatus(PlayerOnCar);
                    // Player not longer on car.
                }

                else if(PlayerTransform.parent != null && !PlayerOnCar)
                {
                    MainCarProperties mcp = PlayerTransform.root.GetComponent<MainCarProperties>();
                    if(mcp)
                    {
                        PlayerOnCar = true;
                        ModAPI.UpdatePlayerStatus(PlayerOnCar, mcp);
                    }
                }
            }
        }
    }
}