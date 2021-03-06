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

        public static void Load()
        {
            DataWrapper LoadedData;
            
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
            SPL.InvokeDataLoadedEvent();
        }

        public static void Save()
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
            }
            
        }
    }
}
