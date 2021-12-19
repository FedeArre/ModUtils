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

        public TransparentData(string name, string attachesTo, Vector3 localPos, Quaternion localRot)
        {
            Name = name;
            AttachesTo = attachesTo;
            LocalPos = localPos;
            LocalRot = localRot;
            Scale = Vector3.one;
        }

        public TransparentData(string name, string attachesTo, Vector3 localPos, Quaternion localRot, Vector3 scale)
        {
            Name = name;
            AttachesTo = attachesTo;
            LocalPos = localPos;
            LocalRot = localRot;
            Scale = scale;
        }
    }
}
