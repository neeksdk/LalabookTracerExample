using System.Collections.Generic;
using neeksdk.Scripts.Extensions;
using neeksdk.Scripts.LevelCreator.Lines.Data;
using neeksdk.Scripts.Properties;
using UnityEngine;

namespace neeksdk.Scripts.LevelCreator.Lines.Mono
{
    [RequireComponent(typeof(LineRenderer))]
    public class BezierLine : MonoBehaviour, IBezierLine
    {
        [SerializeField] private LineRenderer _lineRenderer;
        [SerializeField] private Transform _startLinePoint;
        [SerializeField] private SprColorChanger _startLinePointColorChanger;
        [SerializeField] private SprColorChanger _fingePointerColorChanger;
        [SerializeField] private LnrColorChanger _lineColorChanger;
        [SerializeField] private Transform _fingerPointer;
        [SerializeField] private int _bezierVertexCount;

        private ILineDot _startLineDot;
        public List<IBezierLinePart> Dots { get; } = new List<IBezierLinePart>();

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
        
        public void AddPoint(IBezierLinePart newLinePart)
        {
            Dots.Add(newLinePart);
            UpdateLineWithFingerPoint();
        }

        public void DeletePoint(IBezierLinePart bezierLinePart)
        {
            if (Dots.Remove(bezierLinePart))
            {
                UpdateLineWithFingerPoint();
            }
        }

        public void DeletePoint(int index)
        {
            if (Dots.Count <= index || index < 0)
            {
                return;
            }
            
            Dots.RemoveAt(index);
            UpdateLineWithFingerPoint();
        }

        public void UpdateLineWithFingerPoint()
        {
            UpdateLine();
            UpdateFingerPointerRotation();
        }

        public void ChangeDotColors(ColorTypes colorType)
        {
            _startLinePointColorChanger.ApplyColor(colorType);
            foreach (IBezierLinePart bezierLinePart in Dots)
            {
                bezierLinePart.ApplyColor(colorType);
            }
        }

        public void ChangeDotsAlpha(bool showLine)
        {
            ColorTypes colorType = showLine ? ColorTypes.Revealed : ColorTypes.Hidden;
            _startLinePointColorChanger.ApplyAlpha(colorType);
            _fingePointerColorChanger.ApplyAlpha(colorType);
            _lineColorChanger.ApplyAlpha(colorType);
            foreach (IBezierLinePart bezierLinePart in Dots)
            {
                bezierLinePart.GameObject.SetActive(showLine);
            }
        }

        private void UpdateLine()
        {
            if (_startLinePoint == null || Dots.Count == 0)
            {
                _lineRenderer.positionCount = 0;
                return;
            }

            _lineRenderer.PopulateBezierPoints(_startLinePoint.position, Dots, _bezierVertexCount);
        }

        private void UpdateFingerPointerRotation()
        {
            if (_fingerPointer != null && _lineRenderer.positionCount < 2)
            {
                return;
            }
            
            _fingerPointer.LookAtZAxis(_lineRenderer.GetPosition(1));
        }
    }
}