using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SimplePartLoader
{
    public class SpawnSpot
    {
        internal Vector3 Position;
        internal Quaternion Rotation;
        internal GameObject SpawnSpotObject;

        public SpawnSpot(Vector3 position, Vector3 rotation)
        {
            Position = position;
            Rotation = Quaternion.Euler(rotation);
        }

        public SpawnSpot(Vector3 position, Quaternion rotation)
        {
            Position = position;
            Rotation = rotation;
        }

        public void Create()
        {
            if (SpawnSpotObject)
                return;

            SpawnSpotObject = new GameObject("MODUTILS_SPAWNPOINT");
            SpawnSpotObject.transform.position = Position;
            SpawnSpotObject.transform.rotation = Rotation;
        }

        public Transform Get()
        {
            if (!SpawnSpotObject)
                Create();

            return SpawnSpotObject.transform;
        }
    }
}
