using System;
using UnityEngine;

namespace neeksdk.Scripts.LevelCreator
{
    [Serializable]
    public class LinePiece : MonoBehaviour {
        [SerializeField] public LinePieceType _linePieceType;
        [SerializeField] public int tileNum;
    }
}
