using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SimplePartLoader
{
    internal class TrigerColliderBug : MonoBehaviour
    {
        void Update()
        {
            if(Input.GetKeyDown(KeyCode.Tab))
            {
                StartCoroutine(Fum());
            }
        }

        IEnumerator Fum()
        {
            foreach(Collider c in GetComponentsInChildren<Collider>())
            {
                c.enabled = false;
            }

            yield return 0;
            yield return 0;
            yield return 0;
            yield return 0;
            yield return 0;
            yield return 0;
            yield return 0;
            yield return 0;
            yield return 0;
            yield return 0;

            foreach (Collider c in GetComponentsInChildren<Collider>())
            {
                c.enabled = true;
            }
        }
    }
}
