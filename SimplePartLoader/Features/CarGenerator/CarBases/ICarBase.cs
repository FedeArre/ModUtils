using UnityEngine;

namespace SimplePartLoader.CarGen
{
    public interface ICarBase
    {
        GameObject GetCar();
        void SetupTemplate(GameObject objective, Car car);
        void PostBuild(GameObject objective, Car car);
        void ForceTemplateExceptions(BuildingExceptions exceptions);
        VehicleType VehType();
    }

    public enum VehicleType
    {
        Car = 1,
        Trailer = 2
    }
}
