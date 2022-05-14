using neeksdk.Scripts.Constants;
using UnityEngine;

namespace neeksdk.Scripts.Extensions
{
    public static class GridExtensions
    {
        public static Vector3 WorldToGridVectorCoordinates(this Vector3 point) =>
            new Vector3((int)point.x, (int)point.y, 0);
        
        public static Vector3 WorldToGridVectorCoordinatesCentered(this Vector3 point) =>
            new Vector3((int)point.x + 0.5f, (int)point.y + 0.5f, 0);
        
        public static (int row, int col) WorldToGridCoordinates(this Vector3 point) =>
            ((int)point.x, (int)point.y);
        
        public static Vector3 GridToWorldCoordinates(this Transform transform, int row, int col) {
            Vector3 pos = transform.position;
            Vector3 worldPoint = new Vector3((pos.x + row + 0.5f), (pos.y + col + 0.5f), 0);

            return worldPoint;
        }
        
        public static bool IsInsideGridBounds(this Vector3 point) {
            float minX = point.x;
            float maxX = minX + RedactorConstants.REDACTOR_WIDTH;
            float minY = point.y;
            float maxY = minY + RedactorConstants.REDACTOR_HEIGHT;

            return (point.x >= minX && point.x <= maxX && point.y >= minY && point.y <= maxY);
        }
        
        public static bool IsInsideGridBounds(int row, int col) {
            return (row >= 0 && row < RedactorConstants.REDACTOR_WIDTH && col >= 0 && col < RedactorConstants.REDACTOR_HEIGHT);
        }
    }
}