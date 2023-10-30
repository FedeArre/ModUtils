using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SimplePartLoader.CarGen
{
    internal class LAD : ICarBase
    {
        public GameObject GetCar() => (GameObject)cachedResources.Load("LAD");

        public void ForceTemplateExceptions(BuildingExceptions exceptions)
        {

        }

        public void PostBuild(GameObject objective, Car car)
        {
            // Bone target fix
            foreach (MyBoneSCR scr in objective.GetComponentsInChildren<MyBoneSCR>())
            {
                if (car.EnableDebug)
                    Debug.Log($"[ModUtils/CarGen/Bones]: Bone found at {scr.name} ({Utils.Functions.GetTransformPath(scr.transform)})");

                if (scr.transform.childCount != 0)
                {
                    if (scr.LocalStrtetchTarget != null)
                    {
                        Transform newBone = null;
                        foreach (Transform t in scr.transform)
                        {
                            if (t.name == scr.LocalStrtetchTarget.name)
                            {
                                newBone = t;
                                break;
                            }
                        }

                        if (newBone)
                        {
                            scr.LocalStrtetchTarget = newBone;
                        }
                        else
                        {
                            Debug.LogWarning($"[ModUtils/CarGen/BoneFix/Chad]: Could not find bone for {scr.LocalStrtetchTarget.name} ({scr.LocalStrtetchTarget.parent.name})");
                        }
                    }

                    scr.targetTransform = null;
                }
            }

            // ModUtils LAD template
            if (car.carGeneratorData.DisableModUtilsTemplateSetup)
                return;
        }

        public void SetupTemplate(GameObject objective, Car car)
        {
        }
    }
}
