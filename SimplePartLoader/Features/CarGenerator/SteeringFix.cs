using NWH.VehiclePhysics2;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SimplePartLoader.Features.CarGenerator
{
    /// <summary>
    /// Automatically fix a steering bug caused by reflection
    /// Since v1.4.1 (dev2 version) it also fixes car collider issue, which is caused because game is not consistent.
    /// Parts reset collisions between **all** colliders, but cars only against MeshColliders!
    /// This makes no sense and even causes issues in game cars probably, but whatever. Mod cars will not have this issue
    /// </summary>
    internal class SteeringFix : MonoBehaviour
    {
        void Start()
        {
            StartCoroutine(SteeringCheck());
        }

        IEnumerator SteeringCheck()
        {
            VehicleController vc = GetComponent<VehicleController>();
            foreach (var i in vc.powertrain.wheelGroups)
            {
                i.FindBelongingWheels();
            }

            yield return 0;
            yield return 0;
            yield return 0;
            yield return 0;
            yield return 0;

            foreach (var i in vc.powertrain.wheelGroups)
            {
                i.FindBelongingWheels();
            }

            // MainCarProperties.PreventChildCollisions (but using Collider instead of MeshCollider)
            Collider[] componentsInChildren = base.GetComponentsInChildren<Collider>();
            Collider[] array = componentsInChildren;
            for (int i = 0; i < array.Length; i++)
            {
                for (int j = i + 1; j < array.Length; j++)
                {
                    if (!(array[i] == array[j]))
                    {
                        Physics.IgnoreCollision(array[i], array[j]);
                    }
                }
            }

            yield return 0;

            GameObject.Destroy(this);
        }
    }
}
