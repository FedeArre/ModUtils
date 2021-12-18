using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SimplePartLoader
{
    public class Part
    {
        public GameObject Prefab;
        public CarProperties CarProps;
        public Partinfo PartInfo;

        internal List<TransparentData> transparentData; // Using a list since a Part can be attached into multiple places

        public Part(GameObject prefab, CarProperties carProp, Partinfo partinfo)
        {
            Prefab = prefab;
            CarProps = carProp;
            PartInfo = partinfo;

            transparentData = new List<TransparentData>();
        }

        public void SetupTransparent(string attachesTo, Vector3 transparentLocalPos, Quaternion transaprentLocalRot)
        {
            transparentData.Add(new TransparentData(attachesTo, transparentLocalPos, transaprentLocalRot));
        }
    }
}
