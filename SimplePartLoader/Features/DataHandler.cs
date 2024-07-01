using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SimplePartLoader.Features
{
    public class DataHandler
    {
        internal static Dictionary<string, object> SavedStuff = new Dictionary<string, object>();

        public static void AddData(string dataKey, object data)
        {
            if(SavedStuff.ContainsKey(dataKey))
                SavedStuff[dataKey] = data;
            else
                SavedStuff.Add(dataKey, data);
        }

        public static object GetData(string dataKey)
        {
            return SavedStuff.ContainsKey(dataKey) ? SavedStuff[dataKey] : null;
        }

        public static void RemoveData(string dataKey)
        {
            if(SavedStuff.ContainsKey(dataKey))
                SavedStuff.Remove(dataKey);
        }

        internal static void OnLoad(SaveSystem ss)
        {
            string json = (string) ss.get("ModUtils_DataHandlerJSON", "");

            if (json == "")
                return;

            try
            {
                SavedStuff = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                CustomLogger.AddLine("DataHandler", $"Loaded {SavedStuff.Count} data entries!");
            }
            catch(Exception ex)
            {
                SavedStuff = new Dictionary<string, object>();
                CustomLogger.AddLine("DataHandler", ex);
            }
        }

        internal static void OnSave(SaveSystem ss)
        {
            if (SavedStuff.Count == 0)
                return;

            try
            {
                string json = JsonConvert.SerializeObject(SavedStuff);
                ss.add("ModUtils_DataHandlerJSON", json);
            }
            catch(Exception ex)
            {
                CustomLogger.AddLine("DataHandler", ex);
            }
        }
    }
}
