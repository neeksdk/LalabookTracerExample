using neeksdk.Scripts.StaticData.LinesData;
using UnityEngine;

namespace neeksdk.Scripts.Extensions
{
    public static class VectorExtensions
    {
        public static void LookAtZAxis(this Transform target, Vector3 lookPoint)
        {
            Vector3 diff = lookPoint - target.position;
            float rotationZ = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
            target.rotation = Quaternion.Euler(0, 0, rotationZ);
        }

        public static SerializedVectorData ToSerializedVector(this Vector3 target) =>
            new SerializedVectorData() {
                x = target.x,
                y = target.y,
                z = target.z
            };

        public static Vector3 FromSerializedVector(this SerializedVectorData target) =>
            new Vector3(target.x, target.y, target.z);
    }
}