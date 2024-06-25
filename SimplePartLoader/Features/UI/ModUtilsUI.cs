using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
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
        static GameObject keybindSettingPrefab;
        static GameObject separatorSettingPrefab;
        static GameObject headerSettingPrefab;
        static GameObject smallHeaderSettingPrefab;

       // static GameObject eventSystemSpecial;

        static ModInstance currentModInstance = null;

        static KeybindDetector keybindDetector = null;
        internal static Keybind currentlyEditingKeybind = null;
        static TMP_Text currentlyEditingKeybindText = null;
        static bool editingMultiplier = false;

        static List<string> RegisteredMods = new List<string>();

        internal static void PrepareUI()
        {
            panel = ModMain.UI_Mods.transform.Find("Panel");

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
            keybindSettingPrefab = panel.Find("SettingsKeybindPrefab").gameObject;
            separatorSettingPrefab = panel.Find("SettingSeparator").gameObject;
            headerSettingPrefab = panel.Find("SettingHeader").gameObject;
            smallHeaderSettingPrefab = panel.Find("SettingHeaderSmaller").gameObject;

            panel.Find("Footer").GetComponent<TMP_Text>().text = $"ModUtils {ModMain.TESTING_VERSION_NUMBER} - Developed by Federico Arredondo";
            panel.gameObject.SetActive(false);
            ModMain.UI_Mods.GetComponent<Canvas>().sortingOrder = 501;

            /*eventSystemSpecial = new GameObject("ModUtilsUI_EventSystem_<3");
            eventSystemSpecial.AddComponent<EventSystem>();
            GameObject.DontDestroyOnLoad(eventSystemSpecial);*/

            // Make button work
            ModMain.UI_Mods.transform.Find("OpenMods").GetComponent<Button>().onClick.AddListener(ModsButtonClick);
            panel.Find("Button").GetComponent<Button>().onClick.AddListener(CloseButtonClick);

            keybindDetector = ModMain.UI_Mods.AddComponent<KeybindDetector>();

            // Preload all mods now.
            ModCardLoad();
        }

        internal static void ModCardLoad()
        {
            GameObject modCard = panel.Find("ModCardPrefab").gameObject;
            Transform viewportContent = panel.Find("Rect/Scroll View/Viewport/Content");

            foreach (Mod m in ModLoader.mods)
            {
                ModInstance mi = FindModInstance(m);

                if (mi != null && mi.CardLoaded) continue;
                if (mi != null) mi.CardLoaded = true;
                if (RegisteredMods.Contains(m.ID)) continue;

                RegisteredMods.Add(m.ID);

                Transform card = GameObject.Instantiate(modCard).transform;
                card.gameObject.SetActive(true);
                card.SetParent(viewportContent);
                card.localScale = Vector3.one;
                card.Find("Title").GetComponent<TMP_Text>().text = m.Name;
                card.Find("ModId").GetComponent<TMP_Text>().text = "By: " + m.Author;

                if (m.Icon != null)
                {
                    Texture2D icon = new Texture2D(2, 2);
                    icon.LoadImage(m.Icon);
                    Sprite spr = Sprite.Create(icon, new Rect(0, 0, icon.width, icon.height), Vector2.zero);
                    card.Find("Image").GetComponent<Image>().sprite = spr;
                }

                if (mi != null && mi.ModSettings.Count != 0)
                {
                    Button b = card.Find("Button").GetComponent<Button>();
                    b.interactable = true;
                    b.onClick.AddListener(delegate { OpenUISettings(mi); });
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
                // Keybind case
                if (currentlyEditingKeybind != null) SetKeybind(currentlyEditingKeybind, null);

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
                else if (setting is Header)
                {
                    Header l = (Header)setting;
                    Transform tr = GameObject.Instantiate(headerSettingPrefab).transform;
                    tr.SetParent(modSettingsAttach);
                    tr.localScale = Vector3.one;
                    tr.gameObject.SetActive(true);

                    tr.Find("Text").GetComponent<TMP_Text>().text = l.Text;
                }
                else if (setting is SmallHeader)
                {
                    SmallHeader l = (SmallHeader)setting;
                    Transform tr = GameObject.Instantiate(smallHeaderSettingPrefab).transform;
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
                else if (setting is Separator)
                {
                    Transform tr = GameObject.Instantiate(separatorSettingPrefab).transform;
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
                else if(setting is Keybind)
                {
                    Transform tr = GameObject.Instantiate(keybindSettingPrefab).transform;
                    tr.SetParent(modSettingsAttach);
                    tr.localScale = Vector3.one;
                    tr.gameObject.SetActive(true);

                    Keybind keybind = (Keybind)setting;

                    TMP_Text multiplierBttText = tr.Find("Button/Text (TMP)").GetComponent<TMP_Text>();
                    TMP_Text keyBttText = tr.Find("Button (1)/Text (TMP)").GetComponent<TMP_Text>();

                    tr.Find("Button").GetComponent<Button>().onClick.AddListener(delegate { StartKeybindEdition(keybind, true, multiplierBttText); } );
                    tr.Find("Button (1)").GetComponent<Button>().onClick.AddListener(delegate { StartKeybindEdition(keybind, false, keyBttText); } );

                    keyBttText.text = keybind.Key.ToString();
                    multiplierBttText.text = keybind.Multiplier.ToString();
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

        internal static void StartKeybindEdition(Keybind k, bool multiplier, TMP_Text text)
        {
            if(currentlyEditingKeybind != null)
            {
                SetKeybind(k, null);
            }

            currentlyEditingKeybind = k;
            currentlyEditingKeybindText = text;
            editingMultiplier = multiplier;

            text.text = "...";

            keybindDetector.CurrentlyEditing = true;
        }

        internal static void SetKeybind(Keybind keybind, KeyCode? key)
        {
            if (currentlyEditingKeybind == null) return;
            if (key == null || key == KeyCode.Escape)
            {
                if(editingMultiplier)
                    keybind.Multiplier = KeyCode.None;

                currentlyEditingKeybindText.text = editingMultiplier ? keybind.Multiplier.ToString() : keybind.Key.ToString();
            }
            else
            {
                if (editingMultiplier && keybind.Key != key)
                    keybind.Multiplier = (KeyCode)key;
                else if(keybind.Multiplier != key)
                    keybind.Key = (KeyCode)key;

                currentlyEditingKeybindText.text = editingMultiplier ? keybind.Multiplier.ToString() : keybind.Key.ToString();
            }

            currentlyEditingKeybind = null;
            currentlyEditingKeybindText = null;

            keybindDetector.CurrentlyEditing = false;
        }

        internal static void KeyPressDetected(KeyCode key)
        {
            if(currentlyEditingKeybind != null)
            {
                SetKeybind(currentlyEditingKeybind, key);
            }
        }

        internal static void EscKeybindPress()
        {
            if (currentlyEditingKeybind != null)
            {
                SetKeybind(currentlyEditingKeybind, null);
            }
        }
    }
}
