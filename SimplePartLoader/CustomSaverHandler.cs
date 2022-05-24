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

        public static void Load()
        {
            DataWrapper LoadedData;

            try
            {
                if (!Directory.Exists(SavePath))
                    Directory.CreateDirectory(SavePath);


                if (PlayerPrefs.GetFloat("LoadLevel") == 0f) // New game
                {
                    if (File.Exists(SavePath + FileName))
                        File.Delete(SavePath + FileName);

                    return;
                }

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
                Debug.Log("[SPL]: There was an issue trying to load the custom data.");
                Debug.Log("[SPL]: " + ex.Message);
                return;
            }

            foreach (SaveData sdComponent in UnityEngine.Object.FindObjectsOfType<SaveData>())
            {
                CarProperties carProps = sdComponent.GetComponent<CarProperties>();
                if(!carProps)
                {
                    continue;
                }

                foreach(SavedData loadedData in LoadedData.Data)
                {
                    if(carProps.ObjectNumber == loadedData.ObjectNumber)
                    {
                        sdComponent.Data = loadedData.Data;
                    }
                }
            }

            SPL.InvokeLoadFinish();
        }

        public static void Save()
        {
            DataWrapper DataToSave = new DataWrapper();

            foreach (SaveData data in UnityEngine.Object.FindObjectsOfType<SaveData>())
            {
                CarProperties carProps = data.GetComponent<CarProperties>();
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

                Debug.Log($"[SPL]: Succesfully saved custom data ({DataToSave.Data.Count})");
            }
            catch(Exception ex)
            {
                Debug.Log("[SPL]: Saving was not succesful due to an exception.");
                Debug.Log("[SPL]: " + ex.Message);
            }
            
        }
    }
}
