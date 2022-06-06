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
        public override string Version => "v1.4.0";

        public override byte[] Icon => Properties.Resources.SimplePartLoaderIcon;

        Transform PlayerTransform;
        bool PlayerOnCar;

        public ModMain()
        {
            Debug.Log("SimplePartLoader is loading - Version: " + Version);
            Debug.Log("Developed by Federico Arredondo - www.github.com/FedeArre");
        }

        public override void OnLoad()
        {
            ModUtils.OnLoadCalled();
            PartManager.OnLoadCalled();
            
            PlayerTransform = ModUtils.GetPlayer().transform;

            if(PlayerPrefs.GetFloat("LoadLevel") == 0f)
                CustomSaverHandler.NewGame();
        }

        public override void Continue()
        {
            // Custom saving
            // Custom data saving is not enabled for survival mode!
            if (ModUtils.GetPlayerTools().MapMagic)
                return;

            GameObject dummyObject = new GameObject("SPL_Dummy");
            dummyObject.AddComponent<SavingHandlerMono>().Load();
        }

        public override void OnSaveFinish()
        {
            // Custom data saving is not enabled for survival mode!
            if (ModUtils.GetPlayerTools().MapMagic)
                return;

            CustomSaverHandler.Save();
        }
        
        // For mod utils
        public override void Update()
        {
            if (PlayerTransform)
            {
                if(PlayerTransform.parent == null && PlayerOnCar)
                {
                    PlayerOnCar = false;
                    ModUtils.UpdatePlayerStatus(PlayerOnCar);
                }

                else if(PlayerTransform.parent != null && !PlayerOnCar)
                {
                    MainCarProperties mcp = PlayerTransform.root.GetComponent<MainCarProperties>();
                    if(mcp)
                    {
                        PlayerOnCar = true;
                        ModUtils.UpdatePlayerStatus(PlayerOnCar, mcp);
                    }
                }
            }
        }
    }
}