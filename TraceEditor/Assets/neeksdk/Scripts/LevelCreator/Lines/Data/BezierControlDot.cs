using System;
using UnityEngine;

namespace neeksdk.Scripts.LevelCreator.Lines.Data
{
    [Serializable]
    public class BezierControlDot : ILineDot
    {
        public Vector3 Position { get; set; }
    }
}