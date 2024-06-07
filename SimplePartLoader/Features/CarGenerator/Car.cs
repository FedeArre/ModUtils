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

        public List<string> OtherModBuildingExceptions { get; set; }

        public List<string> FitToCarExceptions { get; set; }
        public string AutomaticFitToCar { get; set; }

        public GameObject BuiltCarPrefab
        {
            get { return carPrefab; }
        }

        [Obsolete("The following property will be removed on ModUtils v1.5")]
        public bool IgnoreLogErrors { get; set; }
        [Obsolete("The following property will be removed on ModUtils v1.5")]
        public bool EnableDebug { get; set; }

        internal bool IssueExternalReport = false;
        internal string ReportedIssue = string.Empty;

        internal bool DelayRearBoneFix = false;

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
            FitToCarExceptions = new List<string>();
            OtherModBuildingExceptions = new List<string>();
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

        public void ApplyRearBoneDelayedFix()
        {
            DelayRearBoneFix = true;
        }
    }
}
