using PaintIn3D;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SimplePartLoader.CarGen
{
    public class EmulatedJunkyard
    {
        public static void SpawnCar(GameObject car)
        {
            Debug.Log($"[ModUtils/EmulatedJunkyard]: Emulated junkyard - Spawning {car.name}");

            // Ignore CS0618 warning (This is game code copy)
#pragma warning disable CS0618
            UnityEngine.Random.seed = DateTime.Now.Millisecond + UnityEngine.Random.Range(0, 999999);
#pragma warning restore CS0618

            GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(car, new Vector3(UnityEngine.Random.Range(0.1f, 10f), UnityEngine.Random.Range(-99f, -70f), UnityEngine.Random.Range(0.1f, 10f)), Quaternion.Euler((float)UnityEngine.Random.Range(0, 360), (float)UnityEngine.Random.Range(0, 360), (float)UnityEngine.Random.Range(0, 360)));
            gameObject.AddComponent<EmulatorComponent>().car = gameObject;
        }

    }
}
