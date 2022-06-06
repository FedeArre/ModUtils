using UnityEngine;

namespace SimplePartLoader
{
    public class TransparentData
    {
        public string Name { get; internal set; }
        public string AttachesTo;
        public Vector3 LocalPos;
        public Quaternion LocalRot;
        public Vector3 Scale;
        public bool TestingEnabled;
        public string PartThatNeedsToBeOff;
        public transparents.AttachingObjects[] AttachingObjects;
        public transparents.dependantObjects[] DependantObjects;
        public int SavePosition;

        public TransparentData(string name, string attachesTo, Vector3 localPos, Quaternion localRot, bool testingModeEnable)
        {
            Name = name;
            AttachesTo = attachesTo;
            LocalPos = localPos;
            LocalRot = localRot;
            Scale = Vector3.one;
            TestingEnabled = testingModeEnable;
            PartThatNeedsToBeOff = null;
            AttachingObjects = new transparents.AttachingObjects[0];
            DependantObjects = new transparents.dependantObjects[0];
        }

        public TransparentData(string name, string attachesTo, Vector3 localPos, Quaternion localRot, Vector3 scale, bool testingModeEnable)
        {
            Name = name;
            AttachesTo = attachesTo;
            LocalPos = localPos;
            LocalRot = localRot;
            Scale = scale;
            TestingEnabled = testingModeEnable;
            PartThatNeedsToBeOff = null;
        }
    }
}
