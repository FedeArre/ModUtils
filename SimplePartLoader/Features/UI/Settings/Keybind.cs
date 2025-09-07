using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SimplePartLoader
{
    [Serializable]
    public class Keybind : ISetting
    {
        public KeyCode Key;
        public KeyCode Multiplier;

        public string SettingSaveId;

        /// <summary>
        /// Check if the key were pressed this frame (Does not account for the multiplier being pressed this frame). Input.GetKey for the multiplier and Input.GetKeyDown for the key
        /// </summary>
        /// <returns>True if multiplier was pressed (and present) and the key was pressed THIS frame, otherwise false</returns>
        public bool WasPressed()
        {
            if (Multiplier == KeyCode.None) return Input.GetKeyDown(Key);
            else return Input.GetKey((KeyCode)Multiplier) && Input.GetKeyDown(Key);
        }

        /// <summary>
        /// Check if the key were pressed this frame (Does not account for the multiplier being pressed this frame). Input.GetKey for the multiplier and Input.GetKeyDown for the key
        /// </summary>
        /// <returns>True if multiplier was pressed (and present) and the key was pressed THIS frame, otherwise false</returns>
        public bool WasReleased()
        {
            if (Multiplier == KeyCode.None) return Input.GetKeyUp(Key);
            else return Input.GetKey((KeyCode)Multiplier) && Input.GetKeyUp(Key);
        }


        /// <summary>
        /// Checks if both keys are present currently (or 1 if multiplier is not set). Works as Input.GetKey
        /// </summary>
        /// <returns>True if both keys are pressed, false otherwise</returns>
        public bool IsPressed()
        {
            if (Multiplier == KeyCode.None) return Input.GetKey(Key);
            else return Input.GetKey(Multiplier) && Input.GetKey(Key);
        }

        public Keybind(string saveId, KeyCode key, KeyCode multiplier)
        {
            SettingSaveId = saveId;
            Multiplier = multiplier;
            Key = key;
        }
    }
}
