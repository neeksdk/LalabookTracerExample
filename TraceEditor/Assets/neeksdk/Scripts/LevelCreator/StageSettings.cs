using System;
using neeksdk.Scripts.Constants;
using UnityEngine;

namespace neeksdk.Scripts.LevelCreator
{
    [Serializable]
    public class StageSettings {
        [SerializeField] public StageClass[] stageTiles = new StageClass[RedactorConstants.REDACTOR_WIDTH*RedactorConstants.REDACTOR_HEIGHT];

        public void PostTile(int num, int tileNum, PointType tileType) {
            StageClass stage = new StageClass {myTileNum = tileNum, myPieceType = tileType};
            stageTiles[num] = stage;
        }

        public void PostTile(int num) {
            StageClass stage = new StageClass();
            stageTiles[num] = stage;
        }
    }

    [Serializable]
    public class StageClass {
        [SerializeField] public int myTileNum = -1;
        [SerializeField] public PointType myPieceType = PointType.GizmoControlPoint;
    }
}