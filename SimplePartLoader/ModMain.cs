// The following preprocessor declaration disables the timing feature from ModUtils.
// This module can create unrequired overhead on final builds
//#define MODUTILS_TIMING_ENABLED

using Autoupdater.Objects;
using ModUI;
using ModUI.Settings;
using static ModUI.Settings.ModSettings;
using Newtonsoft.Json;
using SimplePartLoader;
using SimplePartLoader.CarGen;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine.ProBuilder;
using SimplePartLoader.Features;

namespace SimplePartLoader
{
    public class ModMain : Mod, IModDescription, IModSettings
    {
        // Looking for docs? https://fedearre.github.io/my-garage-modding-docs/
        public override string ID => "ModUtils";
        public override string Name => "ModUtils";
        public override string Author => "Federico Arredondo";
        public override string Version => "v1.3.1";
        
        bool TESTING_VERSION_REMEMBER = true;
        string TESTING_VERSION_NUMBER = "charger development build - 8";
        
        public override byte[] Icon => Properties.Resources.SimplePartLoaderIcon;

        public string Description => "Allows you to create awesome stuff!";

        // Autoupdater
        public const string API_URL = "https://modding.fedes.uy/api";

        internal static GameObject UI_Prefab, UI_Error_Prefab, UI_BrokenInstallation_Prefab, UI_DeveloperLogEnabled_Prefab, UI_Downloader_Prefab;
        AssetBundle AutoupdaterBundle;
        bool MenuFirstLoad;

        // ModUtils
        Transform PlayerTransform;
        bool PlayerOnCar;
        
        // Mod shop
        AssetBundle Bundle;
        GameObject ModShopPrefab;
        Material FloorMat;

        // Mod settings
        internal static ModUI.Settings.ModSettings.Toggle TelemetryToggle;
        public static ModUI.Settings.ModSettings.Toggle EnableEarlyAccess;

        public ModMain()
        {
#if MODUTILS_TIMING_ENABLED
            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();
#endif
            Debug.Log("ModUtils is loading - Version: " + Version);
            Debug.Log("Developed by Federico Arredondo - www.github.com/FedeArre");
            if(TESTING_VERSION_REMEMBER)
                Debug.Log($"This is a testing version ({TESTING_VERSION_NUMBER}) - remember to report bugs and send feedback");

            // Deleting unused stuff
            string ModsFolderPath = Application.dataPath + "/../Mods/";
            
            if (Directory.Exists(ModsFolderPath + "Autoupdater/"))
            {
                Directory.Delete(ModsFolderPath + "Autoupdater/", true);
            }

            if (Directory.Exists(ModsFolderPath + "NewAutoupdater/"))
            {
                Directory.Delete(ModsFolderPath + "NewAutoupdater/", true);
            }

            if (Directory.Exists(ModsFolderPath + "ModUtilsUpdating/"))
            {
                Directory.Delete(ModsFolderPath + "ModUtilsUpdating/", true);
            }

            if (File.Exists(ModsFolderPath + "modUtilsDownloader.exe"))
            {
                File.Delete(ModsFolderPath + "modUtilsDownloader.exe");
            }

            // Mod shop
            Bundle = AssetBundle.LoadFromMemory(Properties.Resources.extra_buildings_models);

            ModShopPrefab = Bundle.LoadAsset<GameObject>("shopWarehouse");
            FloorMat = Bundle.LoadAsset<Material>("customCubeFloor");

            Bundle.Unload(false);

            // Autoupdater
            AutoupdaterBundle = AssetBundle.LoadFromMemory(Properties.Resources.autoupdater_ui_canvas);
            UI_Prefab = AutoupdaterBundle.LoadAsset<GameObject>("Canvas");
            UI_Error_Prefab = AutoupdaterBundle.LoadAsset<GameObject>("CanvasError");
            UI_BrokenInstallation_Prefab = AutoupdaterBundle.LoadAsset<GameObject>("CanvasBrokenInstallation");
            UI_DeveloperLogEnabled_Prefab = AutoupdaterBundle.LoadAsset<GameObject>("CanvasDevLog");
            UI_Downloader_Prefab = AutoupdaterBundle.LoadAsset<GameObject>("CanvasDownloader");

            // Computer stuff
            ComputerUI.UI_Prefab = AutoupdaterBundle.LoadAsset<GameObject>("Computer");
            ComputerUI.ComputerModelPrefab = AutoupdaterBundle.LoadAsset<GameObject>("ComputerPrefab");
            ComputerUI.SetupComputer(AutoupdaterBundle.LoadAsset<GameObject>("AppLauncher"), AutoupdaterBundle.LoadAsset<GameObject>("AppLauncherIcon"));
            
            // Some bug fixing
            UI_Prefab.GetComponent<Canvas>().sortingOrder = 1; // Fixes canva disappearing after a bit.
            UI_Downloader_Prefab.GetComponent<Canvas>().sortingOrder = 1;
            UI_Error_Prefab.GetComponent<Canvas>().sortingOrder = 1;

            PaintingSystem.BackfaceShader = AutoupdaterBundle.LoadAsset<Shader>("BackfaceShader");
            PaintingSystem.CullBaseMaterial = AutoupdaterBundle.LoadAsset<Material>("testMat");
            AutoupdaterBundle.Unload(false);

            ModUtils.SetupSteamworks();
            MainCarGenerator.BaseSetup();

#if MODUTILS_TIMING_ENABLED
            watch.Stop();
            Debug.Log($"[ModUtils/Timing/Constructor]: ModUtils succesfully loaded in {watch.ElapsedMilliseconds} ms");
#endif
        }

