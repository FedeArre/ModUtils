using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

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

        void OnGUI()
        {
            // TODO
            return;
            if (DisabledModList.Count == 0 && !ThumbnaiLGeneratorEnabled && UnsupportedModList.Count == 0 && !EarlyAccessMod && DebugEnabled.Count == 0 && Dissasembler.Count == 0 && UpdateRequired.Count == 0)
                return;

            int nextLineHeight = 20;
            int totalSpaceCountReq = DisabledModList.Count + UnsupportedModList.Count + DebugEnabled.Count + Dissasembler.Count + UpdateRequired.Count;

            GUI.Box(new Rect(Screen.width - 350, 400, 300, 120 + (totalSpaceCountReq*15)), "ModUtils warnings");
            
            if (DisabledModList.Count != 0)
            {
                nextLineHeight += 15;
                GUI.Label(new Rect(Screen.width - 345, 400 + nextLineHeight, 280, 20), "EA check could not authentify some mods");
                nextLineHeight += 15;
                GUI.Label(new Rect(Screen.width - 345, 400 + nextLineHeight, 280, 20), "The following mods were disabled:");
                nextLineHeight += 15;
                
                foreach (string mod in DisabledModList)
                {
                    GUI.Label(new Rect(Screen.width - 345, 400 + nextLineHeight, 280, 20), "- " + mod);
                    nextLineHeight += 15;
                }
            }

            if (UpdateRequired.Count != 0)
            {
                nextLineHeight += 15;
                GUI.Label(new Rect(Screen.width - 345, 400 + nextLineHeight, 280, 20), "Following EA mod(s) are not updated");
                nextLineHeight += 15;
                GUI.Label(new Rect(Screen.width - 345, 400 + nextLineHeight, 280, 20), "Updating them is required to make them work");
                nextLineHeight += 15;

                foreach (string mod in UpdateRequired)
                {
                    GUI.Label(new Rect(Screen.width - 345, 400 + nextLineHeight, 280, 20), "- " + mod);
                    nextLineHeight += 15;
                }
            }

            if (ThumbnaiLGeneratorEnabled)
            {
                nextLineHeight += 15;
                GUI.Label(new Rect(Screen.width - 345, 400 + nextLineHeight, 280, 20), "Thumbnail generator enabled!");
                nextLineHeight += 15;
            }

            if (DebugEnabled.Count != 0)
            {
                nextLineHeight += 15;
                GUI.Label(new Rect(Screen.width - 345, 400 + nextLineHeight, 280, 20), "The following mod(s) enabled debug options: ");
                nextLineHeight += 15;

                foreach (string mod in DebugEnabled)
                {
                    GUI.Label(new Rect(Screen.width - 345, 400 + nextLineHeight, 280, 20), "- " + mod);
                    nextLineHeight += 15;
                }
            }

            if (Dissasembler.Count != 0)
            {
                nextLineHeight += 15;
                GUI.Label(new Rect(Screen.width - 345, 400 + nextLineHeight, 280, 20), "The following mod(s) enabled save dissasembler: ");
                nextLineHeight += 15;

                foreach (string mod in Dissasembler)
                {
                    GUI.Label(new Rect(Screen.width - 345, 400 + nextLineHeight, 280, 20), "- " + mod);
                    nextLineHeight += 15;
                }
            }

            if (EarlyAccessMod)
            {
                nextLineHeight += 15;
                GUI.Label(new Rect(Screen.width - 345, 400 + nextLineHeight, 280, 20), "Early Access (EA) mod detected but loading");
                nextLineHeight += 15;
                GUI.Label(new Rect(Screen.width - 345, 400 + nextLineHeight, 280, 20), "EA mods are not enabled. Enable them on settings");
                nextLineHeight += 15;
                GUI.Label(new Rect(Screen.width - 345, 400 + nextLineHeight, 280, 20), "and restart the game");
            }

            if (UnsupportedModList.Count != 0)
            {
                nextLineHeight += 15;
                GUI.Label(new Rect(Screen.width - 345, 400 + nextLineHeight, 280, 20), "The following mods are marked as");
                nextLineHeight += 15;
                GUI.Label(new Rect(Screen.width - 345, 400 + nextLineHeight, 280, 20), "unsupported / obsolete by the mod author: ");
                nextLineHeight += 15;
                foreach (string mod in UnsupportedModList)
                {
                    GUI.Label(new Rect(Screen.width - 345, 400 + nextLineHeight, 280, 20), "- " + mod);
                    nextLineHeight += 15;
                }
            }
        }
    }
}
