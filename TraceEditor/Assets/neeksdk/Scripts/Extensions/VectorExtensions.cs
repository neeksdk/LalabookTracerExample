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

        public static Vector3 DistanceAlongMousePosition(this Vector3 outsideVector, Vector3 startPoint,
            Vector3 endPoint)
        {
            Vector3 ab = endPoint - startPoint;
            Vector3 mouse = outsideVector - startPoint;
            float distance = Vector3.Distance(startPoint, endPoint);
            float dotNormalized = Vector3.Dot(ab.normalized, mouse);
            float dotCorrected = Mathf.Min(Mathf.Max(0, dotNormalized), distance);

            return Vector3.MoveTowards(startPoint, endPoint, dotCorrected);
        }

        public static Vector3 GetMousePosition(this Camera mainCamera, bool zAxisIsZero = true)
        {
            Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            if (zAxisIsZero)
            {
                mousePosition.z = 0;
            }

            return mousePosition;
        }
    }
}