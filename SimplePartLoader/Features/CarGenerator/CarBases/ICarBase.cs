using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SimplePartLoader.CarGen
{
    internal interface ICarBase
    {
        GameObject GetCar();
        void SetupTemplate(GameObject objective, Car car);
        void PostBuild(GameObject objective, Car car);
        void ForceTemplateExceptions(BuildingExceptions exceptions);
    }
}
