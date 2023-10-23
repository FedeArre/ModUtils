using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SimplePartLoader.Features
{
    internal class ComputerApp
    {
        public string AppNameIdentifier;
        public GameObject WindowPrefab;
        public GameObject IconPrefab;

        internal GameObject CurrentWindowInstance;
        internal GameObject CurrentIconInstance;

        internal bool HideInAppLauncher;
        internal bool GameDefaultApp;
    }
}
