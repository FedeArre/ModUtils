using System.Collections.Generic;
using UnityEngine;

namespace SimplePartLoader
{
    public class Part
    {
        public GameObject Prefab;
        public CarProperties CarProps;
        public Partinfo PartInfo;

        internal List<TransparentData> transparentData; // Using a list since a Part can be attached into multiple places

        public bool TestingEnabled;

        public Part(GameObject prefab, CarProperties carProp, Partinfo partinfo, bool testingEnabled = false)
        {
            Prefab = prefab;
            CarProps = carProp;
            PartInfo = partinfo;

            TestingEnabled = testingEnabled;

            transparentData = new List<TransparentData>();
        }

        public void SetupTransparent(string attachesTo, Vector3 transparentLocalPos, Quaternion transaprentLocalRot)
        {
            transparentData.Add(new TransparentData(attachesTo, transparentLocalPos, transaprentLocalRot));
        }

        public void SetupTransparent(string attachesTo, Vector3 transparentLocalPos, Quaternion transaprentLocalRot, Vector3 scale)
        {
            transparentData.Add(new TransparentData(attachesTo, transparentLocalPos, transaprentLocalRot, scale));
        }
    }
}
