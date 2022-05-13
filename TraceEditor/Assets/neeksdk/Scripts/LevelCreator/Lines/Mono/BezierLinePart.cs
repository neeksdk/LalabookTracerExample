using neeksdk.Scripts.LevelCreator.Lines.Data;
using UnityEngine;

namespace neeksdk.Scripts.LevelCreator.Lines.Mono
{
    public class BezierLinePart : MonoBehaviour
    {
        [SerializeField] private Transform _linePoint;
        [SerializeField] private Transform _bezierControlPoint;

        private ILineDot _lineDot;
        private ILineDot _bezierControlDot;

        public Vector3 GetLineDotPosition => _linePoint.transform.position;
        public Vector3 GetBezierControlDotPosition => _bezierControlPoint.transform.position;

        public void SetLinePointPosition(Vector3 position) => _linePoint.position = position;
        public void SetBezierControlPointPosition(Vector3 position) => _bezierControlPoint.position = position;

        public ILineDot GetLineDot()
        {
            if (_lineDot == null)
            {
                _lineDot = new LineDot();
            }
           
            _lineDot.Position = _linePoint.transform.position;
            return _lineDot;
        }

        public ILineDot GetBezierControlDot()
        {
            if (_bezierControlDot == null)
            {
                _bezierControlDot = new BezierControlDot();
            }
            
            _bezierControlDot.Position = _bezierControlPoint.position;
            return _bezierControlDot;
        }
    }
}