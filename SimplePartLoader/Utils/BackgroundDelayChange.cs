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
            {
                Image img = GameObject.Find("UIController/MainMenu_Canvas/BG").GetComponent<Image>();

                Texture2D texture = new Texture2D(2, 2);
                texture.LoadImage(ModMain.imageBytes);

                // Convert texture to sprite
                Sprite newSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

                img.sprite = newSprite;

                img.preserveAspect = true;
            }

            GameObject.Destroy(this.gameObject);
        }
    }
}
