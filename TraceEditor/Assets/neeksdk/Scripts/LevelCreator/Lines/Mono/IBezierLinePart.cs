using UnityEngine;
using neeksdk.Scripts.LevelCreator.Lines.Data;
using neeksdk.Scripts.Properties;

namespace neeksdk.Scripts.LevelCreator.Lines.Mono
{
    public interface IBezierLinePart
    {
        Vector3 GetLineDotPosition { get; }
        Vector3 GetBezierControlDotPosition { get; }
        GameObject GameObject { get; }
        void ApplyColor(ColorTypes colorType);
        void SetLinePointPosition(Vector3 position);
        void SetBezierControlPointPosition(Vector3 position);
        ILineDot GetLineDot();
        ILineDot GetBezierControlDot();
    }
}