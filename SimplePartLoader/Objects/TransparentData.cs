using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SimplePartLoader
{
    public class TransparentData
    {
        public string AttachesTo;
        public Vector3 LocalPos;
        public Quaternion LocalRot;

        public TransparentData(string attachesTo, Vector3 localPos, Quaternion localRot)
        {
            AttachesTo = attachesTo;
            LocalPos = localPos;
            LocalRot = localRot;
        }
    }
}
