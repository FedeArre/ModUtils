// The following preprocessor declaration disables the timing feature from ModUtils.
// This module can create unrequired overhead on final builds
//#define MODUTILS_TIMING_ENABLED

using Autoupdater.Objects;
using Newtonsoft.Json;
using SimplePartLoader;
using SimplePartLoader.CarGen;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
using SimplePartLoader.Features;
using SimplePartLoader.Features.CarGenerator;
using HarmonyLib;
using SimplePartLoader.Features.UI;

namespace SimplePartLoader
{
    public class ModMain : Mod
    {
        // Looking for docs? https://fedearre.github.io/my-garage-modding-docs/
        public override string ID => "ModUtils";
        public override string Name => "ModUtils";
        public override string Author => "Federico Arredondo";
        public override string Version => "v1.4.0";
        
        bool TESTING_VERSION_REMEMBER = true;
        internal static string TESTING_VERSION_NUMBER = "v1.5.0-dev3";
        
        public override byte[] Icon => Properties.Resources.SimplePartLoaderIcon;

        // Autoupdater
        public const string API_URL = "https://modding.fedes.uy/api";

        internal static GameObject UI_Prefab, UI_Error_Prefab, UI_BrokenInstallation_Prefab, UI_DeveloperLogEnabled_Prefab, UI_Downloader_Prefab, UI_Developer, UI_EA, UI_Mods, UI_Mods_Prefab, UI_Info_Prefab;
        AssetBundle AutoupdaterBundle;
        bool MenuFirstLoad;

        // ModUtils
        Transform PlayerTransform;
        bool PlayerOnCar;
        
        // Mod shop
        AssetBundle Bundle;
        GameObject ModShopPrefab;
        Material FloorMat;

        internal static Checkbox EA_Enabled, Telemetry;

        Stopwatch watch;
        public ModMain()
        {
#if MODUTILS_TIMING_ENABLED
            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();
#endif
            watch = new System.Diagnostics.Stopwatch();
            watch.Start();

            CustomLogger.AddLine("Main", "ModUtils is loading - Version: " + Version);
            CustomLogger.AddLine("Main", "Developed by Federico Arredondo - www.github.com/FedeArre");
            if (TESTING_VERSION_REMEMBER)
                Debug.Log($"This is a testing version ({TESTING_VERSION_NUMBER}) - remember to report bugs and send feedback");

            var harmony = new Harmony("com.modutils");
            harmony.PatchAll();

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
            UI_Developer = AutoupdaterBundle.LoadAsset<GameObject>("DevCanvas");
            UI_EA = AutoupdaterBundle.LoadAsset<GameObject>("EACanvas");
            UI_Mods_Prefab = AutoupdaterBundle.LoadAsset<GameObject>("ModUICanvas");
            UI_Info_Prefab = AutoupdaterBundle.LoadAsset<GameObject>("CanvasInfo");

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

            // UI update
            ModInstance mi = ModUtils.RegisterMod(this);
            mi.SetSettingsLoadedFunction(LoadSettings);
            EA_Enabled = mi.AddCheckboxToUI("ModUtils_EnableEA", "Enable Early Access (requires game restart)", false);
            mi.AddLabelToUI("Telemetry is used by mod developers to know how they mod performs. ModUtils will send a list of the mods you are currently using while playing, no data is stored.");
            Telemetry = mi.AddCheckboxToUI("ModUtils_Telemetry", "Telemetry enabled", true);
            mi.AddSpacerToUI();
            mi.AddSpacerToUI();
            mi.AddLabelToUI("Following stuff is testing for v1.5-dev2 - Ignore! :)");
            mi.AddTextInputToUI("textInputId2", "Text input with default value:", "value");
            mi.AddLabelToUI("Testing!");
            mi.AddLabelToUI("Testing a bit more, how is this?");
            mi.AddTextInputToUI("textInputTest", "Text input test:");
            mi.AddLabelToUI("Testing even more, this is very long now. Great!? ModUtils v1.5.0 dev2 UI tests :)\nTrying jump line.");
            mi.AddDropdownToUI("coolDropdown", "Dropdown test", new string[] { "test 1", "test2", "test55" });
            mi.AddSliderToUI("sliderTest", 0, 10, 2);
            mi.AddButtonToUI("test butotn");
            mi.AddCheckboxToUI("checkboxTest", "hola mi vida no desconfies de la musica", true);
            mi.AddLabelToUI("Setup keybind for do nothing: ");
            mi.AddKeybindToUI("testKeybind", KeyCode.W);

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
                watch.Stop();
                CustomLogger.AddLine("Timing", $"Mods took {watch.ElapsedMilliseconds} ms to load.");

                MenuFirstLoad = true;
                CustomLogger.AddLine("Main", "Printing mod list");
                foreach (Mod m in ModLoader.mods)
                {
                    CustomLogger.AddLine("Main", $"{m.Name} (ID: {m.ID}) - Version {m.Version}");
                }

                UI_Mods = GameObject.Instantiate(UI_Mods_Prefab);
                GameObject.DontDestroyOnLoad(UI_Mods);

                ModUtilsUI.PrepareUI();
                SettingSaver.LoadSettings();

                GameObject modUiRemove = GameObject.Find("ModUICanvas(Clone)");
                if(modUiRemove)
                {
                    modUiRemove.SetActive(false);
                }
                return;
            }

