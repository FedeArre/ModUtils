using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace SimplePartLoader.Features
{
    public class ModShopCatalog
    {
        internal static HashSet<ModShopSellItemData> Items { get; set; } = new HashSet<ModShopSellItemData>();

        internal static ModShopSellItemData RegisterObject(ModInstance mod, string name, float price, GameObject prefab, Action<GameObject, GameObject> action, Sprite photo)
        {
            var data = new ModShopSellItemData()
            {
                Name = name,
                Price = price,
                ModName = mod.Name,
                Prefab = prefab,
                OnBuy = action,
                Photo = photo
            };

            Items.Add(data);

            return data;
        }


        internal static GameObject CurrentCanvas;
        internal static GameObject EventSystem;

        internal static TMP_Dropdown ColorDropdown;
        internal static TMP_Dropdown ModDropdown;
        internal static TMP_InputField SearchBar;
        internal static GameObject ItemCardPrefab;
        internal static Transform Viewport;

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
                if(ModUtils.PlayerTools.EscMenu.activeSelf)
                {
                    return; // Prevent opening if esc menu is open
                }

                EventSystem = new GameObject("EventSystemTEMP");
                EventSystem.AddComponent<EventSystem>();
                EventSystem.AddComponent<StandaloneInputModule>();

                CurrentCanvas = GameObject.Instantiate(ModMain.UI_ModShop_Prefab);

                // Lock player mouse
                ModUtils.PlayerAIO.ControllerPause();

                ModDropdown = CurrentCanvas.transform.Find("Panel/DropdownMod").GetComponent<TMP_Dropdown>();
                SearchBar = CurrentCanvas.transform.Find("Panel/Search").GetComponent<TMP_InputField>();
                ItemCardPrefab = CurrentCanvas.transform.Find("Panel/ItemCardPrefab").gameObject;
                Viewport = CurrentCanvas.transform.Find("Panel/Rect/Scroll View/Viewport/Content");
                ItemCardPrefab.SetActive(false);

                CurrentCanvas.transform.Find("Panel/Button").GetComponent<Button>().onClick.AddListener(OpenOrClose); // Close button

                // Handlers
                ModDropdown.onValueChanged.AddListener(ModDropdownChanged);
                SearchBar.onValueChanged.AddListener((string s) => { FilterUpdate(); });

                ModDropdown.ClearOptions();
                List<TMP_Dropdown.OptionData> newOptions = new List<TMP_Dropdown.OptionData>();

                newOptions.Add(new TMP_Dropdown.OptionData("Any mod"));

                foreach (var mod in Items.Select(x => x.ModName).Distinct())
                {
                    newOptions.Add(new TMP_Dropdown.OptionData(mod));
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
                    if (item.CurrentPrefab != null)
                    {
                        item.CurrentPrefab.SetActive(false);
                    }
                }
                else // Item should be visible
                {
                    if (item.CurrentPrefab != null)
                    {
                        item.CurrentPrefab.SetActive(true);
                    }
                    else
                    {
                        item.CurrentPrefab = GameObject.Instantiate(ItemCardPrefab);

                        item.CurrentPrefab.transform.Find("Title").GetComponent<TMP_Text>().text = item.Name;
                        item.CurrentPrefab.transform.Find("Price").GetComponent<TMP_Text>().text = $"Price: <color=#00FF00>{item.Price}$</color>";
                        item.CurrentPrefab.transform.Find("ModName").GetComponent<TMP_Text>().text = item.ModName;

                        if (item.Photo != null)
                        {
                            item.CurrentPrefab.transform.Find("Image").GetComponent<Image>().sprite = item.Photo;
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

        internal static void BuyItem(ModShopSellItemData item)
        {
            if (tools.money < item.Price)
            {
                return;
            }

            tools.money -= item.Price;
            ModUtils.PlayCashSound();

            GameObject spawnSpot = GameObject.Find("SpawnSpot_EXTRABUILDINGS");

            var spawned = GameObject.Instantiate(item.Prefab, spawnSpot.transform.position, spawnSpot.transform.rotation);

            item.OnBuy?.Invoke(item.Prefab, spawned);
        }
    }

    public class ModShopSellItemData
    {
        public string Name { get; set; }
        public float Price { get; set; }
        public string ModName { get; set; }
        public GameObject Prefab { get; set; }

        public Sprite Photo { get; set; }

        /// <summary>
        /// Action called when the player succesfully buys the item. The first GameObject is the prefab of the item being bought, the second is the spawned GameObject.
        /// </summary>
        public Action<GameObject, GameObject> OnBuy { get; set; }

        /// <summary>
        /// Refers to the current card on the shop UI.
        /// </summary>
        internal GameObject CurrentPrefab { get; set; }
    }
}
