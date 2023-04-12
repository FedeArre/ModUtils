using Autoupdater.Objects;
using Newtonsoft.Json;
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

namespace SimplePartLoader
{
    public class ModMain : Mod
    {
        // Looking for docs? https://fedearre.github.io/my-garage-modding-docs/
        public override string ID => "ModUtils";
        public override string Name => "ModUtils";
        public override string Author => "Federico Arredondo";
        public override string Version => "v1.2.0"; 
        
        bool TESTING_VERSION_REMEMBER = true;
        string TESTING_VERSION_NUMBER = "1.3-pb3-furniture-fix";
        
        public override byte[] Icon => Properties.Resources.SimplePartLoaderIcon;

        // Autoupdater
        public const string API_URL = "https://modding.fedes.uy/api";
        GameObject UI_Prefab, UI_Error_Prefab, UI, UI_BrokenInstallation_Prefab, UI_DeveloperLogEnabled_Prefab;
        AssetBundle AutoupdaterBundle;
        bool MenuFirstLoad;

        // ModUtils
        Transform PlayerTransform;
        bool PlayerOnCar;
        
        // Mod delete
        string[] modsToDelete = { "Extra Buildings.dll", "Autoupdater.dll" };
        
        // Mod shop
        AssetBundle Bundle;
        GameObject ModShopPrefab;
        Material FloorMat;

        public ModMain()
        {
            Debug.Log("ModUtils is loading - Version: " + Version);
            Debug.Log("Developed by Federico Arredondo - www.github.com/FedeArre");
            if(TESTING_VERSION_REMEMBER)
                Debug.Log($"This is a testing version ({TESTING_VERSION_NUMBER}) - remember to report bugs and send feedback");

            // Mod delete
            string ModsFolderPath = Application.dataPath + "/../Mods/";
            foreach(string s in modsToDelete)
            {
                if (File.Exists(ModsFolderPath + s))
                {
                    File.Delete(ModsFolderPath + s);
                    Debug.Log($"[ModUtils/Legacy]: Mod {s} has been replaced by ModUtils");
                }
            }

            if (Directory.Exists(ModsFolderPath + "Autoupdater/"))
            {
                Directory.Delete(ModsFolderPath + "Autoupdater/", true);
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
            
            UI_Prefab.GetComponent<Canvas>().sortingOrder = 1; // Fixes canva disappearing after a bit.
            UI_Error_Prefab.GetComponent<Canvas>().sortingOrder = 1;

            PaintingSystem.BackfaceShader = AutoupdaterBundle.LoadAsset<Shader>("BackfaceShader");
            PaintingSystem.CullBaseMaterial = AutoupdaterBundle.LoadAsset<Material>("testMat");
            AutoupdaterBundle.Unload(false);

            ModUtils.SetupSteamworks();
            MainCarGenerator.BaseSetup();
        }

        public override void OnMenuLoad()
        {
            string autoupdaterDirectory = Path.Combine(Application.dataPath, "..\\Mods\\NewAutoupdater");
            string autoupdaterPath = autoupdaterDirectory + "\\Autoupdater.exe";

            if (!MenuFirstLoad)
            {
                MenuFirstLoad = true;
                Debug.Log("[ModUtils/Main]: Printing mod list.");
                foreach(Mod m in ModLoader.mods)
                {
                    Debug.Log($"{m.Name} (ID: {m.ID}) - Version {m.Version}");
                }

                // Enable heartbeat

                if (!File.Exists(autoupdaterPath + "\\disableStatus.txt"))
                    KeepAlive.GetInstance().Ready();
                
                return;
            }
            Debug.Log("[ModUtils/Autoupdater]: Autoupdater check");

            // Check for broken ModUtils autoupdater installation
            bool brokenInstallation = false;
            
            if(!Directory.Exists(autoupdaterDirectory) || !File.Exists(autoupdaterPath))
            {
                Debug.Log("[ModUtils/Autoupdater/Error]: Autoupdater is not installed properly!");
                GameObject.Instantiate(UI_BrokenInstallation_Prefab);
                brokenInstallation = true;
            }

            if(SPL.DEVELOPER_LOG)
            {
                GameObject.Instantiate(UI_DeveloperLogEnabled_Prefab);
            }
            
            JSON_ModList jsonList = new JSON_ModList();
            foreach (Mod mod in ModLoader.mods)
            {
                JSON_Mod jsonMod = new JSON_Mod();

                jsonMod.modId = mod.ID;
                jsonMod.version = mod.Version;

                jsonList.mods.Add(jsonMod);
            }

            try
            {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(API_URL + "/mods");
                Debug.LogError("Current API url: " + API_URL);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Accept = "application/json";
                httpWebRequest.Method = "POST";

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    streamWriter.Write(JsonConvert.SerializeObject(jsonList));
                }

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    List<JSON_Mod_API_Result> jsonObj = JsonConvert.DeserializeObject<List<JSON_Mod_API_Result>>(result);

                    if (jsonObj.Count > 0 && !brokenInstallation)
                    {
                        // Updates available.
                        UI = GameObject.Instantiate(UI_Prefab);
                        foreach (Button btt in UI.GetComponentsInChildren<Button>())
                        {
                            if (btt.name == "ButtonNo")
                            {
                                btt.onClick.AddListener(UI_ButtonNo);
                            }
                            else if (btt.name == "ButtonYes")
                            {
                                btt.onClick.AddListener(UI_ButtonYes);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Log("[ModUtils/Autoupdater/Error]: Error occured while trying to fetch updates, error: " + ex.ToString());
                GameObject.Instantiate(UI_Error_Prefab);
            }
        }

        public override void OnLoad()
        {
            ModUtils.OnLoadCalled();
            PartManager.OnLoadCalled();
            FurnitureManager.SetupFurniture();
            
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

            GameObject shop = GameObject.Instantiate(ModShopPrefab, new Vector3(722.5838f, 38.12f, -189.3593f), Quaternion.Euler(new Vector3(0, 90f, 0)));
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

        public override void OnSaveSystemSave(SaveSystem saver, bool isBarn)
        {
            if (ModUtils.GetPlayerTools().MapMagic)
                return;
            
            if (isBarn)
            {

            }
            else
            {
                FurnitureManager.SaveFurniture(saver);
            }
        }

        public override void OnSaveSystemLoad(SaveSystem saver, bool isBarn)
        {
            if (ModUtils.GetPlayerTools().MapMagic)
                return;
            
            if (isBarn)
            {

            }
            else
            {
                FurnitureManager.LoadFurniture(saver);
            }
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

        // Autoupdater

        public void UI_ButtonNo()
        {
            if (UI)
                GameObject.Destroy(UI);
        }

        public void UI_ButtonYes()
        {
            string autoupdaterPath = Path.Combine(Application.dataPath, "..\\Mods\\NewAutoupdater\\Autoupdater.exe");
            
            if (File.Exists(autoupdaterPath))
            {
                GameObject.Destroy(UI);
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                startInfo.FileName = autoupdaterPath;
                System.Diagnostics.Process.Start(startInfo);
                Application.Quit(0);
            }
        }
    }
}