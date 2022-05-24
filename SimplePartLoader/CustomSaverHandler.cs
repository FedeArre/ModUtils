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

            if (!Directory.Exists(SavePath))
                Directory.CreateDirectory(SavePath);

            using (TextWriter tw = new StreamWriter(SavePath + FileName))
            {
                tw.Write(JsonConvert.SerializeObject(DataToSave));
            }
        }
    }
}
