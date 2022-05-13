using UnityEngine;

namespace neeksdk.Scripts.LevelCreator.Lines.Data
{
    public interface ILineDot
    {
        Vector3 Position { get; set; }
    }

    public interface IReachableDot
    {
        bool IsReached { get; set; }
    }
}