using UnityEngine;

namespace neeksdk.Scripts.Extensions
{
    public static class VectorExtensions
    {
        public static Quaternion LookAtZAxis(this Transform target, Vector3 lookPoint)
        {
            Vector3 diff = lookPoint - target.position;
            float rotationZ = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
            return Quaternion.Euler(0, 0, rotationZ);
        }
    }
}