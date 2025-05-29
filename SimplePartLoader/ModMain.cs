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
using static Unity.Burst.Intrinsics.X86.Avx;
using System.Linq;
using System.Reflection;
using EVP;
using SimplePartLoader.Utils;
using System.Net.Http;
using System.Security.Policy;

namespace SimplePartLoader
{
    public class ModMain : Mod
    {
        // Looking for docs? https://fedearre.github.io/my-garage-modding-docs/
        public override string ID => "ModUtils";
        public override string Name => "ModUtils";
        public override string Author => "Federico Arredondo";
        public override string Version => "v1.5.1B";
        
        bool TESTING_VERSION_REMEMBER = true;
        internal static string TESTING_VERSION_NUMBER = "v1.5.2-test4";
        
        public override byte[] Icon => Properties.Resources.SimplePartLoaderIcon;

        // Autoupdater
        public const string API_URL = "https://modding.fedes.uy/";
        //public const string API_URL = "https://localhost:7060/";

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

        internal static Checkbox EA_Enabled, Telemetry, DontDisableModUI, RandomBG, UrpCompatibility;
        internal static ModDropdown ForcedPaintQuality;

        internal static HttpClient Client;

        // Developer stuff for UI
        internal static Checkbox DevUIEnabled;
        internal static Keybind TogglePosRot;
        internal static Keybind Multiplier;
        internal static Keybind XMinus;
        internal static Keybind X90;
        internal static Keybind XPlus;
        internal static Keybind YMinus;
        internal static Keybind Y90;
        internal static Keybind YPlus;
        internal static Keybind ZMinus;
        internal static Keybind Z90;
        internal static Keybind ZPlus;
        internal static Keybind PreviewCube;

        internal static byte[] imageBytes; // Random BGs

        Stopwatch watch;
        public ModMain()
        {
            // Some setups
            ModUtils.Version = Version;

            Client = new HttpClient();
            Client.BaseAddress = new Uri(API_URL);
            Client.DefaultRequestHeaders.Add("User-Agent", $"ModUtils/{ModUtils.Version}");

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

            var harmony = new Harmony("com.fedes.modutils");
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
            mi.AddLabelToUI("Telemetry is used by mod developers to know how they mod performs. ModUtils will send a list of the mods you are currently using while playing, no data is stored in any way.");
            Telemetry = mi.AddCheckboxToUI("ModUtils_Telemetry", "Telemetry enabled", true);
            mi.AddLabelToUI("Permit ModUI to load. This will cause you to have 2 'Mods' buttons but will make some older mods that require BrennfuchS's ModUI to work");
            DontDisableModUI = mi.AddCheckboxToUI("ModUtils_ModUIEnable", "Enable ModUI loading (Requires game restart)", false);
            RandomBG = mi.AddCheckboxToUI("ModUtils_RandomBG", "Enable random main menu background", true);
            UrpCompatibility = mi.AddCheckboxToUI("ModUtils_UrpCompatibility", "Enable URP compatibility layer (For old mods)", true);
            ForcedPaintQuality = mi.AddDropdownToUI("ModUtils_paintQuality", "Force paint quality", new string[] { "None", "Very low", "Low", "Medium", "High", "Very high" }, 0);
            mi.AddSpacerToUI();
            mi.AddSeparatorToUI();
            mi.AddHeaderToUI("Settings for developers");
            DevUIEnabled = mi.AddCheckboxToUI("ModUtils_DevUI", "Enable DeveloperUI", false);
            mi.AddSpacerToUI();

            // Developer binds
            mi.AddSmallHeaderToUI("Transparent editor keybinds");
            mi.AddLabelToUI("Switch between position and rotation:");
            // General
            TogglePosRot = mi.AddKeybindToUI("ModUtils_TransparentEditor_Toggle", KeyCode.Keypad0);
            mi.AddLabelToUI("Multiplier:");
            Multiplier = mi.AddKeybindToUI("ModUtils_TransparentEditor_Multp", KeyCode.LeftShift);

            // X axis
            mi.AddLabelToUI("X- :");
            XMinus = mi.AddKeybindToUI("ModUtils_TransparentEditor_X-", KeyCode.Keypad1);
            mi.AddLabelToUI("X90 :");
            X90 = mi.AddKeybindToUI("ModUtils_TransparentEditor_X90", KeyCode.Keypad2);
            mi.AddLabelToUI("X+ :");
            XPlus = mi.AddKeybindToUI("ModUtils_TransparentEditor_X+", KeyCode.Keypad3);

            // Y axis
            mi.AddLabelToUI("Y- :");
            YMinus = mi.AddKeybindToUI("ModUtils_TransparentEditor_Y-", KeyCode.Keypad4);
            mi.AddLabelToUI("Y90 :");
            Y90 = mi.AddKeybindToUI("ModUtils_TransparentEditor_Y90", KeyCode.Keypad5);
            mi.AddLabelToUI("Y+ :");
            YPlus = mi.AddKeybindToUI("ModUtils_TransparentEditor_Y+", KeyCode.Keypad6);

            // Z axis
            mi.AddLabelToUI("Z- :");
            ZMinus = mi.AddKeybindToUI("ModUtils_TransparentEditor_Z-", KeyCode.Keypad7);
            mi.AddLabelToUI("Z90 :");
            Z90 = mi.AddKeybindToUI("ModUtils_TransparentEditor_Z90", KeyCode.Keypad8);
            mi.AddLabelToUI("Z+ :");
            ZPlus = mi.AddKeybindToUI("ModUtils_TransparentEditor_Z+", KeyCode.Keypad9);

            mi.AddLabelToUI("Preview cube toggle: ");
            PreviewCube = mi.AddKeybindToUI("ModUtils_TransparentEditor_Cube", KeyCode.Z);

            mi.AddSpacerToUI();
            mi.AddSmallHeaderToUI("Credits");
            mi.AddLabelToUI("ModUtils (Also known as SimplePartLoader) was developed by Federico Arredondo (fedes.uy). Special thanks to BrennFuchS, ESTBanana, Reliant Robin, Horsey4, mbdriver and Jim Goose");

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
                if (modUiRemove && !DontDisableModUI.Checked)
                {
                    modUiRemove.SetActive(false);
                }


                if (RandomBG.Checked)
                {
                    try
                    {
                        HttpResponseMessage response = Client.GetAsync("/v1/menu").Result;
                        response.EnsureSuccessStatusCode();

                        // Image load
                        imageBytes = response.Content.ReadAsByteArrayAsync().Result;
                    }
                    catch (Exception ex)
                    {
                        CustomLogger.AddLine("RandomBG", "Failed to load random background!");
                        CustomLogger.AddLine("RandomBG", ex);
                    }
                }
            }

