﻿using Autoupdater.Objects;
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

        private KeepAlive()
        {
            // UNSAFE! This has to be removed after ModUtils backend update!
            //ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

            // Keep the json to post on memory
            ModListDTO jsonList = new ModListDTO(-1);
            foreach (Mod mod in ModLoader.mods)
            {
                ModDTO jsonMod = new ModDTO();

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

        public void UpdateJsonList(int buildId)
        {
            ModListDTO jsonList = new ModListDTO(buildId);
            foreach (Mod mod in ModLoader.mods)
            {
                ModDTO jsonMod = new ModDTO();

                jsonMod.modId = mod.ID;
                jsonMod.version = mod.Version;

                jsonList.mods.Add(jsonMod);
            }

            if (ModMain.EA_Enabled.Checked)
                jsonList.SteamId = Steamworks.SteamUser.GetSteamID().m_SteamID;

            serializedJson = JsonConvert.SerializeObject(jsonList);
        }

        private async void SendCurrentStatus()
        {
            if(!ModMain.Telemetry.Checked)
                return;

            try
            {
                var content = new StringContent(serializedJson, Encoding.UTF8, "application/json");
                _ = await ModMain.Client.PostAsync("v1/telemetry", content);
            }
            catch (Exception ex)
            {
                CustomLogger.AddLine("KeepAlive", ex);
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
            CustomLogger.AddLine("KeepAlive", $"Enabled KeepAlive service");
        }
    }
}
