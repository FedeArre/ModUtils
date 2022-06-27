using Autoupdater.Objects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using UnityEngine;
using UnityEngine.UI;

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

        // Autoupdater
        const string API_URL = "https://mygaragemod.xyz/api";
        GameObject UI_Prefab, UI_Error_Prefab, UI;
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

            // Mod delete
            string ModsFolderPath = Application.dataPath + "/../Mods/";
            foreach(string s in modsToDelete)
            {
                if (File.Exists(ModsFolderPath + s))
                {
                    File.Delete(ModsFolderPath + s);
                    Debug.Log($"[ModUtils]: Mod {s} has been replaced by ModUtils");
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
            UI_Prefab.GetComponent<Canvas>().sortingOrder = 1; // Fixes canva disappearing after a bit.
            
            UI_Error_Prefab.GetComponent<Canvas>().sortingOrder = 1;
            AutoupdaterBundle.Unload(false);
        }

        public override void OnMenuLoad()
        {
            if (!MenuFirstLoad)
            {
                MenuFirstLoad = true;
                return;
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
                Debug.LogError(API_URL);
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

                    if (jsonObj.Count > 0)
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
                Debug.Log("Error occured while trying to fetch updates, error: " + ex.ToString());
                GameObject.Instantiate(UI_Error_Prefab);
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
            Debug.Log("UI button yes: Path is " + autoupdaterPath);

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