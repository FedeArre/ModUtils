using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;

namespace SimplePartLoader
{
    internal class CustomSaverHandler
    {
        public static string SavePath = $"{Application.persistentDataPath}/modSaves/";
        public static string FileName = "simplePartLoader.json";

        public static void NewGame()
        {
            try
            {
                if (File.Exists(SavePath + FileName))
                {
                    File.Delete(SavePath + FileName);
                }
            }
            catch(Exception ex)
            {
                Debug.LogError("[ModUtils/SPL/Saving/Error]: There was an issue trying to reset the custom data.");
                Debug.LogError("[ModUtils/SPL/Saving/Error]: " + ex.Message);
                return;
            }
        }

        public static void Load(SaveSystem saveSystem, bool isBarn)
        {
            SaveInside si = GameObject.Find("Props/Hangar_v2_7/SaveInside").GetComponent<SaveInside>();
            Saver saver = ModUtils.GetPlayerTools().saver;

            if (isBarn)
                LoadBarnData(saveSystem, isBarn, si);
            else
                LoadGameData(saveSystem, isBarn, saver);
            /*
            try
            {
                if (!Directory.Exists(SavePath))
                    Directory.CreateDirectory(SavePath);

                using (StreamReader r = new StreamReader(SavePath + FileName))
                {
                    string json = r.ReadToEnd();
                    if (String.IsNullOrEmpty(json))
                        return;

                    LoadedData = JsonConvert.DeserializeObject<DataWrapper>(json);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[ModUtils/SPL/Saving/Error]: There was an issue trying to load the custom data.");
                Debug.LogError($"[ModUtils/SPL/Saving/Error]: Saver: {saver} | isBarn: {isBarn}");
                Debug.LogError("[ModUtils/SPL/Saving/Error]: " + ex.Message);
                return;
            }

            foreach (SaveData sd in UnityEngine.Object.FindObjectsOfType<SaveData>())
            {
                CarProperties carProps = sd.GetComponent<CarProperties>();

                if (!carProps || carProps.ObjectNumber == 0)
                    continue;

                foreach(SavedData loadedData in LoadedData.Data)
                {
                    if(carProps.ObjectNumber == loadedData.ObjectNumber)
                    {
                        sd.Data = loadedData.Data;
                    }
                }
            }

            Debug.Log($"[ModUtils/SPL/Saving]: Loading data was succesful, {LoadedData.Data.Count} entries have been loaded");
            SPL.InvokeDataLoadedEvent();*/
        }

        internal static void LoadBarnData(SaveSystem saveSystem, bool isBarn, SaveInside si)
        {
            DataWrapper LoadedData = null;

            try
            {
                string json = saveSystem.get("ModUtils_BARN_DATA", "") as string;

                if(json != "")
                    LoadedData = JsonConvert.DeserializeObject<DataWrapper>(json);
            }
            catch (Exception ex)
            {
                Debug.LogError("[ModUtils/SPL/Saving/Error]: There was an issue trying to load the custom data.");
                Debug.LogError($"[ModUtils/SPL/Saving/Error]: SaveSystem status: {saveSystem} | isBarn: {isBarn} | saveInside {si}");
                Debug.LogError("[ModUtils/SPL/Saving/Error]: " + ex.Message);
                return;
            }

            if (LoadedData == null)
                return; // No data load!

            foreach(GameObject loadedGameObject in si.goList)
            {
                CarProperties carProps = loadedGameObject.GetComponent<CarProperties>();
                SaveData sdComponent = loadedGameObject.GetComponent<SaveData>();

                if (!sdComponent || !carProps || carProps.ObjectNumber == 0)
                    continue;

                foreach (SavedData loadedData in LoadedData.Data)
                {
                    if (carProps.ObjectNumber == loadedData.ObjectNumber)
                    {
                        sdComponent.Data = loadedData.Data;
                    }
                }
            }

            Debug.Log($"[ModUtils/SPL/Saving]: Loading barn data was succesful, {LoadedData.Data.Count} entries have been loaded");
            SPL.InvokeDataLoadedEvent();
        }
        internal static void LoadGameData(SaveSystem saveSystem, bool isBarn, Saver saver)
        {
            DataWrapper LoadedData = null;

            try
            {
                string json = saveSystem.get("ModUtils_GAME_DATA", "") as string;

                if(json == "") // Fallback to users migrating from v1.2 -> v1.3 (no-barn & no-save-slots support)
                {
                    if(File.Exists(SavePath + FileName))
                    {
                        using (StreamReader r = new StreamReader(SavePath + FileName))
                        {
                            json = r.ReadToEnd();
                            if (String.IsNullOrEmpty(json))
                                return;
                        }
                    }
                }

                if(json != "")
                    LoadedData = JsonConvert.DeserializeObject<DataWrapper>(json);
            }
            catch (Exception ex)
            {
                Debug.LogError("[ModUtils/SPL/Saving/Error]: There was an issue trying to load the custom data.");
                Debug.LogError($"[ModUtils/SPL/Saving/Error]: SaveSystem status: {saveSystem} | isBarn: {isBarn} | saver {saver}");
                Debug.LogError("[ModUtils/SPL/Saving/Error]: " + ex.Message);
                return;
            }

            if (LoadedData == null)
                return; // No data load!

            foreach (SaveData sd in UnityEngine.Object.FindObjectsOfType<SaveData>())
            {
                CarProperties carProps = sd.GetComponent<CarProperties>();

                if (!carProps || carProps.ObjectNumber == 0)
                    continue;

                foreach (SavedData loadedData in LoadedData.Data)
                {
                    if (carProps.ObjectNumber == loadedData.ObjectNumber)
                    {
                        sd.Data = loadedData.Data;
                    }
                }
            }

            Debug.Log($"[ModUtils/SPL/Saving]: Loading game data was succesful, {LoadedData.Data.Count} entries have been loaded");
            SPL.InvokeDataLoadedEvent();
        }

