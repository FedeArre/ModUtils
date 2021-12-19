using System.Collections;
using UnityEngine;

namespace SimplePartLoader
{
    public class Part
    {
        public GameObject Prefab;
        public CarProperties CarProps;
        public Partinfo PartInfo;

        internal bool TestingEnabled;

        public Part(GameObject prefab, CarProperties carProp, Partinfo partinfo, bool testingEnabled = false)
        {
            Prefab = prefab;
            CarProps = carProp;
            PartInfo = partinfo;

            TestingEnabled = testingEnabled;
        }

        public void EnableTransparentEditing()
        {
            TestingEnabled = true;
        }

        public void SetupTransparent(string attachesTo, Vector3 transparentLocalPos, Quaternion transaprentLocalRot)
        {
            PartManager.transparentData[attachesTo] = new TransparentData(PartInfo.RenamedPrefab, attachesTo, transparentLocalPos, transaprentLocalRot);
        }

        public void SetupTransparent(string attachesTo, Vector3 transparentLocalPos, Quaternion transaprentLocalRot, Vector3 scale)
        {
            PartManager.transparentData[attachesTo] = new TransparentData(PartInfo.RenamedPrefab, attachesTo, transparentLocalPos, transaprentLocalRot, scale);
        }
    }
}
