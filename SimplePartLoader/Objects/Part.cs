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

        public TransparentData transparentData;

        public Part(GameObject prefab, CarProperties carProp, Partinfo partinfo)
        {
            Prefab = prefab;
            CarProps = carProp;
            PartInfo = partinfo;
        }

        public void SetupTransparent(string attachesTo, Vector3 transparentLocalPos, Quaternion transaprentLocalRot)
        {
            if (transparentData != null)
                return;

            transparentData = new TransparentData(attachesTo, transparentLocalPos, transaprentLocalRot, CarProps.PrefabName);
            PartManager.transparentsData[CarProps.PrefabName] = transparentData; // This has to be checked.
        }
    }
}