        public static void Save(SaveSystem saveSystem, bool isBarn)
        {
            SaveInside si = GameObject.Find("Props/Hangar_v2_7/SaveInside").GetComponent<SaveInside>();

            if (isBarn)
                SaveBarnData(saveSystem, isBarn, si);
            else
                SaveGameData(saveSystem, isBarn);

            /*
            foreach (SaveData data in UnityEngine.Object.FindObjectsOfType<SaveData>())
            {
                CarProperties carProps = data.GetComponent<CarProperties>();
                if (!carProps)
                {
                    Debug.LogWarning("[ModUtils/SPL/Saving/Error]: CarProperties was not found on part " + data.name + ", make sure to remove SaveData component if the component is not a car part!");
                    continue;
                }

                SavedData sd = new SavedData();

                sd.PrefabName = data.PartName;
                sd.Data = data.Data;
                sd.ObjectNumber = carProps.ObjectNumber;
                
                DataToSave.Data.Add(sd);
            }

            try
            {
                if (!Directory.Exists(SavePath))
                    Directory.CreateDirectory(SavePath);

                using (TextWriter tw = new StreamWriter(SavePath + FileName))
                {
                    tw.Write(JsonConvert.SerializeObject(DataToSave));
                }

                Debug.Log($"[ModUtils/SPL/Saving]: Succesfully saved custom data ({DataToSave.Data.Count})");
            }
            catch(Exception ex)
            {
                Debug.LogError("[ModUtils/SPL/Saving/Error]: Saving was not succesful due to an exception.");
                Debug.LogError("[ModUtils/SPL/Saving/Error]: " + ex.Message);
            }*/

        }

        internal static void SaveBarnData(SaveSystem saveSystem, bool isBarn, SaveInside si)
        {
            try
            {
                DataWrapper DataToSave = new DataWrapper();
                foreach (GameObject gameObject in si.goList)
                {
                    SaveData data = gameObject.GetComponent<SaveData>();

                    if (!data) continue; // We ignore the part if it does not have custom data written on it

                    CarProperties carProps = data.GetComponent<CarProperties>();
                    if (!carProps)
                    {
                        Debug.LogWarning("[ModUtils/SPL/Saving/Error]: CarProperties was not found on part " + data.name + ", make sure to remove SaveData component if the component is not a car part!");
                        continue;
                    }

                    SavedData sd = new SavedData();

                    sd.PrefabName = data.PartName;
                    sd.Data = data.Data;
                    sd.ObjectNumber = carProps.ObjectNumber;

                    DataToSave.Data.Add(sd);
                }

                saveSystem.add("ModUtils_BARN_DATA", JsonConvert.SerializeObject(DataToSave));
            }
            catch (Exception ex)
            {
                Debug.LogError("[ModUtils/SPL/Saving/Error]: Major issue ocurred while saving barn data!");
                Debug.LogError($"[ModUtils/SPL/Saving/Error]: saveSystem: {saveSystem} | isBarn: {isBarn} | saveInside: {si}");
                Debug.LogError(ex.ToString());
            }
        }

        internal static void SaveGameData(SaveSystem saveSystem, bool isBarn)
        {
            try
            {
                DataWrapper DataToSave = new DataWrapper();
                foreach (SaveData data in UnityEngine.Object.FindObjectsOfType<SaveData>())
                {
                    CarProperties carProps = data.GetComponent<CarProperties>();
                    if (!carProps)
                    {
                        Debug.LogWarning("[ModUtils/SPL/Saving/Error]: CarProperties was not found on part " + data.name + ", make sure to remove SaveData component if the component is not a car part!");
                        continue;
                    }

                    SavedData sd = new SavedData();

                    sd.PrefabName = data.PartName;
                    sd.Data = data.Data;
                    sd.ObjectNumber = carProps.ObjectNumber;

                    DataToSave.Data.Add(sd);
                }

                saveSystem.add("ModUtils_GAME_DATA", JsonConvert.SerializeObject(DataToSave));
            }
            catch (Exception ex)
            {
                Debug.LogError("[ModUtils/SPL/Saving/Error]: Major issue ocurred while saving game data!");
                Debug.LogError($"[ModUtils/SPL/Saving/Error]: saveSystem: {saveSystem} | isBarn: {isBarn}");
                Debug.LogError(ex.ToString());
            }
        }
    }
}
