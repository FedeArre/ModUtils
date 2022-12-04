using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SimplePartLoader.CarGen
{
    public class Car
    {
        internal GameObject carPrefab;
        internal GameObject emptyCarPrefab;
        internal GameObject transparentsObject;

        internal CarGenerator carGeneratorData;

        internal Action<GameObject> OnSetupCarTemplate;
        internal Action<GameObject> OnPostBuild;

        internal BuildingExceptions exceptionsObject;

        internal ModInstance loadedBy;

        public bool EnableDebug = false;
        
        internal Car(GameObject car, GameObject empty, GameObject transparents)
        {
            carPrefab = car;
            emptyCarPrefab = empty;
            transparentsObject = transparents;

            carGeneratorData = car.GetComponent<CarGenerator>();

            exceptionsObject = new BuildingExceptions();
        }
        
        public void SetCarTemplateFunction(Action<GameObject> function)
        {
            OnSetupCarTemplate = function;
        }

        public void SetPostBuildFunction(Action<GameObject> function)
        {
            OnPostBuild = function;
        }
    }
}
