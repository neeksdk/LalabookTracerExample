using System;
using neeksdk.Scripts.Constants;
using UnityEngine;

namespace neeksdk.Scripts.FigureTracer
{
    public class DragHelper : MonoBehaviour
    {
        public Action OnDrag;
        public Action OnFingerOutOfPointer;

        private bool _enableDragDetection = false;
        private bool _isDragging = false;
        private Camera _camera;

        public void StartDragDetection(bool beginDetection)
        {
            _enableDragDetection = beginDetection;
        }

        public void SetCamera(Camera mainCamera)
        {
            _camera = mainCamera;
        }

        private void OnMouseDown()
        {
            if (!_enableDragDetection)
            {
                return;
            }

            _isDragging = true;
        }

        private void OnMouseUp()
        {
            if (!_enableDragDetection)
            {
                return;
            }

            _isDragging = false;
        }

        private void OnMouseDrag()
        {
            if (!_enableDragDetection || !_isDragging || _camera == null)
            {
                return;
            }
            
            Vector3 mousePos = Input.mousePosition;
            Vector3 mousePosition = _camera.ScreenToWorldPoint(mousePos);
            mousePosition.z = 0;
            
            bool mouseOutsideAllowedRadius = Vector3.Distance(mousePosition, transform.position) > RedactorConstants.FINGER_WRONG_RADIUS;
            if (mouseOutsideAllowedRadius)
            {
                _isDragging = false;
                OnFingerOutOfPointer?.Invoke();
                return;
            }
            
            OnDrag?.Invoke();
        }
    }
}