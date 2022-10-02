using Autoupdater.Objects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SimplePartLoader
{
    internal class KeepAlive
    {
        private static KeepAlive Instance;
        string serializedJson;
        public HttpClient client = new HttpClient();
        
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
                Task.Run(SendCurrentStatus);
            }, null, startTimeSpan, periodTimeSpan);
        }

        private async void SendCurrentStatus()
        {
            Debug.Log("[ModUtils/KeepAlive]: Sending status");
            try
            {
                var content = new StringContent(serializedJson, Encoding.UTF8, "application/json");
                _ = await client.PostAsync(ModMain.API_URL + "/alive", content);
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
