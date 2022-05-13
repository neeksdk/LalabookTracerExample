using System.Collections.Generic;
using neeksdk.Scripts.LevelCreator.Lines.Mono;
using UnityEngine;

namespace neeksdk.Scripts.Extensions
{
    public static class LineRendererExtensions
    {
        public static void PopulateBezierPoints(this LineRenderer lineRenderer, Vector3 initialPos, List<IBezierLinePart> lineParts, int vertexCount = 12)
        {
            List<Vector3> pointList = new List<Vector3>();
            Vector3 startPos = initialPos;
            foreach (IBezierLinePart linePart in lineParts)
            {
                Vector3 bezierPos = linePart.GetBezierControlDotPosition;
                Vector3 endPos = linePart.GetLineDotPosition;
                for (float ratio = 0.5f / vertexCount; ratio < 1; ratio += 1.0f / vertexCount)
                {
                    Vector3 tangentLineVertex1 = Vector3.Lerp(startPos, bezierPos, ratio);
                    Vector3 tangentLineVertex2 = Vector3.Lerp(bezierPos, endPos, ratio);
                    Vector3 bezierPoint = Vector3.Lerp(tangentLineVertex1, tangentLineVertex2, ratio);
                    pointList.Add(bezierPoint);
                }
                startPos = linePart.GetLineDotPosition;
            }
            
            lineRenderer.positionCount = pointList.Count;
            lineRenderer.SetPositions(pointList.ToArray());
        }
    }
}