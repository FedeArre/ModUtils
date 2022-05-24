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
        public void Load()
        {
            StartCoroutine(LoadCoroutine());
        }

        IEnumerator LoadCoroutine()
        {
            yield return new WaitForEndOfFrame();

            CustomSaverHandler.Load();
            GameObject.Destroy(this.gameObject);
        }
    }
}
