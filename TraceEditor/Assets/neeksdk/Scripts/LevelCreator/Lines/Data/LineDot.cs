using System;
using UnityEngine;

namespace neeksdk.Scripts.LevelCreator.Lines.Data
{
    [Serializable]
    public class LineDot : ILineDot, IReachableDot
    {
        public Vector3 Position { get; set; }
        public bool IsReached { get; set; }
    }
}