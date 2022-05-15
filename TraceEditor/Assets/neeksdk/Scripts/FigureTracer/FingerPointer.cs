using System;
using System.Collections.Generic;
using DG.Tweening;
using neeksdk.Scripts.Constants;
using neeksdk.Scripts.Extensions;
using neeksdk.Scripts.StaticData.LinesData;
using RSG;
using UnityEngine;

namespace neeksdk.Scripts.FigureTracer
{
    public class FingerPointer : MonoBehaviour
    {
        [SerializeField] private LineRenderer _path;
        [SerializeField] private LineRenderer _pathReached;
        [SerializeField] private Transform _fingerPointerTransform;
        [SerializeField] private Transform _fingerPointerArtTransform;
        [SerializeField] private DragHelper _dragHelper;

        private Vector3 _nextPointPosition;
        private Vector3 _fingerPointerInitialScale;
        private int _nextPointIndex;
        private Camera _camera;

        private const float SCALE_ANIMATION_DURATION = 1f;
        private const float DRAW_LINE_DURATION = 0.025f;

        public Action<FingerPointer> OnDestinationReached;
        public Action OnFingerOutOfPointer;

        public void SetupFingerPointer(Camera mainCamera)
        {
            if (_path.positionCount < 2)
            {
                return;
            }

            _camera = mainCamera;
            _pathReached.positionCount = 1;
            
            Vector3 startPosition = _path.GetPosition(0);
            _pathReached.positionCount = 2;
            _pathReached.SetPosition(0, startPosition);
            _pathReached.SetPosition(1, startPosition);
            _fingerPointerTransform.position = startPosition;
            _nextPointIndex = 1;
            _nextPointPosition = _path.GetPosition(_nextPointIndex);
            _fingerPointerArtTransform.LookAtZAxis(_nextPointPosition);
        }

        public void BeginDrag() =>
            _fingerPointerTransform.DOScale(_fingerPointerInitialScale, SCALE_ANIMATION_DURATION).SetEase(Ease.OutCirc).OnComplete(EnableDragging);

        public IPromise PopulateBezierLineData(BezierDotsData dotsData, bool withAnimation = true)
        {
            _path.positionCount = 1;
            Vector3 previousPosition = dotsData.FirstDot.FromSerializedVector();
            _path.SetPosition(0, previousPosition);

            List<Func<IPromise>> promises = new List<Func<IPromise>>();

            for (int i = 0; i < dotsData.LineDots.Length; i++)
            {
                Vector3 dotPosition = dotsData.LineDots[i].FromSerializedVector();
                int index = i;
                Vector3 from = previousPosition;
                Func<IPromise> nextLineDrawPromise = new Func<IPromise>(() =>
                {
                    Promise promise = new Promise();
                    _path.positionCount = index + 2;
                    Vector3 nextDotPosition = dotsData.LineDots[index].FromSerializedVector();
                    float distance = Vector3.Distance(from, nextDotPosition);
                    
                    DOVirtual.Float(0, distance, DRAW_LINE_DURATION, lerpDist =>
                    {
                        _path.SetPosition(index + 1, Vector3.MoveTowards(from, nextDotPosition, lerpDist));
                    }).SetEase(Ease.Linear).OnComplete(() => promise.Resolve());
                    return promise;
                });

                promises.Add(nextLineDrawPromise);
                previousPosition = dotPosition;
            }

            return Promise.Sequence(promises);
        }

        private void Awake()
        {
            _fingerPointerInitialScale = _fingerPointerTransform.localScale;
            _fingerPointerTransform.localScale = Vector3.zero;
        }

        private void FingerOutOfPointer() => OnFingerOutOfPointer?.Invoke();

        private void EnableDragging()
        {
            _dragHelper.StartDragDetection(true);
            _dragHelper.OnDrag += MoveFingerPointer;
            _dragHelper.OnFingerOutOfPointer += FingerOutOfPointer;
        }
        
        private void DisableDragDetection()
        {
            _dragHelper.StartDragDetection(false);
            _dragHelper.OnDrag -= MoveFingerPointer;
            _dragHelper.OnFingerOutOfPointer -= FingerOutOfPointer;
        }
        
        private void MoveFingerPointer()
        {
            Vector3 mousePosition = _camera.GetMousePosition();
            Vector3 startPoint = _path.GetPosition(_nextPointIndex - 1);
            Vector3 endPoint = _nextPointPosition;

            Vector3 nextFingerPointerPosition = mousePosition.DistanceAlongMousePosition(startPoint, endPoint);
            float distanceToMouseAlongPath = Vector3.Distance(nextFingerPointerPosition, endPoint);
            float distanceFromFingerPointer = Vector3.Distance(_fingerPointerTransform.position, endPoint);
            if (distanceToMouseAlongPath < distanceFromFingerPointer)
            {
                _fingerPointerTransform.position = nextFingerPointerPosition;
            }
            
            if (distanceFromFingerPointer < RedactorConstants.VECTOR_COMPARISON_TOLERANCE)
            {
                if (!TryGetNextPointPosition(out Vector3 nextPointPosition))
                {
                    DisableDragDetection();
                    OnDestinationReached?.Invoke(this);
                    return;
                }

                _nextPointPosition = nextPointPosition;
                _fingerPointerArtTransform.LookAtZAxis(_nextPointPosition);
            }
            
            _pathReached.SetPosition(_nextPointIndex, _fingerPointerTransform.position);
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
