using Autoupdater.Objects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SimplePartLoader
{
    internal class KeepAlive
    {
        private static KeepAlive Instance;
        string serializedJson;
        
        private KeepAlive()
        {
            // Keep the json to post on memory
            JSON_ModList jsonList = new JSON_ModList();
            foreach (Mod mod in ModLoader.mods)
            {
                JSON_Mod jsonMod = new JSON_Mod();

                jsonMod.modId = mod.ID;
                jsonMod.version = mod.Version;

                jsonList.mods.Add(jsonMod);
            }
            serializedJson = JsonConvert.SerializeObject(jsonList);
            
            // Repeating the heartbeat func
            var startTimeSpan = TimeSpan.Zero;
            var periodTimeSpan = TimeSpan.FromMinutes(1);

            var timer = new System.Threading.Timer((e) =>
            {
                SendCurrentStatus();
            }, null, startTimeSpan, periodTimeSpan);
        }

        private void SendCurrentStatus()
        {
            Debug.Log("[ModUtils/KeepAlive]: Sending status");
            try
            {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(ModMain.API_URL + "/alive");
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Accept = "application/json";
                httpWebRequest.Method = "POST";

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    streamWriter.Write(serializedJson);
                }
                
                httpWebRequest.GetResponseAsync();
            }
            catch (Exception ex)
            {
                Debug.Log("[ModUtils/KeepAlive/Error]: Error occured while trying to send heartbeat, error: " + ex.ToString());
            }
        }
        public static KeepAlive GetInstance()
        {
            if (Instance == null)
                Instance = new KeepAlive();
            return Instance;
        }

        public void Ready()
        {
            Debug.Log("[ModUtils/KeepAlive]: Enabled!");
        }
    }
}
