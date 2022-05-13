using System;
using System.Collections.Generic;
using neeksdk.Scripts.Extensions;
using UnityEngine;

namespace neeksdk.Scripts.LevelCreator
{
    [RequireComponent(typeof(LineRenderer)), ExecuteInEditMode]
    public class BezierLine : MonoBehaviour
    {
        [SerializeField] private LinePoint _firstLinePoint;
        [SerializeField] private Transform pointDirection;

        public int vertexCount = 12;

        [SerializeField] private LineRenderer _lineRenderer;
        public BezierLinePointsData[] _linePoints = new BezierLinePointsData[1];

        private void Update()
        {
            List<Vector3> lineRendererPointList = new List<Vector3>();
            LinePoint previousLinePoint = _firstLinePoint;
            foreach (BezierLinePointsData pointsData in _linePoints)
            {
                PopulateBezierPoints(lineRendererPointList, pointsData, previousLinePoint);
                previousLinePoint = pointsData.linePoint;
            }
            
            _lineRenderer.positionCount = lineRendererPointList.Count;
            _lineRenderer.SetPositions(lineRendererPointList.ToArray());
            SetDirection();
        }

        private void PopulateBezierPoints(List<Vector3> lineRendererPointList, BezierLinePointsData nextPointsData, LinePoint previousPoint)
        {
            Vector3 startPos = previousPoint.transform.position;
            Vector3 bezierPos = nextPointsData.bezierPoint.transform.position;
            Vector3 endPos = nextPointsData.linePoint.transform.position;
            for (float ratio = 0.5f / vertexCount; ratio < 1; ratio += 1.0f / vertexCount)
            {
                Vector3 tangentLineVertex1 = Vector3.Lerp(startPos, bezierPos, ratio);
                Vector3 tangentLineVertex2 = Vector3.Lerp(bezierPos, endPos, ratio);
                Vector3 bezierPoint = Vector3.Lerp(tangentLineVertex1, tangentLineVertex2, ratio);
                lineRendererPointList.Add(bezierPoint);
            }
        }

        private void SetDirection()
        {
            if (_lineRenderer.positionCount >= 2)
                pointDirection.rotation = _firstLinePoint.transform.LookAtZAxis(_lineRenderer.GetPosition(1));
        }

        private void OnDrawGizmos()
        {
            /*Gizmos.color = Color.red;
            Gizmos.DrawLine(point1.position, point2.position);
            Gizmos.DrawLine(point2.position, point3.position);

            for (float ratio = 0.5f / vertexCount; ratio < 1; ratio += 1.0f / vertexCount)
            {
                Gizmos.DrawLine(
                    Vector3.Lerp(point1.position, point2.position, ratio),
                    Vector3.Lerp(point2.position, point3.position, ratio)
                );
            }*/
        }
    }
}