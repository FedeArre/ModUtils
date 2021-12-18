using UnityEngine;

namespace SimplePartLoader
{
    public class TransparentData
    {
        private readonly Vector3 DEFAULT_SCALE = new Vector3(0.1f, 0.1f, 0.1f);

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
            Scale = DEFAULT_SCALE;
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
