using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SimplePartLoader.Features.UI
{
    internal class ModUtilsUI
    {
        // Mods panel
        static Transform panel;

        // Mod list & Mod settings
        static Transform allModPanel;
        static Transform modSettingsPanel;
        static Transform modSettingsAttach;
        static Text exitButtonText;
        static TMP_Text titleText;

        // Prefabs
        static GameObject labelSettingPrefab;
        static GameObject textInputSettingPrefab;
        static GameObject dropdownSettingPrefab;
        static GameObject spacerSettingPrefab;

        static GameObject eventSystemSpecial;

        static ModInstance currentModInstance = null;

        internal static void PrepareUI()
        {
            panel = ModMain.UI_Mods.transform.Find("Panel");
            GameObject modCard = panel.Find("ModCardPrefab").gameObject;
            Transform viewportContent = panel.Find("Rect/Scroll View/Viewport/Content");

            allModPanel = panel.Find("Rect");
            modSettingsPanel = panel.Find("ModSettings");
            modSettingsAttach = panel.Find("ModSettings/Scroll View/Viewport/Content");
            exitButtonText = panel.Find("Button/Text").GetComponent<Text>();
            titleText = panel.Find("Title").GetComponent<TMP_Text>();

            labelSettingPrefab = panel.Find("SettingLabelPrefab").gameObject;
            textInputSettingPrefab = panel.Find("SettingTextInputPrefab").gameObject;
            dropdownSettingPrefab = panel.Find("SettingDropdownPrefab").gameObject;
            spacerSettingPrefab = panel.Find("SettingSpacer").gameObject;

            panel.Find("Footer").GetComponent<TMP_Text>().text = $"ModUtils {ModMain.TESTING_VERSION_NUMBER} - Developed by Federico Arredondo";
            panel.gameObject.SetActive(false);

            eventSystemSpecial = new GameObject("ModUtilsUI_EventSystem_<3");
            eventSystemSpecial.AddComponent<EventSystem>();
            GameObject.DontDestroyOnLoad(eventSystemSpecial);

            // Make button work
            ModMain.UI_Mods.transform.Find("OpenMods").GetComponent<Button>().onClick.AddListener(ModsButtonClick);
            panel.Find("Button").GetComponent<Button>().onClick.AddListener(CloseButtonClick);

            // Preload all mods now.
            foreach (Mod m in ModLoader.mods)
            {
                Transform card = GameObject.Instantiate(modCard).transform;
                card.gameObject.SetActive(true);
                card.SetParent(viewportContent);
                card.localScale = Vector3.one;
                card.Find("Title").GetComponent<TMP_Text>().text = m.Name;
                card.Find("ModId").GetComponent<TMP_Text>().text = "ID: " + m.ID;

                if(m.Icon != null)
                {
                    Texture2D icon = new Texture2D(2, 2);
                    icon.LoadImage(m.Icon);
                    Sprite spr = Sprite.Create(icon, new Rect(0, 0, icon.width, icon.height), Vector2.zero);
                    card.Find("Image").GetComponent<Image>().sprite = spr;
                }

                ModInstance mi = FindModInstance(m);
                if(mi != null && mi.ModSettings.Count != 0)
                {
                    Button b = card.Find("Button").GetComponent<Button>();
                    b.interactable = true;
                    b.onClick.AddListener(delegate { OpenUISettings(mi);  });
                }
            }
        }

        internal static void ModsButtonClick()
        {
            if (currentModInstance != null) CloseButtonClick();

            panel.gameObject.SetActive(!panel.gameObject.activeSelf);
            eventSystemSpecial.SetActive(!eventSystemSpecial.activeSelf);
        }

        /// <summary>
        /// Close the current mod page if exists. Close Mods panel if no mod page is open
        /// </summary>
        internal static void CloseButtonClick()
        {
            if (currentModInstance != null) // Mod settings page open. Close and go back to the all mods page
            {
                // Settings cleanup
                for (int i = 0; i < modSettingsAttach.childCount; i++)
                {
                    GameObject.Destroy(modSettingsAttach.GetChild(i).gameObject);
                }

                allModPanel.gameObject.SetActive(true);
                modSettingsPanel.gameObject.SetActive(false);
                exitButtonText.text = "X";
                titleText.text = "List of loaded mods";
                currentModInstance = null;
            }
            else
            {
                panel.gameObject.SetActive(false);
                eventSystemSpecial.SetActive(false);
            }
        }

        internal static void OpenUISettings(ModInstance mi)
        {
            currentModInstance = mi;

            allModPanel.gameObject.SetActive(false);
            modSettingsPanel.gameObject.SetActive(true);

            exitButtonText.text = "<<";
            titleText.text = mi.Mod.Name + " settings";

            foreach (ISetting setting in mi.ModSettings)
            {
                if(setting is Label)
                {
                    Label l = (Label)setting;
                    Transform tr = GameObject.Instantiate(labelSettingPrefab).transform;
                    tr.SetParent(modSettingsAttach);
                    tr.localScale = Vector3.one;
                    tr.gameObject.SetActive(true);

                    tr.Find("Text").GetComponent<TMP_Text>().text = l.Text;
                }
                else if(setting is TextInput)
                {
                    TextInput l = (TextInput) setting;
                    Transform tr = GameObject.Instantiate(textInputSettingPrefab).transform;
                    tr.SetParent(modSettingsAttach);
                    tr.localScale = Vector3.one;
                    tr.gameObject.SetActive(true);

                    tr.Find("Text").GetComponent<TMP_Text>().text = l.Text;
                    TMP_InputField field = tr.Find("InputField").GetComponent<TMP_InputField>();
                    field.text = l.CurrentValue;

                    if (l.OnValueChange != null) field.onValueChanged.AddListener(delegate { l.OnValueChange.Invoke(field.text);  });
                }
                else if(setting is ModDropdown)
                {
                    ModDropdown d = (ModDropdown)setting;
                    if (d.Options == null || d.Options.Length == 0) continue;

                    Transform tr = GameObject.Instantiate(textInputSettingPrefab).transform;
                    tr.SetParent(modSettingsAttach);
                    tr.localScale = Vector3.one;
                    tr.gameObject.SetActive(true);

                    tr.Find("Text").GetComponent<TMP_Text>().text = d.Text;
                    TMP_Dropdown dropdown = tr.Find("Dropdown").GetComponent<TMP_Dropdown>();
                    dropdown.ClearOptions();
                    dropdown.AddOptions(d.Options.ToList());

                    dropdown.value = d.selectedOption;
                    dropdown.onValueChanged.AddListener(delegate { d.OnValueChange.Invoke(dropdown.value); });
                }
                else if(setting is Spacer)
                {
                    Transform tr = GameObject.Instantiate(spacerSettingPrefab).transform;
                    tr.SetParent(modSettingsAttach);
                    tr.localScale = Vector3.one;
                    tr.gameObject.SetActive(true);
                }
            }
        }

        internal static ModInstance FindModInstance(Mod mod)
        {
            foreach (ModInstance mi in ModUtils.RegisteredMods)
            {
                if (mod.ID == mi.Mod.ID)
                {
                    return mi;
                }
            }
            return null;
        }
    }
}
