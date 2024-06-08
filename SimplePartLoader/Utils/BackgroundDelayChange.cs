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
            GameObject.Find("UIController/MainMenu_Canvas/BG").GetComponent<Image>().sprite.texture.LoadImage(ModMain.imageBytes);
            GameObject.Destroy(this.gameObject);
            //StartCoroutine(Test());
        }

        IEnumerator Test()
        {
            yield return 0;
        }
    }
}
