using System;
using UnityEngine;

namespace SimplePartLoader.Features.IngameDialogs
{
    public class IngameDialog
    {
        internal GameObject Instance;
        internal DialogType Type;
        internal Action OnDismiss;

        internal IngameDialog(GameObject instance, DialogType type, Action onDismiss = null)
        {
            Instance = instance;
            Type = type;
            OnDismiss = onDismiss;
            instance.AddComponent<IngameDialogLifetime>().Dialog = this;
        }

        public void Close()
        {
            IngameDialogManager.CloseDialog(this);
        }
    }

    internal class IngameDialogLifetime : MonoBehaviour
    {
        internal IngameDialog Dialog;

        void OnDestroy()
        {
            IngameDialogManager.HandleDialogDestroyed(Dialog);
        }
    }
}
