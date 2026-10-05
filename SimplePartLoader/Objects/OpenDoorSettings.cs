using UnityEngine;

namespace SimplePartLoader
{
    public sealed class OpenDoorSettings : MonoBehaviour
    {
        public Vector3 Axis = Vector3.up;
        public float Angle = 90f;

        internal bool IsValid
        {
            get
            {
                float magnitude = Axis.sqrMagnitude;
                return !float.IsNaN(magnitude) && !float.IsInfinity(magnitude) && magnitude >= 0.000001f && !float.IsNaN(Angle) && !float.IsInfinity(Angle) && Angle > 0f && Angle <= 180f;
            }
        }
    }
}
