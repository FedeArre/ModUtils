using Autoupdater.Objects;
using Newtonsoft.Json;
using SimplePartLoader.Features.Autoupdating;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

namespace SimplePartLoader
{
    [DisallowMultipleComponent]
    internal class EACheck : MonoBehaviour
    {
        internal int frameCount = 0;
        internal bool checkDone = false;

        GameObject UI;

        string DownloadFolder = Application.dataPath + "/../Mods/ModUtilsUpdating";

        List<JSON_Mod_API_Result> AutoupdaterResult;
        Queue<JSON_Mod_API_Result> Data;

        JSON_Mod_API_Result CurrentDownloadingMod;
        Text progressText;

        void Update()
        {
            if (checkDone)
                return;

            frameCount++;
            if (!SteamManager.Initialized)
                return;

            ulong steamID = Steamworks.SteamUser.GetSteamID().m_SteamID;
            Debug.Log("[ModUtils/EACheck]: Identified user: " + steamID);
            Debug.Log("[ModUtils/Steam]: APP build id: " + Steamworks.SteamApps.GetAppBuildId());
            KeepAlive.GetInstance().UpdateJsonList(Steamworks.SteamApps.GetAppBuildId());

            // Load keys
            Dictionary<string, string> foundKeys = new Dictionary<string, string>();
            string ModsFolderPath = Application.dataPath + "/../Mods/";
            string[] files = Directory.GetFiles(ModsFolderPath);

            for (int i = 0; i < files.Length; i++)
            {
                try
                {
                    string stuff = File.ReadAllText(files[i]);
                    if (stuff.StartsWith("MDU783-"))
                    {
                        foundKeys.Add(files[i], stuff.Substring(7));
                    }
                }
                catch (Exception ex)
                {
                    Debug.Log("[ModUtils/EACheck]: Something is going wrong internally on mod data reading: " + ex.Message);
                }
            }

            if(!ModMain.EnableEarlyAccess.Value && foundKeys.Count != 0)
            {
                ErrorMessageHandler.GetInstance().EarlyAccessMod = true;
            }

            if (ModMain.EnableEarlyAccess.Value)
            {
                
                // If we have keys, we now start loading the mods
                foreach (var item in foundKeys)
                {
                    try
                    {
                        var httpWebRequest = (HttpWebRequest)WebRequest.Create(ModMain.API_URL + "/eachecknew");

                        httpWebRequest.ContentType = "application/json";
                        httpWebRequest.Accept = "application/json";
                        httpWebRequest.Method = "POST";

                        EarlyAccessObjectModel eamo = new EarlyAccessObjectModel();
                        eamo.Key = item.Value;
                        eamo.SteamId = steamID + "";

                        using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                        {
                            streamWriter.Write(JsonConvert.SerializeObject(eamo));
                        }

                        using (HttpWebResponse response = (HttpWebResponse)httpWebRequest.GetResponse())
                        {
                            if (response.StatusCode == HttpStatusCode.OK)
                            {
                                using (MemoryStream memoryStream = new MemoryStream())
                                using (Stream responseStream = response.GetResponseStream())
                                {
                                    int bufferSize = 4096;
                                    byte[] buffer = new byte[bufferSize];
                                    int bytesRead;
                                    while ((bytesRead = responseStream.Read(buffer, 0, bufferSize)) > 0)
                                    {
                                        memoryStream.Write(buffer, 0, bytesRead);
                                        if (bytesRead < bufferSize)
                                        {
                                            break;
                                        }
                                    }

                                    // Load the assembly from the memory stream
                                    byte[] assemblyBytes = memoryStream.ToArray();
                                    Type[] types = Assembly.Load(assemblyBytes).GetTypes();
                                    Type typeFromHandle = typeof(Mod);
                                    for (int i = 0; i < types.Length; i++)
                                    {
                                        if (typeFromHandle.IsAssignableFrom(types[i]))
                                        {
                                            Mod m = (Mod)Activator.CreateInstance(types[i]);
                                            ModLoader.mods.Add(m);
                                            m.OnMenuLoad();
                                        }
                                    }
                                }

                                Debug.Log($"[ModUtils/EACheck]: Succesfully loaded " + Path.GetFileName(item.Key));
                            }
                            else
                            {
                                ErrorMessageHandler.GetInstance().DisabledModList.Add(Path.GetFileName(item.Key));
                                Debug.LogWarning($"[ModUtils/EACheck/Error]: Could not load " + Path.GetFileName(item.Key));
                                Debug.LogWarning($"[ModUtils/EACheck/Error]: Status code: " +  response.StatusCode);
                            }
                        }
                    }
                    catch(Exception ex)
                    {
                        ErrorMessageHandler.GetInstance().DisabledModList.Add(Path.GetFileName(item.Key + " (FATAL)"));
                        Debug.LogError("[ModUtils/EACheck/Error]: An error occured checking a file. " + ex.Message);
                        Debug.LogError("[ModUtils/EACheck/Error]: " + ex.StackTrace);
                    }
                }
            }

            foreach (ModInstance mi in ModUtils.ModInstances)
            {
                if (mi.RequiresSteamCheck)
                {
                    mi.Check(steamID);
                }
            }
            
            // Autoupdating stuff goes here too now!
            JSON_ModList jsonList = new JSON_ModList(Steamworks.SteamApps.GetAppBuildId());
            foreach (Mod mod in ModLoader.mods)
            {
                JSON_Mod jsonMod = new JSON_Mod();

                jsonMod.modId = mod.ID;
                jsonMod.version = mod.Version;

                jsonList.mods.Add(jsonMod);
            }

            try
            {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(ModMain.API_URL + "/mods");
                Debug.LogError("Current API url: " + ModMain.API_URL);
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

                    AutoupdaterResult = JsonConvert.DeserializeObject<List<JSON_Mod_API_Result>>(result);

                    // First check for unsupported mods.

                    List<JSON_Mod_API_Result> unsupportedMods = new List<JSON_Mod_API_Result>();
                    if (AutoupdaterResult.Count > 0)
                    {
                        Debug.Log("api result");
                        foreach (var item in AutoupdaterResult)
                        {
                            Debug.Log(item);
                            Debug.Log(item.file_name);
                            Debug.Log(item.unsupported);
                            if (item.unsupported) unsupportedMods.Add(item);
                        }

                        foreach(var item in unsupportedMods) AutoupdaterResult.Remove(item);
                    }

                    if (AutoupdaterResult.Count > 0)
                    {
                        // Updates available.
                        Data = new Queue<JSON_Mod_API_Result>();

                        JSON_Mod_API_Result fakeResult = new JSON_Mod_API_Result();
                        fakeResult.current_download_link = "https://github.com/FedeArre/ModUtils/releases/download/updatehelper/UpdaterHelper.exe";
                        fakeResult.file_name = "ModUtilsAutoupdater.exe";
                        fakeResult.mod_name = "Autoupdating helper";
                        Data.Enqueue(fakeResult);

                        AutoupdaterResult.ForEach(d => Data.Enqueue(d));

                        UI = GameObject.Instantiate(ModMain.UI_Prefab);
                        foreach (UnityEngine.UI.Button btt in UI.GetComponentsInChildren<UnityEngine.UI.Button>())
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

                        string names = "";
                        AutoupdaterResult.ForEach(x => names += $"{x.mod_name}, ");
                        names = names.Substring(0, names.Length - 2);
                        UI.transform.Find("Panel/TextMods").GetComponent<Text>().text = names;
                    }

                    if(unsupportedMods.Count > 0)
                    {
                        foreach(var item in unsupportedMods)
                        {
                            ErrorMessageHandler.GetInstance().UnsupportedModList.Add(item.mod_name);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Log("[ModUtils/Autoupdater/Error]: Error occured while trying to fetch updates, error: " + ex.ToString());
                GameObject.Instantiate(ModMain.UI_Error_Prefab);
            }
            checkDone = true;
        }


        // Autoupdater

        public void UI_ButtonNo()
        {
            if (UI)
                GameObject.Destroy(UI);
        }

        public void UI_ButtonYes()
        {
            if (UI)
                GameObject.Destroy(UI);
            
            UI = GameObject.Instantiate(ModMain.UI_Downloader_Prefab);
            progressText = UI.transform.Find("Panel/TextProgress").GetComponent<Text>();


            if (!Directory.Exists(DownloadFolder))
                Directory.CreateDirectory(DownloadFolder);
            else
            {
                Directory.Delete(DownloadFolder, true);
                Directory.CreateDirectory(DownloadFolder);
            }

            GameObject.Find("UIController/MainMenu_Canvas/MenuDefaultButtons_Canvas").SetActive(false);

            StartDownloads();
        }

        public void StartDownloads()
        {
            if(Data.Any())
            {
                WebClient client = new WebClient();
                client.DownloadProgressChanged += DownloadProgressChange;
                client.DownloadFileCompleted += DownloadFinished;

                JSON_Mod_API_Result dd = Data.Dequeue();
                CurrentDownloadingMod = dd;
                try
                {
                    client.DownloadFileAsync(new Uri(dd.current_download_link), DownloadFolder + $"\\{dd.file_name}");
                    Debug.Log($"Now downloading: " + dd.mod_name);
                }
                catch (Exception ex)
                {
                    progressText.text = "An error occurred, please report this and attach the log file!";
                    Debug.Log($"Error trying to download a mod, error: " + ex.Message);
                    Debug.Log($"Inner: " + ex.InnerException);
                }
                return;
            }

            try
            {
                if(File.Exists(DownloadFolder + "/ModUtilsAutoupdater.exe")) 
                {
                    File.SetAttributes(DownloadFolder + "/ModUtilsAutoupdater.exe", FileAttributes.Normal);
                    Process.Start(DownloadFolder + "/ModUtilsAutoupdater.exe");
                }
                else
                {
                    progressText.text = "Failed to start helper, not present. Please send your log to check what went wrong!";
                    Debug.Log($"[ModUtils/Autoupdating/Error]: Could not find helper!");
                    return;
                }
            }
            catch(Exception ex)
            {
                progressText.text = "Failed to start helper, error: " + ex.Message;
                Debug.Log($"[ModUtils/Autoupdating/Error]: Major error trying to open updater helper, data: {ex.Message} ({ex.InnerException})");
                return;
            }

            try
            {
                Process[] processes = Process.GetProcessesByName(Application.productName);
                foreach (Process process in processes)
                {
                    process.Kill();
                }
            }
            catch(Exception ex)
            {
                Debug.Log("Could not kill game, slowly exiting! - " + ex.Message);
                Environment.Exit(0);
            }
        }
        public void DownloadProgressChange(object sender, DownloadProgressChangedEventArgs e)
        {
            double bytesIn = double.Parse(e.BytesReceived.ToString());
            double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
            double percentage = bytesIn / totalBytes * 100;
            int val = int.Parse(Math.Truncate(percentage).ToString());
            if (CurrentDownloadingMod != null)
                progressText.text = $"Downloading {CurrentDownloadingMod.mod_name} - {val}%";
        }

        public void DownloadFinished(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Debug.Log($"Error on mod download finish, error: " + e.Error.ToString());
                progressText.text = "An error ocurred while downloading the file, if this happens again report this!\n\nError: " + e.Error.Message;
                return;
            }

            if (e.Cancelled)
            {
                Debug.Log($"Mod download was cancelled");
                progressText.text = "The current download has been cancelled";
            }

            StartDownloads();
        }
    }
}
