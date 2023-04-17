using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SimplePartLoader
{
    internal class SavingHandlerMono : MonoBehaviour
    {
        SaveSystem _saver;
        bool _isBarn;

        public void Load(SaveSystem saver, bool isBarn)
        {
            _saver = saver;
            _isBarn = isBarn;

            StartCoroutine(LoadCoroutine());
        }

        IEnumerator LoadCoroutine()
        {
            // Wait a frame and load the stuff
            yield return 0;

            CustomSaverHandler.Load(_saver, _isBarn);

            yield return 0;
            GameObject.DestroyImmediate(gameObject);
        }
    }
}
