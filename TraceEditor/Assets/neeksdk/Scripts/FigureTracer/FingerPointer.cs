using System;
using neeksdk.Scripts.Constants;
using neeksdk.Scripts.Extensions;
using UnityEngine;

namespace neeksdk.Scripts.FigureTracer
{
    public class FingerPointer : MonoBehaviour
    {
        [SerializeField] private LineRenderer _path;
        [SerializeField] private LineRenderer _pathReached;
        [SerializeField] private Transform _transform;
        [SerializeField] private Transform _fingerPointTransform;

        private Vector3 _nextPointPosition;
        private int _nextPointIndex;
        private Vector3 _previousMousePosition;
        private bool _isDragging;
        private bool _destinationReached;

        public Action<FingerPointer> OnDestinationReached;
        
        private void OnEnable()
        {
            SetToStartPosition();
        }

        private void SetToStartPosition()
        {
            if (_path.positionCount < 2)
            {
                return;
            }
            
            _pathReached.positionCount = 1;
            Vector3 startPosition = _path.GetPosition(0);
            _pathReached.positionCount = 2;
            _pathReached.SetPosition(0, startPosition);
            _pathReached.SetPosition(1, startPosition);
            _transform.position = startPosition;
            _nextPointIndex = 1;
            _nextPointPosition = _path.GetPosition(_nextPointIndex);
            _fingerPointTransform.LookAtZAxis(_nextPointPosition);
        }

        private void OnMouseUp()
        {
            if (_destinationReached)
            {
                return;
            }
            
            Debug.Log(" --- mouse up");
            _isDragging = false;
        }

        private void OnMouseDown()
        {
            if (_destinationReached)
            {
                return;
            }
            
            Debug.Log(" --- mouse down");
            _isDragging = true;
            _previousMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            //_previousMousePosition.z = -20;
        }

        private void OnMouseDrag()
        {
            if (!_isDragging || _destinationReached)
            {
                return;
            }
            
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 previousMousePosition = _previousMousePosition;
            _previousMousePosition = mousePosition;
            mousePosition.z = 0;
            Debug.Log($" --- mouse pos: {mousePosition}");
            Debug.Log($" --- previous mouse pos: {previousMousePosition}");

            bool mouseOutsideAllowedRadius = Vector3.Distance(mousePosition, _transform.position) > RedactorConstants.FINGER_WRONG_RADIUS;

            if (mouseOutsideAllowedRadius)
            {
                _isDragging = false;
                Debug.Log(" --- out of area");
                return;
            }

            float distanceFromFingerPointer = Vector3.Distance(_transform.position, _nextPointPosition);
            float distanceFromMousePosition = Vector3.Distance(mousePosition, _nextPointPosition);
            float moveDistance = Vector3.Distance(previousMousePosition, mousePosition);
            if (distanceFromFingerPointer < RedactorConstants.VECTOR_COMPARISON_TOLERANCE)
            {
                if (!TryGetNextPointPosition(out Vector3 nextPointPosition))
                {
                    _destinationReached = true;
                    OnDestinationReached?.Invoke(this);
                    return;
                }

                _nextPointPosition = nextPointPosition;
                _fingerPointTransform.LookAtZAxis(_nextPointPosition);
            }

            float diff = distanceFromFingerPointer - distanceFromMousePosition;
            if (diff > 0)
            {
                _transform.position = Vector3.MoveTowards(_transform.position, _nextPointPosition, moveDistance);
            }
            
            _pathReached.SetPosition(_nextPointIndex, _transform.position);
        }

        private void Update()
        {
            if (!_isDragging || _destinationReached)
            {
                return;
            }
            
            //Debug.Log(" --- update");
            
        }

        private bool TryGetNextPointPosition(out Vector3 nextPointPosition)
        {
            _pathReached.SetPosition(_nextPointIndex, _nextPointPosition);
            nextPointPosition = Vector3.zero;
            _nextPointIndex += 1;
            if (_path.positionCount <= _nextPointIndex)
            {
                return false;
            }

            _pathReached.positionCount = _nextPointIndex + 1;
            _pathReached.SetPosition(_nextPointIndex, _nextPointPosition);
            nextPointPosition = _path.GetPosition(_nextPointIndex);
            return true;
        }
    }
}
