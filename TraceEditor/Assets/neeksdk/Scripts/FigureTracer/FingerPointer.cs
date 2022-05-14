using UnityEngine;

namespace neeksdk.Scripts.FigureTracer
{
    public class FingerPointer : MonoBehaviour
    {
        [SerializeField] private LineRenderer _path;
        [SerializeField] private LineRenderer _pathReached;
        [SerializeField] private Transform _transform;
        [SerializeField] private Transform _fingerPointTransform;
        
        
    }
}
