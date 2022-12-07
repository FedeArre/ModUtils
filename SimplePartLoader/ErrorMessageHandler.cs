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
        public bool ThumbnaiLGeneratorEnabled;

        void OnGUI()
        {
            if (DisabledModList.Count == 0 && !ThumbnaiLGeneratorEnabled)
                return;

            int nextLineHeight = 40;
            GUI.Box(new Rect(Screen.width - 350, 400, 300, 120 + (DisabledModList.Count*15)), "ModUtils warnings");
            
            if (DisabledModList.Count != 0)
            {
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

            if (ThumbnaiLGeneratorEnabled)
            {
                nextLineHeight += 15;
                GUI.Label(new Rect(Screen.width - 345, 400 + nextLineHeight, 280, 20), "Thumbnail generator enabled!");
                nextLineHeight += 15;
            }
        }
    }
}
