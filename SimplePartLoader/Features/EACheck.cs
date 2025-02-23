using Autoupdater.Objects;
using Newtonsoft.Json;
using SimplePartLoader.Features.Auto
    
    
    
    ;
using SimplePartLoader.Features.UI;
using SimplePartLoader.Objects.DTO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Policy;
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

        string DownloadFolder = "";

        List<JSON_Mod_API_Result> AutoupdaterResult;
        Queue<JSON_Mod_API_Result> Data;

        JSON_Mod_API_Result CurrentDownloadingMod;
        Text progressText;
        void Start()
        {
            DownloadFolder = Application.dataPath + "/../Mods/ModUtilsUpdating";
        }

        void Update()
        {
            if (checkDone)
                return;

            frameCount++;

            if (!SteamManager.Initialized)
                return;

            ulong steamID = Steamworks.SteamUser.GetSteamID().m_SteamID; // User SteamID
            CustomLogger.AddLine("EACheck", $"Identified user: " + steamID);
            CustomLogger.AddLine("EACheck", $"App build id: " + Steamworks.SteamApps.GetAppBuildId());

            KeepAlive.GetInstance().UpdateJsonList(Steamworks.SteamApps.GetAppBuildId());

            // Load keys
            string ModsFolderPath = Application.dataPath + "/../Mods/";
            string CachedFolderPath = Application.dataPath + "/../Mods/LockedCache";

            if(!Directory.Exists(CachedFolderPath))
            {
                CustomLogger.AddLine("EACheck", "Creating cache folder as it does not exist");
                Directory.CreateDirectory(CachedFolderPath);
            }

            Dictionary<string, string> foundKeys = new Dictionary<string, string>();
            Dictionary<string, KeyAnswer> aesKeys = new Dictionary<string, KeyAnswer>();
            string[] modKeys = Directory.GetFiles(ModsFolderPath, "*.locked");
            string[] cachedMods = Directory.GetFiles(CachedFolderPath, "*.modutilscache");

            if ((modKeys.Length > 0) && !ModMain.EA_Enabled.Checked)
            {
                ErrorMessageHandler.GetInstance().EarlyAccessMod = true;
                CustomLogger.AddLine("EACheck", $"Locked mods found but loading of them is not allowed!");
            }

            // If we have user consent to load EA/PL mods, we first read all the keys.
            if(ModMain.EA_Enabled.Checked)
            {
                try
                {
                    foreach (string file in modKeys)
                    {
                        string contents = File.ReadAllText(file);
                        if (contents.StartsWith("MODUTILS-22-LKM-")) // We know is a valid key, we add it to our tracker.
                        {
                            CustomLogger.AddLine("EACheck", $"Found key on {Path.GetFileName(file)}");
                            foundKeys.Add(file, contents);
                        }
                    }
                }
                catch(Exception ex)
                {
                    CustomLogger.AddLine("EACheck", $"Error trying to read key of a file.");
                    CustomLogger.AddLine("EACheck", ex);
                }

                // We have all the keys, now we ask the API if they are still valid.
                if(foundKeys.Count > 0)
                {
                    foreach(var item in foundKeys)
                    {
                        KeyValidationDTO key = new KeyValidationDTO()
                        {
                            Key = item.Value,
                            SteamID = steamID.ToString().Trim()
                        };
                        CustomLogger.AddLine("EACheck", $"Trying to validate {Path.GetFileName(item.Key)} key.");

                        var response = ModMain.Client.PostAsync("v1/locked/key-auth", new StringContent(JsonConvert.SerializeObject(key), Encoding.UTF8, "application/json")).Result;

                        if (response.IsSuccessStatusCode)
                        {
                            CustomLogger.AddLine("EACheck", $"Recieved 200 for {Path.GetFileNameWithoutExtension(item.Key)} key.");
                            var responseBody = response.Content.ReadAsStringAsync().Result;
                            var jsonResponse = JsonConvert.DeserializeObject<KeyAnswerDTO>(responseBody);

                            KeyAnswer keyAnswer = new KeyAnswer()
                            {
                                ModId = jsonResponse.ModId,
                                Checksum = jsonResponse.Checksum,
                                Key = Convert.FromBase64String(jsonResponse.Key),
                                IV = Convert.FromBase64String(jsonResponse.IV),
                                PublicKey = key.Key
                            };

                            aesKeys.Add(jsonResponse.ModId, keyAnswer);
                            CustomLogger.AddLine("EACheck", $"Succesfully added {Path.GetFileName(item.Key)} key.");
                        }
                        else
                        {
                            if (response.StatusCode == HttpStatusCode.Unauthorized)
                            {
                                ErrorMessageHandler.GetInstance().DisabledModList.Add(Path.GetFileName(item.Key));

                                CustomLogger.AddLine("EACheck", $"Recieved 401 - User is not allowed for this mod");
                            }
                            else if (response.StatusCode == HttpStatusCode.NotFound)
                            {
                                ErrorMessageHandler.GetInstance().UpdateRequired.Add(Path.GetFileName(item.Key));

                                CustomLogger.AddLine("EACheck", $"Recieved 404 - Update may be required.");
                            }
                            else
                            {
                                ErrorMessageHandler.GetInstance().DisabledModList.Add(Path.GetFileName(item.Key));
                                CustomLogger.AddLine("EACheck", $"Error trying to validate key of {item.Key}");
                                CustomLogger.AddLine("EACheck", $"{response.StatusCode}");
                                CustomLogger.AddLine("EACheck", response.Content.ReadAsStringAsync().Result);
                            }
                        }
                    }

                    // aesKey is now populated. checksum cached.
                    CustomLogger.AddLine("EACheck", "Cache checksum check");
                    List<string> updateModPathList = new List<string>();

                    // load all files to be downloaded
                    foreach (var item in aesKeys)
                    {
                        string modPath = Path.Combine(CachedFolderPath, $"{item.Key}.modutilscache");
                        if (!cachedMods.Contains(modPath))
                        {
                            updateModPathList.Add(modPath);
                        }
                    }

                    foreach (string file in cachedMods)
                    {
                        var checksum = GenerateChecksum(file);
                        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file);
                        string modPath = Path.Combine(CachedFolderPath, $"{fileNameWithoutExtension}.modutilscache");

                        CustomLogger.AddLine("EACheck", $"Checking {fileNameWithoutExtension}.modutilscache");

                        if (aesKeys.ContainsKey(fileNameWithoutExtension))
                        {
                            var aesChecksum = aesKeys[fileNameWithoutExtension].Checksum;

                            if (aesChecksum == checksum)
                            {
                                CustomLogger.AddLine("EACheck", $"Checksum of {Path.GetFileName(file)} matches, not updating.");

                                if (updateModPathList.Contains(modPath))
                                {
                                    updateModPathList.Remove(modPath);
                                    CustomLogger.AddLine("EACheck", $"Removed from update list.");
                                }
                            }
                            else
                            {
                                CustomLogger.AddLine("EACheck", $"Checksum mismatch for {fileNameWithoutExtension}.modutilscache ({checksum})");
                               
                                if (!updateModPathList.Contains(modPath))
                                {
                                    updateModPathList.Add(modPath);
                                    CustomLogger.AddLine("EACheck", $"Added to update list due to checksum mismatch.");
                                }
                            }
                        }
                        else
                        {
                            CustomLogger.AddLine("EACheck", $"{Path.GetFileName(file)} cache file found but no mod key available.");
                        }
                    }

                    CustomLogger.AddLine("EACheck", $"Cache download count is {updateModPathList.Count}");
                    if (updateModPathList.Count > 0)
                    {
                        // update mods
                        foreach (var item in updateModPathList)
                        {
                            if(File.Exists(item))
                                File.Delete(item); // delete existing file

                            if(aesKeys.ContainsKey(Path.GetFileNameWithoutExtension(item)))
                            {
                                KeyAnswer key = aesKeys[Path.GetFileNameWithoutExtension(item)];
                                string path = Path.Combine(CachedFolderPath, $"{key.ModId}.modutilscache");

                                try
                                {
                                    using (var response = ModMain.Client.GetAsync($"v1/locked/download?ModId={key.ModId}", HttpCompletionOption.ResponseHeadersRead).Result)
                                    {
                                        response.EnsureSuccessStatusCode();

                                        using (var httpStream = response.Content.ReadAsStreamAsync().Result)
                                        using (var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
                                        {
                                            httpStream.CopyTo(fileStream);
                                            CustomLogger.AddLine("EACheck", $"{Path.GetFileName(item)} succesfully downloaded from MuS");
                                        }
                                    }
                                }
                                catch(Exception ex)
                                {
                                    CustomLogger.AddLine("EACheck", $"{Path.GetFileName(item)} cache file was being downloaded but an issue occured.");
                                    CustomLogger.AddLine("EACheck", ex);
                                }
                            }
                            else
                            {
                                CustomLogger.AddLine("EACheck", $"{Path.GetFileName(item)} cache file found and set for update without available key.");
                            }
                        }
                    }

                    // load cached
                    CustomLogger.AddLine("EACheck", "Cache folder: " + CachedFolderPath);
                    cachedMods = Directory.GetFiles(CachedFolderPath, "*.modutilscache");
                    foreach (string file in cachedMods)
                    {
                        var modId = Path.GetFileNameWithoutExtension(file);
                        if (aesKeys.ContainsKey(modId))
                        {
                            KeyAnswer key = aesKeys[modId];
                            byte[] fileByteEncrypted = File.ReadAllBytes(file);
                            byte[] fileBytes = null;

                            try
                            {
                                CustomLogger.AddLine("EACheck", $"Found key of {key.ModId}.");
                                fileBytes = Decrypt(fileByteEncrypted, key.Key, key.IV);
                                CustomLogger.AddLine("EACheck", $"D: {modId}");
                            }
                            catch (Exception ex)
                            {
                                CustomLogger.AddLine("EACheck", $"Major error trying to decrypt {modId}");
                                CustomLogger.AddLine("EACheck", ex);
                                continue;
                            }

                            try
                            {
                                Type[] types = Assembly.Load(fileBytes).GetTypes();
                                Type typeFromHandle = typeof(Mod);
                                for (int i = 0; i < types.Length; i++)
                                {
                                    if (typeFromHandle.IsAssignableFrom(types[i]))
                                    {
                                        CustomLogger.AddLine("EACheck", $"Trying to start mod {modId}");
                                        Mod m = (Mod)Activator.CreateInstance(types[i]);
                                        ModLoader.mods.Add(m);
                                        m.OnMenuLoad();
                                        break;
                                    }
                                }
                            }
                            catch(Exception ex)
                            {
                                ErrorMessageHandler.GetInstance().DisabledModList.Add(modId + " (FATAL)");
                                CustomLogger.AddLine("EACheck", $"Fatal error on mod load of " + modId);
                                CustomLogger.AddLine("EACheck", ex);
                            }
                        }
                        else
                        {
                            CustomLogger.AddLine("EACheck", $"{modId} was ready to be loaded but no key available.");
                        }
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

            SettingSaver.LoadSettings(); // Reload settings in case an EA mod is using settings. Great edge case.
            ModUtilsUI.ModCardLoad();

            // Autoupdating stuff goes here too now!
            KeepAlive.GetInstance().UpdateJsonList(Steamworks.SteamApps.GetAppBuildId()); // Update list so EA mods show telemetry

            ModListDTO jsonList = new ModListDTO(Steamworks.SteamApps.GetAppBuildId());
            foreach (Mod mod in ModLoader.mods)
            {
                ModDTO jsonMod = new ModDTO();

                jsonMod.modId = mod.ID;
                jsonMod.version = mod.Version;

                jsonList.mods.Add(jsonMod);
            }

            if (ModMain.EA_Enabled.Checked)
                jsonList.SteamId = Steamworks.SteamUser.GetSteamID().m_SteamID;

            try
            {
                var content = new StringContent(JsonConvert.SerializeObject(jsonList), Encoding.UTF8, "application/json");
                var result = ModMain.Client.PostAsync("v1/updating/mods", content).Result;
                string contents = result.Content.ReadAsStringAsync().Result;

                AutoupdaterResult = JsonConvert.DeserializeObject<List<JSON_Mod_API_Result>>(contents);

                // First check for unsupported mods.
                List<JSON_Mod_API_Result> unsupportedMods = new List<JSON_Mod_API_Result>();
                if (AutoupdaterResult.Count > 0)
                {
                    foreach (var item in AutoupdaterResult)
                    {
                        if (item.unsupported) unsupportedMods.Add(item);
                    }

                    foreach (var item in unsupportedMods) AutoupdaterResult.Remove(item);
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

                    CustomLogger.AddLine("Autoupdating", $"Found updates for {names}");
                }

                if (unsupportedMods.Count > 0)
                {
                    foreach (var item in unsupportedMods)
                    {
                        ErrorMessageHandler.GetInstance().UnsupportedModList.Add(item.mod_name);
                    }
                }

            }
            catch (Exception ex)
            {
                CustomLogger.AddLine("Autoupdating", ex);
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
                }
                catch (Exception ex)
                {
                    progressText.text = "An error occurred, please report this and attach the log file!";
                    CustomLogger.AddLine("ClientHelper", ex);
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
                    CustomLogger.AddLine("ClientHelper", $"Helper could not be started");
                    return;
                }
            }
            catch(Exception ex)
            {
                progressText.text = "Failed to start helper, error: " + ex.Message;
                CustomLogger.AddLine("ClientHelper", ex);
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
                CustomLogger.AddLine("ClientHelper", ex);
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
                CustomLogger.AddLine("EACheckDownloads", e.Error);
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

        public string GenerateChecksum(string filePath)
        {
            using (MD5 md5Hash = MD5.Create())
            using (var stream = File.OpenRead(filePath))
            {
                byte[] hashBytes = md5Hash.ComputeHash(stream);

                StringBuilder builder = new StringBuilder();
                foreach (byte b in hashBytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }

        public static byte[] Decrypt(byte[] cipherText, byte[] key, byte[] iv)
        {
            Aes aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            
            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            MemoryStream ms = new MemoryStream(cipherText);
            CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            MemoryStream output = new MemoryStream();

            cs.CopyTo(output);
            byte[] decryptedBytes = output.ToArray();

            // Cleanup
            output.Close();
            cs.Close();
            ms.Close();
            decryptor.Dispose();
            aes.Dispose();

            return decryptedBytes;
        }
    }
}
