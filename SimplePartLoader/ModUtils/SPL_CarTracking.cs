using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SimplePartLoader
{
    internal class SPL_CarTracking : MonoBehaviour
    {
        bool dummy;

        void Start()
        {
            if (dummy)
                return;

            ModUtils.Cars.Add(this.gameObject);
        }

        void OnDestroy()
        {
            if (dummy)
                return;

            ModUtils.Cars.Remove(this.gameObject);
        }

        public void AddToAll()
        {
            dummy = true;
            StartCoroutine(AddToAllCars());
        }

        IEnumerator AddToAllCars()
        {
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            foreach (GameObject car in GameObject.FindGameObjectsWithTag("Vehicle"))
            {
                if (!car.GetComponent<SPL_CarTracking>())
                    car.AddComponent<SPL_CarTracking>();
            }

            GameObject.Destroy(this);
        }
    }
}
