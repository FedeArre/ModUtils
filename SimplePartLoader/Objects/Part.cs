using System.Collections;
using UnityEngine;

namespace SimplePartLoader
{
    public class Part
    {
        public GameObject Prefab;
        public CarProperties CarProps;
        public Partinfo PartInfo;

        internal Hashtable transparentData; // Using a list since a Part can be attached into multiple places

        internal bool TestingEnabled;

        public Part(GameObject prefab, CarProperties carProp, Partinfo partinfo, bool testingEnabled = false)
        {
            Prefab = prefab;
            CarProps = carProp;
            PartInfo = partinfo;

            TestingEnabled = testingEnabled;

            transparentData = new Hashtable();
        }

        public void EnableTransparentEditing()
        {
            TestingEnabled = true;
        }

        public void SetupTransparent(string attachesTo, Vector3 transparentLocalPos, Quaternion transaprentLocalRot)
        {
            transparentData[attachesTo] = new TransparentData(attachesTo, transparentLocalPos, transaprentLocalRot);
        }

        public void SetupTransparent(string attachesTo, Vector3 transparentLocalPos, Quaternion transaprentLocalRot, Vector3 scale)
        {
            transparentData[attachesTo] = new TransparentData(attachesTo, transparentLocalPos, transaprentLocalRot, scale);
        }
    }
}
