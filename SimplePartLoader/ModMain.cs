using System.Collections;
using System.IO;
using UnityEngine;

namespace SimplePartLoader
{
    public class ModMain : Mod
    {
        // Looking for docs? https://fedearre.github.io/my-garage-modding-docs/
        public override string ID => "ModUtils";
        public override string Name => "ModUtils";
        public override string Author => "Federico Arredondo";
        public override string Version => "v1.0.0";

        public override byte[] Icon => Properties.Resources.SimplePartLoaderIcon;

        Transform PlayerTransform;
        bool PlayerOnCar;
        
        string[] modsToDelete = { "_SimplePartLoader.dll", "Extra Buildings.dll", "Autoupdater.dll" };
        
        AssetBundle Bundle;
        GameObject ModShopPrefab;
        Material FloorMat;

        readonly Vector3 POSITION_SHOP = new Vector3(722.5838f, 38.12f, -189.3593f);
        readonly Vector3 ROTATION_SHOP = new Vector3(0f, 90f, 0f);

        public ModMain()
        {
            Debug.Log("ModUtils is loading - Version: " + Version);
            Debug.Log("Developed by Federico Arredondo - www.github.com/FedeArre");

            string ModsFolderPath = Application.dataPath + "/../Mods/";
            foreach(string s in modsToDelete)
            {
                if (File.Exists(ModsFolderPath + s))
                {
                    File.Delete(ModsFolderPath + s);
                    Debug.Log($"[ModUtils]: Mod {s} has been replaced by ModUtils");
                }
            }

            Bundle = AssetBundle.LoadFromMemory(Properties.Resources.extra_buildings_models);

            ModShopPrefab = Bundle.LoadAsset<GameObject>("shopWarehouse");
            FloorMat = Bundle.LoadAsset<Material>("customCubeFloor");

            Bundle.Unload(false);
        }

        public override void OnMenuLoad()
        {
            Debug.Log("[ModUtils]: Printing current loaded mods");
            foreach(Mod m in ModLoader.mods)
            {
                Debug.Log($"- {m.Name} (ID: {m.ID}) - Version {m.Version}");
            }
        }

        public override void OnLoad()
        {
            ModUtils.OnLoadCalled();
            PartManager.OnLoadCalled();
            
            PlayerTransform = ModUtils.GetPlayer().transform;

            if(PlayerPrefs.GetFloat("LoadLevel") == 0f)
                CustomSaverHandler.NewGame();

            // Mod shop load
            if (ModUtils.GetPlayerTools().MapMagic)
                return;

            GameObject shop = GameObject.Instantiate(ModShopPrefab, POSITION_SHOP, Quaternion.Euler(ROTATION_SHOP));
            shop.transform.localScale = new Vector3(0.87f, 0.87f, 0.87f);
            shop.name = "EXTRA_BUILDINGS_MODSHOP";

            GameObject shopSupportCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            shopSupportCube.transform.localPosition = new Vector3(657.1829f, 51.2f, -46.6193f);
            shopSupportCube.transform.localScale = new Vector3(31.37f, 4.26f, 84.48f);
            shopSupportCube.GetComponent<Renderer>().material = FloorMat;
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