            UI_Mods.SetActive(true);
            UI_Mods.transform.Find("OpenMods").localScale = Vector3.one;
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
            try
            {
                PartManager.OnLoadCalled();
            }
            catch(Exception ex)
            {
                CustomLogger.AddLine("Parts", ex);
            }

#if MODUTILS_TIMING_ENABLED
            watch.Stop();
            totalTime += watch.ElapsedMilliseconds;
            Debug.Log($"[ModUtils/Timing]: SPL (PartManager) library succesfully loaded - Took ${watch.ElapsedMilliseconds} ms");

            watch.Restart();
#endif
            BuildableManager.OnGameLoad();
#if MODUTILS_TIMING_ENABLED
            watch.Stop();
            totalTime += watch.ElapsedMilliseconds;
            Debug.Log($"[ModUtils/Timing]: Building (BuildingManager) library succesfully loaded - Took ${watch.ElapsedMilliseconds} ms");

            watch.Restart();
#endif
            PlayerTransform = ModUtils.GetPlayer().transform;

            if(PlayerPrefs.GetFloat("LoadLevel") == 0f)
                CustomSaverHandler.NewGame();

            if(CustomLogger.SaveDissasamble)
            {
                CustomLogger.AddLine("Dev", "Save dissasembling has been enabled! - Information in Player.log");
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

            UI_Mods.transform.Find("OpenMods").localScale = Vector3.one / 2;
            UI_Mods.SetActive(false);

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

            // Load modshop into map
            try
            {
                GameObject scene = GameObject.Find("SceneManager");
                Transform centerMap = scene.transform.Find("MapCanvas/GameObject/MAP/CenterOfMap");
                Transform shopRef = centerMap.Find("Shop");
                GameObject modShopObj = GameObject.Instantiate(shopRef.gameObject);
                modShopObj.transform.SetParent(centerMap);
                modShopObj.transform.Find("GameObject").GetComponent<Text>().text = "Mod shop";
                modShopObj.transform.localPosition = new Vector3(217f, -27f, 0f);
            }
            catch { }

            new GameObject("tempRuinedOverwrite").AddComponent<RuinedOverwrite>();

            CatalogMassUpdate.ApplyChanges();

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

                if(ModUtils.PlayerTools.EscMenu.activeSelf && !UI_Mods.activeSelf)
                {
                    UI_Mods.SetActive(true);
                }

                else if(!ModUtils.PlayerTools.EscMenu.activeSelf && UI_Mods.activeSelf)
                {
                    UI_Mods.SetActive(false);
                }
            }
        }

        public override void OnNewMapLoad()
        {
            BuildableManager.OnNewMapEnabled();
        }

        public override void LateUpdate()
        {
            if (ComputerUI.PlayerAtComputer && Input.GetKeyDown(KeyCode.Escape))
            {
                ComputerUI.Close();
                ModUtils.PlayerTools.ESC();
            }
        }

        public  void LoadSettings()
        {
            // Enable heartbeat
            KeepAlive.GetInstance().Ready();
        }
    }
}