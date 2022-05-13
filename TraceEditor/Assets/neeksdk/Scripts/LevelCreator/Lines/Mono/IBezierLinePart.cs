using UnityEngine;
using neeksdk.Scripts.LevelCreator.Lines.Data;

namespace neeksdk.Scripts.LevelCreator.Lines.Mono
{
    public interface IBezierLinePart
    {
        Vector3 GetLineDotPosition { get; }
        Vector3 GetBezierControlDotPosition { get; }
        
        void SetLinePointPosition(Vector3 position);
        void SetBezierControlPointPosition(Vector3 position);
        ILineDot GetLineDot();
        ILineDot GetBezierControlDot();
    }
}