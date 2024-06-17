using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace SimplePartLoader.Utils
{
    internal class BackgroundDelayChange : MonoBehaviour
    {
        void Start()
        {
            // Hook image
            if(ModMain.imageBytes.Length > 0)
                GameObject.Find("UIController/MainMenu_Canvas/BG").GetComponent<Image>().sprite.texture.LoadImage(ModMain.imageBytes);

            GameObject.Destroy(this.gameObject);
        }
    }
}
