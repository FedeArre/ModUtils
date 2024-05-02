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
        static GameObject sliderSettingPrefab;
        static GameObject buttonSettingPrefab;
        static GameObject checkboxSettingPrefab;

       // static GameObject eventSystemSpecial;

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
            sliderSettingPrefab = panel.Find("SettingSliderPrefab").gameObject;
            buttonSettingPrefab = panel.Find("SettingButtonPrefab").gameObject;
            checkboxSettingPrefab = panel.Find("SettingCheckboxPrefab").gameObject;
            spacerSettingPrefab = panel.Find("SettingSpacer").gameObject;

            panel.Find("Footer").GetComponent<TMP_Text>().text = $"ModUtils {ModMain.TESTING_VERSION_NUMBER} - Developed by Federico Arredondo";
            panel.gameObject.SetActive(false);
            ModMain.UI_Mods.GetComponent<Canvas>().sortingOrder = 500;

            /*eventSystemSpecial = new GameObject("ModUtilsUI_EventSystem_<3");
            eventSystemSpecial.AddComponent<EventSystem>();
            GameObject.DontDestroyOnLoad(eventSystemSpecial);*/

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
            //eventSystemSpecial.SetActive(!eventSystemSpecial.activeSelf);
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

                SettingSaver.SaveSettings();
            }
            else
            {
                panel.gameObject.SetActive(false);
                //eventSystemSpecial.SetActive(false);
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

                    field.onValueChanged.AddListener(delegate { l.CurrentValue = field.text; });
                    if (l.OnValueChange != null) field.onValueChanged.AddListener(delegate { l.OnValueChange.Invoke(field.text);  });
                }
                else if(setting is ModDropdown)
                {
                    ModDropdown d = (ModDropdown)setting;
                    if (d.Options == null || d.Options.Length == 0) continue;

                    Transform tr = GameObject.Instantiate(dropdownSettingPrefab).transform;
                    tr.SetParent(modSettingsAttach);
                    tr.localScale = Vector3.one;
                    tr.gameObject.SetActive(true);

                    tr.Find("Text").GetComponent<TMP_Text>().text = d.Text;
                    TMP_Dropdown dropdown = tr.Find("Dropdown").GetComponent<TMP_Dropdown>();
                    
                    dropdown.ClearOptions();
                    dropdown.AddOptions(d.Options.ToList());

                    dropdown.value = d.selectedOption;
                    dropdown.onValueChanged.AddListener(delegate { d.selectedOption = dropdown.value;  });

                    if(d.OnValueChange != null) dropdown.onValueChanged.AddListener(delegate { d.OnValueChange.Invoke(dropdown.value); });
                }
                else if(setting is Spacer)
                {
                    Transform tr = GameObject.Instantiate(spacerSettingPrefab).transform;
                    tr.SetParent(modSettingsAttach);
                    tr.localScale = Vector3.one;
                    tr.gameObject.SetActive(true);
                }
                else if(setting is ModSlider)
                {
                    ModSlider sl = (ModSlider)setting;
                    Transform tr = GameObject.Instantiate(sliderSettingPrefab).transform;
                    tr.SetParent(modSettingsAttach);
                    tr.localScale = Vector3.one;
                    tr.gameObject.SetActive(true);

                    TMP_Text textValue = tr.transform.Find("Text").GetComponent<TMP_Text>();
                    textValue.text = "Value: " + sl.Value;
                    tr.transform.Find("MaxValue").GetComponent<TMP_Text>().text = ""+sl.MaxValue;
                    tr.transform.Find("MinValue").GetComponent<TMP_Text>().text = ""+sl.MinValue;

                    Slider slider = tr.transform.Find("Slider").GetComponent<Slider>();
                    slider.minValue = sl.MinValue;
                    slider.maxValue = sl.MaxValue;
                    slider.value = sl.Value;
                    slider.wholeNumbers = sl.WholeNumbers;

                    slider.onValueChanged.AddListener(delegate { sl.Value = slider.value; });
                    slider.onValueChanged.AddListener(delegate { UpdateSlider(textValue, slider.value);  });
                    if (sl.OnValueChanged != null) slider.onValueChanged.AddListener(delegate { sl.OnValueChanged.Invoke(slider.value); });
                }
                else if(setting is ModButton)
                {
                    ModButton modButton = (ModButton)setting;
                    Transform tr = GameObject.Instantiate(buttonSettingPrefab).transform;
                    tr.SetParent(modSettingsAttach);
                    tr.localScale = Vector3.one;
                    tr.gameObject.SetActive(true);

                    tr.Find("Button/Text (TMP)").GetComponent<TMP_Text>().text = modButton.Text;

                    if(modButton.OnButtonPress != null) tr.Find("Button").GetComponent<Button>().onClick.AddListener(delegate { modButton.OnButtonPress.Invoke(); });
                }
                else if(setting is Checkbox)
                {
                    Checkbox checkbox = (Checkbox)setting;
                    Transform tr = GameObject.Instantiate(checkboxSettingPrefab).transform;
                    tr.SetParent(modSettingsAttach);
                    tr.localScale = Vector3.one;
                    tr.gameObject.SetActive(true);

                    Toggle toggle = tr.Find("Toggle").GetComponent<Toggle>();
                    toggle.isOn = checkbox.Checked;

                    toggle.onValueChanged.AddListener(delegate { checkbox.Checked = toggle.isOn; });
                    if (checkbox.OnValueChange != null) toggle.onValueChanged.AddListener(delegate { checkbox.OnValueChange.Invoke(toggle.isOn); });

                    tr.Find("Toggle/Text (TMP)").GetComponent<TMP_Text>().text = checkbox.Text;
                }
            }
        }

        internal static void UpdateSlider(TMP_Text t, float value)
        {
            t.text = $"Value: {value}";
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
