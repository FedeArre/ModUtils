using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SimplePartLoader
{
    internal class ErrorMessageHandler : MonoBehaviour
    {
        private static ErrorMessageHandler Instance;
        public static ErrorMessageHandler GetInstance()
        {
            if (Instance == null)
            {
                GameObject go = new GameObject("ModUtils_ErrorMessageHandler");
                Instance = go.AddComponent<ErrorMessageHandler>();
            }

            return Instance;
        }

        public List<string> DisabledModList = new List<string>();
        public List<string> DisallowedModList = new List<string>();
        public List<string> UnsupportedModList = new List<string>();
        public bool ThumbnaiLGeneratorEnabled;
        public bool EarlyAccessMod;
        public List<string> DebugEnabled = new List<string>();
        public List<string> Dissasembler = new List<string>();
        public List<string> UpdateRequired = new List<string>();
        GameObject ui;

        void Start()
        {
            if (DisabledModList.Count == 0 && !ThumbnaiLGeneratorEnabled && UnsupportedModList.Count == 0 && !EarlyAccessMod && DebugEnabled.Count == 0 && Dissasembler.Count == 0 && UpdateRequired.Count == 0)
                return;

            ui = GameObject.Instantiate(ModMain.UI_Info_Prefab);

            int totalSpaceCountReq = DisabledModList.Count + UnsupportedModList.Count + DebugEnabled.Count + Dissasembler.Count + UpdateRequired.Count;
            totalSpaceCountReq *= 15;
            ui.transform.Find("Panel").GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, totalSpaceCountReq);
            string textToAdd = "ModUtils warnings";

            if (DisabledModList.Count != 0)
            {
                textToAdd += "\nEA check could not authentify some mods";
                textToAdd += "\nThe following mods were disabled:";
                
                foreach (string mod in DisabledModList)
                {
                    textToAdd += "\n- " + mod;
                }
            }

            if (UpdateRequired.Count != 0)
            {
                textToAdd += "\nFollowing EA mod(s) are not updated";
                textToAdd += "\nUpdating them is required to make them work";

                foreach (string mod in UpdateRequired)
                {
                    textToAdd += "\n- " + mod;
                }
            }

            if (ThumbnaiLGeneratorEnabled)
            {
                textToAdd += "\nThumbnail generator enabled!";
            }

            if (DebugEnabled.Count != 0)
            {
                textToAdd += "\nThe following mod(s) enabled debug options: ";

                foreach (string mod in DebugEnabled)
                {
                    textToAdd += "\n - " + mod;
                }
            }

            if (Dissasembler.Count != 0)
            {
                textToAdd += "\nThe following mod(s) enabled save dissasembler: ";

                foreach (string mod in Dissasembler)
                {
                    textToAdd += "\n - " + mod;
                }
            }

            if (EarlyAccessMod)
            {
                textToAdd += "\nEarly Access (EA) mod detected but loading";
                textToAdd += "\nEA mods are not enabled. Enable them on settings";
                textToAdd += "\nand restart the game";
            }

            if (UnsupportedModList.Count != 0)
            {
                textToAdd += "\nThe following mods are marked as";
                textToAdd += "\nunsupported / obsolete by the mod author: ";
                foreach (string mod in UnsupportedModList)
                {
                    textToAdd += "\n - " + mod;
                }
            }

            ui.transform.Find("Panel/Text (TMP)").GetComponent<TMP_Text>().text = textToAdd;

            Button[] bt = Resources.FindObjectsOfTypeAll(typeof(Button)) as Button[];
            foreach(Button b in bt)
            {
                b.onClick.AddListener(remove);
            }
        }

        public void remove()
        {
            if (!ui) return;
            GameObject.Destroy(ui);
        }
    }
}
