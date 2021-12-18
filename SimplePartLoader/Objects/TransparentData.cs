using UnityEngine;

namespace SimplePartLoader
{
    public class TransparentData
    {
        public string Name;
        public string AttachesTo;
        public Vector3 LocalPos;
        public Quaternion LocalRot;
        public Vector3 Scale;

        public TransparentData(string attachesTo, Vector3 localPos, Quaternion localRot)
        {
            AttachesTo = attachesTo;
            LocalPos = localPos;
            LocalRot = localRot;
            Scale = Vector3.one;
        }

        public TransparentData(string attachesTo, Vector3 localPos, Quaternion localRot, Vector3 scale)
        {
            AttachesTo = attachesTo;
            LocalPos = localPos;
            LocalRot = localRot;
            Scale = scale;
        }
    }
}