            UI_Mods.SetActive(true);
            UI_Mods.transform.Find("OpenMods").localScale = Vector3.one;

            if(RandomBG.Checked) new GameObject("test").AddComponent<BackgroundDelayChange>();
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

            // If Urp compatibility is enabled, we can try to convert old materials to the new system.
            if (ModMain.UrpCompatibility.Checked)
            {
                foreach(var part in PartManager.prefabGenParts)
                {
                    if (part.CarProps == null || part.CarProps.PrefabName == null || part.CarProps.PrefabName == "")
                        continue;

                    MeshRenderer[] renderers = part.Prefab.GetComponentsInChildren<MeshRenderer>();
                    bool changesApplied = false;

                    foreach (var renderer in renderers)
                    {
                        Material[] partMats = renderer.materials;

                        foreach (Material mat in partMats)
                        {
                            if (mat && (mat.shader.name == "Standard" || mat.shader.name == "Azerilo/Double Sided Standard" || mat.shader.name == "Standard (Specular setup)"))
                            {
                                changesApplied = true;
                                var color = mat.color;
                                var texture = mat.mainTexture;

                                bool doubleSided = mat.shader.name == "Azerilo/Double Sided Standard";
                                mat.shader = Shader.Find("Universal Render Pipeline/Lit");

                                if (texture)
                                {
                                    mat.SetTexture("_BaseMap", texture);
                                }
                                else
                                {
                                    mat.SetTexture("_BaseMap", null);
                                }

                                mat.SetColor("_BaseColor", color);
                                mat.SetFloat("_Cull", doubleSided ? 0 : 2);
                            }
                        }

                        renderer.materials = partMats;

                        if (changesApplied)
                        {
                            CustomLogger.AddLine("URPCompatibility", $"Part {part.Name} ({part.CarProps.PrefabName}) materials were converted to URP compatible materials.");
                        }
                    }
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

        public override void OnSaveSystemSave(SaveSystem saver, bool isBarn)
        {
            if (ModUtils.GetPlayerTools().MapMagic)
                return;

            CustomSaverHandler.Save(saver, isBarn);
            if (!isBarn)
            {
                FurnitureManager.SaveFurniture(saver);
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

                else if(!ModUtils.PlayerTools.EscMenu.activeSelf && UI_Mods.activeSelf && ModUtilsUI.currentlyEditingKeybind == null)
                {
                    UI_Mods.SetActive(false);
                }

                /*
                if(Input.GetKeyDown(KeyCode.DownArrow))
                {
                    Debug.Log("Report of car");
                    NWH.VehiclePhysics2.VehicleController veh = ModUtils.GetPlayerCurrentCar().GetComponent<NWH.VehiclePhysics2.VehicleController>();

                    Type type = veh.GetType();

                    IEnumerable<PropertyInfo> pinfos = type.GetProperties(Extension.bindingFlags);
                    IEnumerable<FieldInfo> finfos = type.GetFields(Extension.bindingFlags);

                    foreach (var pinfo in pinfos)
                    {
                        if (pinfo.GetType() == typeof(object)) continue;
                        Debug.Log($"P{pinfo.Name} - {pinfo.GetValue(veh)}");

                        if (pinfo.GetValue(veh) == null || pinfo.GetValue(veh).GetType() == typeof(object)) continue;

                        Type t = pinfo.GetValue(veh).GetType();
                        bool isPrimitiveType = t.IsPrimitive || t.IsValueType || (t == typeof(string));
                        if(!isPrimitiveType)
                        {
                            MyObjectSerialize(pinfo.GetValue(veh), 0);
                        }
                    }

                    foreach (var finfo in finfos)
                    {
                        if (finfo.GetType() == typeof(object)) continue;
                        Debug.Log($"F{finfo.Name} - {finfo.GetValue(veh)}");

                        if (finfo.GetValue(veh) == null || finfo.GetValue(veh).GetType() == typeof(object)) continue;

                        Type t = finfo.GetValue(veh).GetType();
                        bool isPrimitiveType = t.IsPrimitive || t.IsValueType || (t == typeof(string) || t != typeof(IEnumerable));
                        if (!isPrimitiveType)
                        {
                            MyObjectSerialize(finfo.GetValue(veh), 0);
                        }
                    }
                }*/
            }
        }
        /*
        public void MyObjectSerialize(object obj, int iterations)
        {
            if (iterations > 15)
            {
                Debug.Log("end");
                return;
            }

            Type type = obj.GetType();
            if (type == null || type == typeof(object)) return;

            Debug.Log($"-------- {type.Name} ");
            IEnumerable<PropertyInfo> pinfos = type.GetProperties(Extension.bindingFlags);
            IEnumerable<FieldInfo> finfos = type.GetFields(Extension.bindingFlags);

            foreach (var pinfo in pinfos)
            {
                if (pinfo.GetType() == typeof(object)) continue;
                Debug.Log($"P{pinfo.Name} - {pinfo.GetValue(obj)}");

                if (pinfo.GetValue(obj) == null || pinfo.GetValue(obj).GetType() == typeof(object)) continue;

                Type t = pinfo.GetValue(obj).GetType();
                bool isPrimitiveType = t.IsPrimitive || t.IsValueType || (t == typeof(string));
                if (!isPrimitiveType)
                {
                    MyObjectSerialize(pinfo.GetValue(obj), iterations + 1);
                }
            }

            foreach (var finfo in finfos)
            {if (finfo.GetType() == typeof(object)) continue;
                Debug.Log($"F{finfo.Name} - {finfo.GetValue(obj)}");

                if (finfo.GetValue(obj) == null || finfo.GetValue(obj).GetType() == typeof(object)) continue;

                Type t = finfo.GetValue(obj).GetType();
                bool isPrimitiveType = t.IsPrimitive || t.IsValueType || (t == typeof(string));
                if (!isPrimitiveType)
                {
                    MyObjectSerialize(finfo.GetValue(obj), iterations+1);
                }
            }
        }
        */
        public override void OnNewMapLoad()
        {
            BuildableManager.OnNewMapEnabled();
        }

        public void LoadSettings()
        {
            // Enable heartbeat
            KeepAlive.GetInstance().Ready();
        }
    }
}