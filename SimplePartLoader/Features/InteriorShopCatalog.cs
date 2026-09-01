using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SimplePartLoader.Features
{
    public class InteriorShopCatalog
    {
        internal static HashSet<InteriorShopSellData> Items { get; set; } = new HashSet<InteriorShopSellData>();

        internal static GameObject CurrentCanvas;
        internal static GameObject EventSystem;

        internal static TMP_Dropdown ColorDropdown;
        internal static TMP_Dropdown ModDropdown;
        internal static TMP_InputField SearchBar;
        internal static GameObject ItemCardPrefab;
        internal static Transform Viewport;

        internal static Dictionary<string, ModInstance> ModList { get; set; }

        internal static void OpenOrClose()
        {
            if (CurrentCanvas)
            {
                GameObject.Destroy(CurrentCanvas);
                GameObject.Destroy(EventSystem);

                // Unlock mouse
                ModUtils.PlayerAIO.ControllerUnPause();
            }
            else
            {
                if (ModUtils.PlayerTools.EscMenu.activeSelf)
                {
                    return; // Prevent opening if esc menu is open
                }

                EventSystem = new GameObject("EventSystemTEMP");
                EventSystem.AddComponent<EventSystem>();
                EventSystem.AddComponent<StandaloneInputModule>();

                CurrentCanvas = GameObject.Instantiate(ModMain.UI_InteriorCatalog_Prefab);

                // Lock player mouse
                ModUtils.PlayerAIO.ControllerPause();

                ColorDropdown = CurrentCanvas.transform.Find("Panel/DropdownColor").GetComponent<TMP_Dropdown>();
                ModDropdown = CurrentCanvas.transform.Find("Panel/DropdownMod").GetComponent<TMP_Dropdown>();
                SearchBar = CurrentCanvas.transform.Find("Panel/Search").GetComponent<TMP_InputField>();
                ItemCardPrefab = CurrentCanvas.transform.Find("Panel/ItemCardPrefab").gameObject;
                Viewport = CurrentCanvas.transform.Find("Panel/Rect/Scroll View/Viewport/Content");
                ItemCardPrefab.SetActive(false);

                CurrentCanvas.transform.Find("Panel/Button").GetComponent<Button>().onClick.AddListener(OpenOrClose); // Close button

                // Handlers
                ModDropdown.onValueChanged.AddListener(ModDropdownChanged);
                SearchBar.onValueChanged.AddListener((string s) => { FilterUpdate(); });

                // Load mods to list
                if (ModList is null)
                {
                    ModList = new Dictionary<string, ModInstance>();
                    foreach (Part p in Items.Select(x => x.Part).ToArray())
                    {
                        if (!ModList.ContainsKey(p.Mod.Name))
                            ModList.Add(p.Mod.Name, p.Mod);
                    }
                }

                ModDropdown.ClearOptions();
                List<TMP_Dropdown.OptionData> newOptions = new List<TMP_Dropdown.OptionData>();

                newOptions.Add(new TMP_Dropdown.OptionData("Any mod"));

                foreach (var kvp in ModList.Values.Distinct())
                {
                    newOptions.Add(new TMP_Dropdown.OptionData(kvp.Name));
                }

                ModDropdown.AddOptions(newOptions);

                FilterUpdate();
            }
        }

        internal static void ModDropdownChanged(int index)
        {
            FilterUpdate();
        }

        internal static void FilterUpdate()
        {
            var modName = ModDropdown.options[ModDropdown.value].text;
            var searchText = SearchBar.text.ToLowerInvariant();

            foreach (var item in Items)
            {
                // Item should not be visible
                if ((modName != "Any mod" && item.ModName != modName) || !item.Name.ToLowerInvariant().Contains(searchText))
                {
                    if(item.CurrentPrefab != null)
                    {
                        item.CurrentPrefab.SetActive(false);
                    }
                }
                else // Item should be visible
                {
                    if(item.CurrentPrefab != null)
                    {
                        item.CurrentPrefab.SetActive(true);
                    }
                    else
                    {
                        item.CurrentPrefab = GameObject.Instantiate(ItemCardPrefab);

                        item.CurrentPrefab.transform.Find("Title").GetComponent<TMP_Text>().text = item.Name;
                        item.CurrentPrefab.transform.Find("Price").GetComponent<TMP_Text>().text = $"Price: <color=#00FF00>{item.Price}$</color>";

                        if(item.Part.PartInfo.Thumbnail)
                        {
                            item.CurrentPrefab.transform.Find("Image").GetComponent<Image>().sprite = Sprite.Create(item.Part.PartInfo.Thumbnail, new Rect(0f, 0f, 105.44f, 98.87f), new Vector2(0.5f, 0.5f));
                        }

                        item.CurrentPrefab.transform.Find("Button").GetComponent<Button>().onClick.AddListener(() =>
                        {
                            BuyItem(item);
                        });

                        item.CurrentPrefab.transform.SetParent(Viewport);
                        item.CurrentPrefab.transform.localScale = Vector3.one;

                        item.CurrentPrefab.SetActive(true);
                    }
                }
            }
        }

        internal static void BuyItem(InteriorShopSellData item)
        {
            if (tools.money < item.Price)
            {
                return;
            }

            tools.money -= item.Price;
            ModUtils.PlayCashSound();

            GameObject spawnSpot = GameObject.Find("SpawnSpot_EXTRABUILDINGS");
            tools.SpawnSpot = spawnSpot;

            var originalInterior = item.Part.CarProps.OriginalInterior;

            item.Part.CarProps.OriginalInterior = ColorDropdown.value + 1;
            item.Part.PartInfo.SpawnThis();
            item.Part.CarProps.OriginalInterior = originalInterior;
        }

        internal static void RegisterPart(Part p, string name = null)
        {
            Items.Add(new InteriorShopSellData()
            {
                Name = string.IsNullOrWhiteSpace(name) ? p.CarProps.name : name,
                Price = p.PartInfo.price,
                ModName = p.Mod.Name,
                Part = p
            });
        }
    }

    internal class InteriorShopSellData
    {
        public string Name { get; set; }
        public float Price { get; set; }
        public string ModName { get; set; }
        public Part Part { get; set; }
        public GameObject CurrentPrefab { get; set; }
    }
}