        public override void OnMenuLoad()
        {
            if (!MenuFirstLoad)
            {
                MenuFirstLoad = true;
                Debug.Log("[ModUtils/Main]: Printing mod list.");
                foreach(Mod m in ModLoader.mods)
                {
                    Debug.Log($"{m.Name} (ID: {m.ID}) - Version {m.Version}");
                }

                // Enable heartbeat
                KeepAlive.GetInstance().Ready();
                
                return;
            }

            if(SPL.DEVELOPER_LOG)
            {
                GameObject.Instantiate(UI_DeveloperLogEnabled_Prefab);
            }
        }

        public override void OnLoad()
        {
#if MODUTILS_TIMING_ENABLED
            var watch = new System.Diagnostics.Stopwatch();
            long totalTime = 0;

            watch.Start();

            Debug.Log($"[ModUtils/Timing]: OnLoad method was called!");
#endif
            // ModUtils library time
            ModUtils.OnLoadCalled();
#if MODUTILS_TIMING_ENABLED
            watch.Stop();
            totalTime += watch.ElapsedMilliseconds;
            Debug.Log($"[ModUtils/Timing]: ModUtils library succesfully loaded - Took ${watch.ElapsedMilliseconds} ms");

            // PartManager library time
            watch.Restart();
#endif
            PartManager.OnLoadCalled();
#if MODUTILS_TIMING_ENABLED
            watch.Stop();
            totalTime += watch.ElapsedMilliseconds;
            Debug.Log($"[ModUtils/Timing]: SPL (PartManager) library succesfully loaded - Took ${watch.ElapsedMilliseconds} ms");

            watch.Restart();
#endif
            PlayerTransform = ModUtils.GetPlayer().transform;

            if(PlayerPrefs.GetFloat("LoadLevel") == 0f)
                CustomSaverHandler.NewGame();

            if(SPL.ENABLE_SAVE_DISSASAMBLE)
            {
                Debug.Log("[ModUtils/Dev]: Save dissasembling has been enabled!");
                SaveSystem save = new SaveSystem(Application.persistentDataPath + "/save1/save.dat");
                if (File.Exists(Application.persistentDataPath + "/save1/save.dat"))
                {
                    save.read();
                    Debug.Log("[ModUtils/Dev]: Save file found, loading...");
                    foreach(DictionaryEntry s in save.table)
                    {
                        Debug.Log("[SD]: " + s.Key + " | " + s.Value);
                        if(s.Value is List<String>)
                        {
                            Debug.Log("[SD]: Entry above is string list, content: ");
                            foreach(string str in (List<string>)s.Value)
                            {
                                Debug.Log("[SD]: " + str);
                            }
                        }
                    }
                    Debug.Log("[ModUtils/Dev]: Loaded " + save.table.Count + " entries.");
                }
            }
            
            // Mod shop load
            if (ModUtils.GetPlayerTools().MapMagic)
                return;

#if MODUTILS_TIMING_ENABLED
            watch.Stop();
            totalTime += watch.ElapsedMilliseconds;
            Debug.Log($"[ModUtils/Timing]: Save dissasembly & various library succesfully loaded - Took ${watch.ElapsedMilliseconds} ms");
            watch.Restart();
#endif
            // Computer UI stuff
            ComputerUI.LoadComputerTable();
#if MODUTILS_TIMING_ENABLED
            watch.Stop();
            totalTime += watch.ElapsedMilliseconds;
            Debug.Log($"[ModUtils/Timing]: In-game computer succesfully loaded - Took ${watch.ElapsedMilliseconds} ms");
            watch.Restart();
#endif
            // Furniture stuff
            FurnitureManager.SetupFurniture();

            GameObject shop = GameObject.Instantiate(ModShopPrefab, new Vector3(722.5838f, 38.12f, -189.3593f), Quaternion.Euler(new Vector3(0, 90f, 0)));
            shop.transform.localScale = new Vector3(0.87f, 0.87f, 0.87f);
            shop.name = "EXTRA_BUILDINGS_MODSHOP";

            GameObject shopSupportCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            shopSupportCube.transform.localPosition = new Vector3(657.1829f, 51.2f, -46.6193f);
            shopSupportCube.transform.localScale = new Vector3(31.37f, 4.26f, 84.48f);
            shopSupportCube.GetComponent<Renderer>().material = FloorMat;
#if MODUTILS_TIMING_ENABLED
            watch.Stop();
            totalTime += watch.ElapsedMilliseconds;
            Debug.Log($"[ModUtils/Timing]: Furniture manager & ModShop succesfully loaded - Took {watch.ElapsedMilliseconds} ms");
            Debug.Log($"[ModUtils/Timing]: ModUtils loading took ${totalTime} ms");
#endif

        }

