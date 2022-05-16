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
        [SerializeField] private Transform _startDot;
        [SerializeField] private Transform _endDot;
        [SerializeField] private GameObject _endDotRadialArtGo;

        private List<PositionData> _positionData = new List<PositionData>();
        private Vector3 _nextPointPosition;
        private Vector3 _fingerPointerInitialScale;
        private int _nextPointIndex;
        private Camera _camera;
        private int _lastSegmentIndex;

        private const float SCALE_ANIMATION_DURATION = 1f;
        private const float DRAW_LINE_DURATION = 0.05f;

        public Action<FingerPointer> OnDestinationReached;
        public Action OnFingerOutOfPointer;

        public void SetupFingerPointer(Camera mainCamera)
        {
            if (_path.positionCount < 2)
            {
                return;
            }

            _camera = mainCamera;
            _dragHelper.SetCamera(mainCamera);
            _pathReached.positionCount = 1;
            
            Vector3 startPosition = _path.GetPosition(0);
            _pathReached.positionCount = 2;
            _pathReached.SetPosition(0, startPosition);
            _pathReached.SetPosition(1, startPosition);
            _fingerPointerTransform.position = startPosition;
            _nextPointIndex = 1;
            _lastSegmentIndex = 0;
            _nextPointPosition = _path.GetPosition(_nextPointIndex);
            _fingerPointerArtTransform.LookAtZAxis(_nextPointPosition);
        }

        public void BeginDrag() =>
            _fingerPointerTransform.DOScale(_fingerPointerInitialScale, SCALE_ANIMATION_DURATION).SetEase(Ease.OutCirc).OnComplete(EnableDragging);
        
        public IPromise EndDrag()
        {
            Promise promise = new Promise();
            _fingerPointerTransform.DOScale(0, SCALE_ANIMATION_DURATION).SetEase(Ease.OutCirc).OnComplete(promise.Resolve);

            return promise;
        }

        public IPromise PopulateBezierLineData(BezierDotsData dotsData, bool withAnimation = true)
        {
            PopulatePositionsArray(dotsData);
            _path.positionCount = 1;
            Vector3 previousPosition = _positionData[0].DotPosition;
            _path.SetPosition(0, previousPosition);
            _startDot.position = previousPosition;
            _endDot.position = previousPosition;
            _endDotRadialArtGo.SetActive(false);

            List<Func<IPromise>> promises = new List<Func<IPromise>>();
            for (int i = 1; i < _positionData.Count; i++)
            {
                int index = i;
                Vector3 from = previousPosition;
                Func<IPromise> nextLineDrawPromise = new Func<IPromise>(() =>
                {
                    Promise promise = new Promise();
                    _path.positionCount = index + 1;
                    Vector3 nextDotPosition = _positionData[index].DotPosition;
                    float distance = Vector3.Distance(from, nextDotPosition);
                    
                    DOVirtual.Float(0, distance, DRAW_LINE_DURATION, lerpDist =>
                    {
                        Vector3 nextPosition = Vector3.MoveTowards(from, nextDotPosition, lerpDist);
                        _path.SetPosition(index, nextPosition);
                        _endDot.position = nextPosition;
                    }).SetEase(Ease.Linear).OnComplete(() => promise.Resolve());
                    return promise;
                });

                promises.Add(nextLineDrawPromise);
                previousPosition = _positionData[i].DotPosition;
            }

            return Promise.Sequence(promises).Then(() => _endDotRadialArtGo.SetActive(true));
        }

        private void PopulatePositionsArray(BezierDotsData dotsData)
        {
            _positionData.Clear();
            Vector3 initialPos = dotsData.FirstDot.FromSerializedVector();
            _positionData.Add(GetPositionData(initialPos, true));
            int nextDotIndex = 1;
            for (int index = 0; index < dotsData.LineDots.Length; index++)
            {
                Vector3 dot = dotsData.LineDots[index].FromSerializedVector();
                Vector3 bezierControlDot = dotsData.BezierControlDots[index].FromSerializedVector();
                List<Vector3> curvePoints = LineRendererExtensions.GetBezierLinePositions(initialPos, dot, bezierControlDot);
                for (int i = 0; i < curvePoints.Count; i++)
                {
                    Vector3 curvePoint = curvePoints[i];
                    _positionData.Add(GetPositionData(curvePoint));
                    nextDotIndex += 1;
                }

                _positionData[nextDotIndex - 1].IsSegment = true;
                initialPos = _positionData[nextDotIndex - 1].DotPosition;
            }
        }

        private PositionData GetPositionData(Vector3 position, bool isSegment = false)
        {
            return new PositionData()
            {
                DotPosition = position,
                IsSegment = isSegment
            };
        }

        private void Awake()
        {
            _fingerPointerInitialScale = _fingerPointerTransform.localScale;
            _fingerPointerTransform.localScale = Vector3.zero;
        }

        private void FingerOutOfPointer()
        {
            _nextPointIndex = _lastSegmentIndex + 1;
            _nextPointPosition = _positionData[_nextPointIndex].DotPosition;
            Vector3 lastPointPosition = _path.GetPosition(_lastSegmentIndex);
            _fingerPointerTransform.position = lastPointPosition;
            _pathReached.positionCount = _lastSegmentIndex + 2;
            _pathReached.SetPosition(_nextPointIndex, lastPointPosition);
            _fingerPointerArtTransform.LookAtZAxis(_nextPointPosition);
            OnFingerOutOfPointer?.Invoke();
        }

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
            if (_positionData.Count <= _nextPointIndex)
            {
                return false;
            }

            PositionData nextPositionData = _positionData[_nextPointIndex];
            if (nextPositionData.IsSegment)
            {
                _lastSegmentIndex = _nextPointIndex;
            }
            _pathReached.positionCount = _nextPointIndex + 1;
            _pathReached.SetPosition(_nextPointIndex, _nextPointPosition);
            nextPointPosition = nextPositionData.DotPosition;
            return true;
        }

        private class PositionData
        {
            public Vector3 DotPosition;
            public bool IsSegment;
        }
    }
}
