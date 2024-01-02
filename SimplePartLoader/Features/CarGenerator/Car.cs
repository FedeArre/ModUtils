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

        public GameObject BuiltCarPrefab
        {
            get { return carPrefab; }
        }

        public bool IgnoreLogErrors { get; set; }
        public bool EnableDebug { get; set; }

        internal bool IssueExternalReport = false;
        internal string ReportedIssue = string.Empty;

        internal void ReportIssue(string issue)
        {
            IssueExternalReport = true;
            ReportedIssue += issue + "\n";
        }

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

        public void AddException(string partName, string prefabName, bool forceFittingIgnoringModUtilsConditions = false)
        {
            exceptionsObject.AddException(partName, prefabName, forceFittingIgnoringModUtilsConditions);
        }
    }
}
