using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SimplePartLoader.Features.IngameDialogs
{
    internal class IngameDialogManager
    {
        internal static GameObject Dialog1ButtonPrefab;
        internal static GameObject Dialog2ButtonPrefab;
        internal static GameObject Dialog2ButtonInputPrefab;

        static IngameDialog currentDialog = null;
        static GameObject eventSystemObject = null;

        internal static bool IsDialogOpen => currentDialog != null;

        internal static IngameDialog OpenInformationDialog(InformationDialogOptions options)
        {
            if (currentDialog != null)
            {
                CustomLogger.AddLine("IngameDialog", "Tried to open an information dialog but another dialog is already open.");
                return null;
            }

            GameObject instance = GameObject.Instantiate(Dialog1ButtonPrefab);
            instance.GetComponent<Canvas>().sortingOrder = 600;

            instance.transform.Find("Panel/Title").GetComponent<TMP_Text>().text = options.Title;
            instance.transform.Find("Panel/Description").GetComponent<TMP_Text>().text = options.Description;

            TMP_Text buttonText = instance.transform.Find("Panel/Button1/Text (TMP)").GetComponent<TMP_Text>();
            buttonText.text = options.ButtonText;

            Button button = instance.transform.Find("Panel/Button1").GetComponent<Button>();
            button.onClick.AddListener(delegate
            {
                if (options.OnButtonClick != null)
                    options.OnButtonClick.Invoke();

                CloseCurrentDialog();
            });

            IngameDialog dialog = new IngameDialog(instance, DialogType.Information, options.OnButtonClick);
            currentDialog = dialog;
            CreateEventSystem();
            return dialog;
        }

        internal static IngameDialog OpenConfirmationDialog(ConfirmationDialogOptions options)
        {
            if (currentDialog != null)
            {
                CustomLogger.AddLine("IngameDialog", "Tried to open a confirmation dialog but another dialog is already open.");
                return null;
            }

            GameObject instance = GameObject.Instantiate(Dialog2ButtonPrefab);
            instance.GetComponent<Canvas>().sortingOrder = 600;

            instance.transform.Find("Panel/Title").GetComponent<TMP_Text>().text = options.Title;
            instance.transform.Find("Panel/Description").GetComponent<TMP_Text>().text = options.Description;

            TMP_Text yesText = instance.transform.Find("Panel/ButtonRight/Text (TMP)").GetComponent<TMP_Text>();
            yesText.text = options.YesButtonText;

            TMP_Text noText = instance.transform.Find("Panel/ButtonLeft/Text (TMP)").GetComponent<TMP_Text>();
            noText.text = options.NoButtonText;

            Button yesButton = instance.transform.Find("Panel/ButtonRight").GetComponent<Button>();
            yesButton.onClick.AddListener(delegate
            {
                if (options.OnYesClick != null)
                    options.OnYesClick.Invoke();

                CloseCurrentDialog();
            });

            Button noButton = instance.transform.Find("Panel/ButtonLeft").GetComponent<Button>();
            noButton.onClick.AddListener(delegate
            {
                if (options.OnNoClick != null)
                    options.OnNoClick.Invoke();

                CloseCurrentDialog();
            });

            IngameDialog dialog = new IngameDialog(instance, DialogType.Confirmation, options.OnNoClick);
            currentDialog = dialog;
            CreateEventSystem();
            return dialog;
        }

        internal static IngameDialog OpenInputDialog(InputDialogOptions options)
        {
            if (currentDialog != null)
            {
                CustomLogger.AddLine("IngameDialog", "Tried to open an input dialog but another dialog is already open.");
                return null;
            }

            GameObject instance = GameObject.Instantiate(Dialog2ButtonInputPrefab);
            instance.GetComponent<Canvas>().sortingOrder = 600;

            instance.transform.Find("Panel/Title").GetComponent<TMP_Text>().text = options.Title;
            instance.transform.Find("Panel/Description").GetComponent<TMP_Text>().text = options.Description;

            TMP_Text yesText = instance.transform.Find("Panel/ButtonRight/Text (TMP)").GetComponent<TMP_Text>();
            yesText.text = options.YesButtonText;

            TMP_Text noText = instance.transform.Find("Panel/ButtonLeft/Text (TMP)").GetComponent<TMP_Text>();
            noText.text = options.NoButtonText;

            TMP_InputField inputField = instance.transform.Find("Panel/Input").GetComponent<TMP_InputField>();
            inputField.text = options.InputDefaultText;

            TMP_Text placeholder = inputField.placeholder as TMP_Text;
            if (placeholder != null)
                placeholder.text = options.InputPlaceholderText;

            Button yesButton = instance.transform.Find("Panel/ButtonRight").GetComponent<Button>();
            yesButton.onClick.AddListener(delegate
            {
                if (options.OnYesClick != null)
                    options.OnYesClick.Invoke(inputField.text);

                CloseCurrentDialog();
            });

            Button noButton = instance.transform.Find("Panel/ButtonLeft").GetComponent<Button>();
            noButton.onClick.AddListener(delegate
            {
                if (options.OnNoClick != null)
                    options.OnNoClick.Invoke();

                CloseCurrentDialog();
            });

            IngameDialog dialog = new IngameDialog(instance, DialogType.Input, options.OnNoClick);
            currentDialog = dialog;
            CreateEventSystem();
            return dialog;
        }

        internal static void HandleEscapePress()
        {
            if (currentDialog == null) return;

            if (currentDialog.OnDismiss != null)
                currentDialog.OnDismiss.Invoke();

            CloseCurrentDialog();
        }

        internal static void CloseDialog(IngameDialog dialog)
        {
            if (currentDialog == dialog)
            {
                CloseCurrentDialog();
            }
        }

        static void CloseCurrentDialog()
        {
            if (currentDialog == null) return;

            GameObject.Destroy(currentDialog.Instance);
            currentDialog = null;
            DestroyEventSystem();
        }

        static void CreateEventSystem()
        {
            if (eventSystemObject != null) return;

            eventSystemObject = new GameObject("IngameDialog_EventSystem");
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<StandaloneInputModule>();

            ModUtils.PlayerAIO?.ControllerPause();
        }

        static void DestroyEventSystem()
        {
            if (eventSystemObject == null) return;

            GameObject.Destroy(eventSystemObject);
            eventSystemObject = null;

            ModUtils.PlayerAIO?.ControllerUnPause();
        }
    }
}
