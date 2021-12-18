using UnityEngine;

namespace SimplePartLoader
{
    public class TransparentData
    {
        public string Name;
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