        public override void Continue()
        {
            ComputerUI.Continue();
        }

        public override void OnSaveSystemSave(SaveSystem saver, bool isBarn)
        {
            if (ModUtils.GetPlayerTools().MapMagic)
                return;

            CustomSaverHandler.Save(saver, isBarn);
            if (!isBarn)
            {
                FurnitureManager.SaveFurniture(saver);
                ComputerUI.Save();
                DataHandler.OnSave(saver);
            }
        }

        public override void OnSaveSystemLoad(SaveSystem saver, bool isBarn)
        {
            if (ModUtils.GetPlayerTools().MapMagic)
                return;

            // We execute the Load method
            // A dummy is required because we need a frame between the actual object load and the data load
            GameObject dummyObject = new GameObject("SPL_Dummy");
            dummyObject.AddComponent<SavingHandlerMono>().Load(saver, isBarn);

            if (!isBarn) {
                FurnitureManager.LoadFurniture(saver);
                DataHandler.OnLoad(saver);
            }
        }

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

        public override void LateUpdate()
        {
            if (ComputerUI.PlayerAtComputer && Input.GetKeyDown(KeyCode.Escape))
            {
                ComputerUI.Close();
                ModUtils.PlayerTools.ESC();
            }
        }

        public void CreateModSettings(ModUI.Settings.ModSettings modSettings)
        {
            EnableEarlyAccess = modSettings.AddToggle("Enable Early Access (You need to restart the game for mods to load when enabled)", "EnableEA_ModUtils", false);
            modSettings.AddSpace();
            modSettings.AddSpace();
            TelemetryToggle = modSettings.AddToggle("Telemetry enabled", "TelemetryEnabledModutils", true);
            modSettings.AddLabel("Telemetry is used by mod developers to know how they mod performs. ModUtils will send a list of the mods you are currently using while playing, no data is stored.");
            modSettings.AddSpace();
            modSettings.AddSpace();
            modSettings.AddSpace();
        }

        public void ModSettingsLoaded() { }
    }
}