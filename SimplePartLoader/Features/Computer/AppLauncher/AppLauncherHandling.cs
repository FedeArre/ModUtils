using float_oat.Desktop90;
using Rewired.UI.ControlMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace SimplePartLoader.Features.Computer.AppLauncher
{
    internal class AppLauncherHandling
    {
        internal static GameObject currentWindow;

        public static void Load(GameObject window)
        {
            currentWindow = window;

            Dropdown d = window.transform.Find("Content/D90 Dropdown").GetComponent<Dropdown>();
            d.ClearOptions();

            if (ComputerLogic.RegisteredApps.Count == 1)
            {
                d.AddOptions(new List<string> { "No apps installed! " });
            }
            else
            {
                List<string> list = new List<string>();
                ComputerLogic.RegisteredApps.ForEach(app =>
                {
                    if (!app.HideInAppLauncher)
                        list.Add(app.AppNameIdentifier);
                });

                d.AddOptions(list);

                D90Button btt1 = window.transform.Find("Content/D90 Button").GetComponent<D90Button>(); // Create icon
                btt1.onClick.AddListener(CreateIcon);

                D90Button btt2 = window.transform.Find("Content/D90 Button (1)").GetComponent<D90Button>(); // Create app
                btt2.onClick.AddListener(OpenApp);
            }
        }

        public static void CreateIcon()
        {
            Dropdown d = currentWindow.transform.Find("Content/D90 Dropdown").GetComponent<Dropdown>();

            ComputerLogic.CreateIcon(d.options[d.value].text);
        }
        public static void OpenApp()
        {
            Dropdown d = currentWindow.transform.Find("Content/D90 Dropdown").GetComponent<Dropdown>();

            ComputerLogic.ShowWindow(d.options[d.value].text);
        }
    }
}
