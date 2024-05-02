using Newtonsoft.Json;
using SimplePartLoader.Features.UI.Saving;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SimplePartLoader.Features.UI
{
    internal class SettingSaver
    {
        public static UISettingsWrapper Wrapper { get; set; }

        public static void LoadSettings()
        {
            string pathToFile = Application.persistentDataPath + "\\settingsModUtilsUI.json";
            if (!File.Exists(pathToFile)) return;

            try
            {
                string json = File.ReadAllText(pathToFile);

                if (json != "")
                    Wrapper = JsonConvert.DeserializeObject<UISettingsWrapper>(json);
                else
                    return;
            }
            catch (Exception ex)
            {
                CustomLogger.AddLine("SettingSaver", $"Major exception trying to access settingsModUtilsUI.json file");
                CustomLogger.AddLine("SettingSaver", ex);
                return;
            }

            try
            {
                foreach (ModInstance mod in ModUtils.RegisteredMods)
                {
                    ModWrapper modWrapper = GetModOnWrapper(mod.Mod.ID);
                    if (modWrapper == null || modWrapper.Settings.Count == 0) continue;
                    Dictionary<string, string> dicSettings = new Dictionary<string, string>();

                    foreach (SettingWrapper sw in modWrapper.Settings)
                    {
                        try
                        {
                            dicSettings.Add(sw.Id, sw.Value.ToString());
                        }
                        catch (Exception ex)
                        {
                            CustomLogger.AddLine("SettingSaver", $"Issue on ModSettings load CAST - {sw.Id}");
                            CustomLogger.AddLine("SettingSaver", ex);
                        }
                    }

                    foreach (ISetting setting in mod.ModSettings)
                    {
                        if (setting is TextInput)
                        {
                            var temp = (TextInput)setting;
                            if (dicSettings.ContainsKey(temp.SettingSaveId))
                            {
                                try
                                {
                                    temp.CurrentValue = dicSettings[temp.SettingSaveId];
                                }
                                catch (Exception ex)
                                {
                                    CustomLogger.AddLine("SettingSaver", $"Issue on ModSettings load - {temp.SettingSaveId}");
                                    CustomLogger.AddLine("SettingSaver", ex);
                                }
                            }
                        }
                        else if (setting is ModDropdown)
                        {
                            var temp = (ModDropdown)setting;
                            if (dicSettings.ContainsKey(temp.SettingSaveId))
                            {
                                try
                                {
                                    temp.selectedOption = int.Parse(dicSettings[temp.SettingSaveId]);
                                }
                                catch (Exception ex)
                                {
                                    CustomLogger.AddLine("SettingSaver", $"Issue on ModSettings load - {temp.SettingSaveId}");
                                    CustomLogger.AddLine("SettingSaver", ex);
                                }
                            }
                        }
                        else if (setting is ModSlider)
                        {
                            var temp = (ModSlider)setting;
                            if (dicSettings.ContainsKey(temp.SettingSaveId))
                            {
                                try
                                {
                                    temp.Value = float.Parse(dicSettings[temp.SettingSaveId]);
                                }
                                catch (Exception ex)
                                {
                                    CustomLogger.AddLine("SettingSaver", $"Issue on ModSettings load - {temp.SettingSaveId}");
                                    CustomLogger.AddLine("SettingSaver", ex);
                                }
                            }
                        }
                        else if (setting is Checkbox)
                        {
                            var temp = (Checkbox)setting;
                            if (dicSettings.ContainsKey(temp.SettingSaveId))
                            {
                                try
                                {
                                    temp.Checked = bool.Parse(dicSettings[temp.SettingSaveId]);
                                }
                                catch (Exception ex)
                                {
                                    CustomLogger.AddLine("SettingSaver", $"Issue on ModSettings load - {temp.SettingSaveId}");
                                    CustomLogger.AddLine("SettingSaver", ex);
                                }
                            }
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                CustomLogger.AddLine("SettingSaver", $"MAJOR ISSUE on ModSettings load - Not properly handled!");
                CustomLogger.AddLine("SettingSaver", ex);
            }

            // Settings are now loaded
            foreach (ModInstance mi in ModUtils.RegisteredMods)
            {
                if (mi.ModSettings.Count == 0) continue;
                if (mi.SettingsLoaded) continue;
                if (mi.OnSettingsLoad != null)
                {
                    mi.SettingsLoaded = true;
                    mi.OnSettingsLoad.Invoke();
                }
            }
        }

        public static ModWrapper GetModOnWrapper(string id)
        {
            foreach(ModWrapper mod in Wrapper.ModWrappers)
            {
                if(mod.ModId == id) return mod;
            }

            return null;
        }
        public static void SaveSettings() 
        {
            string pathToFile = Application.persistentDataPath + "\\settingsModUtilsUI.json";

            Wrapper = new UISettingsWrapper();

            foreach(ModInstance mods in ModUtils.RegisteredMods)
            {
                List<ISetting> settings = mods.GetSaveablesSettings();
                if (settings == null) continue;

                ModWrapper modWrapper = new ModWrapper();
                modWrapper.ModId = mods.Mod.ID;

                foreach(ISetting setting in settings) // dumb way to do this, was better to implement directly on interface! late realization :(
                {
                    if(setting is Checkbox)
                    {
                        var temp = (Checkbox)setting;
                        modWrapper.Settings.Add(new SettingWrapper(temp.SettingSaveId, temp.Checked));
                    }
                    else if (setting is ModSlider)
                    {
                        var temp = (ModSlider)setting;
                        modWrapper.Settings.Add(new SettingWrapper(temp.SettingSaveId, temp.Value));
                    }
                    else if (setting is ModDropdown)
                    {
                        var temp = (ModDropdown)setting;
                        modWrapper.Settings.Add(new SettingWrapper(temp.SettingSaveId, temp.selectedOption));
                    }
                    else if (setting is TextInput)
                    {
                        var temp = (TextInput)setting;
                        modWrapper.Settings.Add(new SettingWrapper(temp.SettingSaveId, temp.CurrentValue));
                    }
                }

                Wrapper.ModWrappers.Add(modWrapper);
            }

            if(File.Exists(pathToFile))
            {
                File.Delete(pathToFile);
            }

            File.WriteAllText(pathToFile, JsonConvert.SerializeObject(Wrapper));
        }
    }
}
