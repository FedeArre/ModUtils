using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SimplePartLoader
{
    public class DelayedBuildableLoader : MonoBehaviour
    {
        void Start()
        {
            StartCoroutine(LoadDelay());
        }

        IEnumerator LoadDelay()
        {
            // Scene is not ready to load in same frame, wait 3 and do it.
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            BuildableManager.OnNewMapLoad();

            Destroy(this);
        }
    }
}
