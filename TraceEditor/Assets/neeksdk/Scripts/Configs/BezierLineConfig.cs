using neeksdk.Scripts.FigureTracer;
using neeksdk.Scripts.LevelCreator.Lines.Mono;
using UnityEngine;

namespace neeksdk.Scripts.Configs
{
    [CreateAssetMenu(fileName = "BezierLineConfig", menuName = "Configs/Bezier Line Config")]
    public class BezierLineConfig : ScriptableObject
    {
        public BezierLine DefaultBezierLinePrefab;
        public BezierLinePart DefaultBezierLinePartPrefab;
        public FingerPointer FingerPointerPrefab;
    }
}
