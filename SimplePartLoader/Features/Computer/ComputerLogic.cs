using float_oat.Desktop90;
using SimplePartLoader.Features.Computer.AppLauncher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace SimplePartLoader.Features
{
    public class ComputerLogic
    {
        internal static List<ComputerApp> RegisteredApps = new List<ComputerApp>();

        internal static GameObject CurrentUI_Instance = null;
        
        internal static void Setup(GameObject canvas)
        {
            CurrentUI_Instance = canvas;

            Transform windowsTab = canvas.transform.Find("Content/Windows");
            Transform iconTab = canvas.transform.Find("Content/Icons");

            foreach(ComputerApp app in RegisteredApps)
            {
                GameObject window = GameObject.Instantiate(app.WindowPrefab, windowsTab);
                GameObject icon = GameObject.Instantiate(app.IconPrefab, iconTab);

                app.CurrentWindowInstance = window;
                app.CurrentIconInstance = icon;

                window.AddComponent<ComputerIdentifier>().Name = app.AppNameIdentifier;
                icon.AddComponent<ComputerIdentifier>().Name = app.AppNameIdentifier;

                icon.SetActive(false);
                window.SetActive(false);

                if(app.AppNameIdentifier == "App launcher")
                    AppLauncherHandling.Load(window); // App launcher!

                if (DataHandler.GetData($"ModUtils_Computer_{app.AppNameIdentifier}_IconX") != null)
                {
                    float pX = (float)Convert.ChangeType(DataHandler.GetData($"ModUtils_Computer_{app.AppNameIdentifier}_IconX"), typeof(float));
                    float pY = (float)Convert.ChangeType(DataHandler.GetData($"ModUtils_Computer_{app.AppNameIdentifier}_IconY"), typeof(float));

                    icon.SetActive(true);
                    icon.transform.localPosition = new Vector3(pX, pY, 0);
                }
                else if(app.GameDefaultApp)
                {
                    icon.SetActive(true);
                    icon.transform.localPosition = new Vector3(0, 0, 0);
                }

                icon.GetComponent<DesktopIcon>().OnDoubleClick.AddListener(window.GetComponent<WindowController>().Open);
            }
        }

        internal static void OnComputerScreenClose()
        {
            if (!CurrentUI_Instance)
                return;

            Transform iconTab = CurrentUI_Instance.transform.Find("Content/Icons");
            foreach(Transform t in iconTab.GetComponentsInChildren<Transform>())
            {
                ComputerIdentifier ci = t.GetComponent<ComputerIdentifier>();
                if(ci && t.gameObject.activeSelf)
                {
                    Debug.Log("Saving icon data for " + ci.Name);
                    DataHandler.AddData($"ModUtils_Computer_{ci.Name}_IconX", t.localPosition.x);
                    DataHandler.AddData($"ModUtils_Computer_{ci.Name}_IconY", t.localPosition.y);
                }
            }

            //CurrentUI_Instance = null;
        }

        internal static ComputerApp RegisterApp(string appName, GameObject windowPrefab, GameObject iconPrefab, bool defaultApp)
        {
            ComputerApp existingAppCheck = RegisteredApps.Where(x => x.AppNameIdentifier == appName).FirstOrDefault();
            if (existingAppCheck != null)
            {
                Debug.LogError($"[ModUtils/Computer/Error]: Tried to register app but name is already on use! App-name: {appName}");
                return null;
            }

            if (windowPrefab == null || iconPrefab == null || String.IsNullOrEmpty(appName))
            {
                Debug.LogError($"[ModUtils/Computer/Error]: Required data to create app was not given!");
                return null;
            }

            ComputerApp app = new ComputerApp();

            app.AppNameIdentifier = appName;
            app.WindowPrefab = windowPrefab;
            app.IconPrefab = iconPrefab;

            app.GameDefaultApp = defaultApp;
            app.HideInAppLauncher = defaultApp;

            RegisteredApps.Add(app);
            return app;
        }

        public static void RegisterApp(string appName, GameObject windowPrefab, GameObject iconPrefab)
        {
            RegisterApp(appName, windowPrefab, iconPrefab, false);
        }

        internal static void CreateIcon(string itemText)
        {
            if (!CurrentUI_Instance)
                return;

            ComputerApp app = RegisteredApps.Where(a => a.AppNameIdentifier == itemText).FirstOrDefault();
            if(app != null)
            {
                if (app.CurrentIconInstance)
                    app.CurrentIconInstance.SetActive(true);
            }
        }

        internal static void ShowWindow(string itemText)
        {
            if (!CurrentUI_Instance)
                return;

            ComputerApp app = RegisteredApps.Where(a => a.AppNameIdentifier == itemText).FirstOrDefault();
            if (app != null)
            {
                if (app.CurrentWindowInstance)
                    app.CurrentWindowInstance.GetComponent<WindowController>().Open();
            }
        }
    }
}
