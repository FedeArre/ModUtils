using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SimplePartLoader.Features
{
    internal class DevUI
    {
        private static DevUI Instance;

        public bool WasModPressed = false;
        public bool IsUIOpen = false;

        public GameObject CurrentCanvas;
        public GameObject EventSystem;

        public void LogPressed()
        {
            if(IsUIOpen) // Close UI
            {
                GameObject.Destroy(CurrentCanvas);
                GameObject.Destroy(EventSystem);
                WasModPressed = false;
            }
            else // Open UI
            {
                EventSystem = new GameObject("EventSystemTEMP");
                EventSystem.AddComponent<EventSystem>();
                EventSystem.AddComponent<StandaloneInputModule>();

                CurrentCanvas = GameObject.Instantiate(ModMain.UI_Developer);
                CurrentCanvas.AddComponent<ComponentDevUI>();
            }

            IsUIOpen = !IsUIOpen;
        }

        public static DevUI GetInstance()
        {
            if(Instance == null)
                Instance = new DevUI();
            return Instance;
        }
    }
}
