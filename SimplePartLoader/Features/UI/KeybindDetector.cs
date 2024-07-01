using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SimplePartLoader.Features.UI
{
    internal class KeybindDetector : MonoBehaviour
    {
        private static readonly KeyCode[] keyCodes = Enum.GetValues(typeof(KeyCode))
                                                 .Cast<KeyCode>()
                                                 .Where(k => ((int)k < (int)KeyCode.Mouse0))
                                                 .ToArray();

        public bool CurrentlyEditing;

        void Update()
        {
            if (!CurrentlyEditing) return;

            foreach (KeyCode keyCode in keyCodes)
            {
                if (Input.GetKey(keyCode))
                {
                    ModUtilsUI.KeyPressDetected(keyCode);
                    break;
                }
            }
        }
    }
}
