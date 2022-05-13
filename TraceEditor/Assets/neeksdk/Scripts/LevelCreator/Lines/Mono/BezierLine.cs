using System.Collections.Generic;
using neeksdk.Scripts.Extensions;
using neeksdk.Scripts.LevelCreator.Lines.Data;
using UnityEngine;

namespace neeksdk.Scripts.LevelCreator.Lines.Mono
{
    [RequireComponent(typeof(LineRenderer))]
    public class BezierLine : MonoBehaviour, IBezierLine
    {
        [SerializeField] private Transform _startLinePoint;
        [SerializeField] private Transform _fingerPointer;
        [SerializeField] private int _bezierVertexCount;

        private ILineDot _startLineDot;
        private LineRenderer _lineRenderer;
        private readonly List<BezierLinePart> _lineParts = new List<BezierLinePart>();

        public Transform StartPointTransform => _startLinePoint;
        
        public ILineDot GetLineDot()
        {
            if (_startLineDot == null)
            {
                _startLineDot = new LineDot();
            }
            
            _startLineDot.Position = _startLinePoint.position;
            return _startLineDot;
        }
        
        public void AddPoint(BezierLinePart newLinePart)
        {
            _lineParts.Add(newLinePart);
            UpdateLineWithFingerPoint();
        }

        public void DeletePoint(BezierLinePart bezierLinePart)
        {
            if (_lineParts.Remove(bezierLinePart))
            {
                UpdateLineWithFingerPoint();
            }
        }

        public void DeletePoint(int index)
        {
            if (_lineParts.Count <= index || index < 0)
            {
                return;
            }
            
            _lineParts.RemoveAt(index);
            UpdateLineWithFingerPoint();
        }

        public void UpdateLineWithFingerPoint()
        {
            UpdateLine();
            UpdateFingerPointerRotation();
        }
        
        private void UpdateLine()
        {
            if (_startLinePoint == null || _lineParts.Count == 0)
            {
                return;
            }
            
            _lineRenderer.PopulateBezierPoints(_startLinePoint.position, _lineParts, _bezierVertexCount);
        }

        private void UpdateFingerPointerRotation()
        {
            if (_fingerPointer != null && _lineRenderer.positionCount < 2)
            {
                return;
            }
            
            _fingerPointer.LookAtZAxis(_lineRenderer.GetPosition(1));
        }
        
        private void Awake()
        {
            _lineRenderer = GetComponent<LineRenderer>();
        }
    }
}