using System.IO;
using neeksdk.Scripts.Configs;
using neeksdk.Scripts.Constants;
using neeksdk.Scripts.Extensions;
using neeksdk.Scripts.LevelCreator.Lines.Mono;
using UnityEditor;
using UnityEngine;

namespace neeksdk.Scripts.Infrastructure.Factory
{
    public class BezierLineFactory
    {
        private BezierLineConfig _bezierLineConfig;
        
        private BezierLineConfig BezierLineConfig
        {
            get
            {
                if (_bezierLineConfig == null)
                {
                    _bezierLineConfig = AssetDatabase.LoadAssetAtPath<BezierLineConfig>(Path.Combine(RedactorConstants.CONFIGS_PATH, RedactorConstants.BEZIER_CONFIG));
                }

                return _bezierLineConfig;
            }
        }
        
        public void InstantiateDotPrefab((int col, int row) coords, GameObject prefab, BezierLine parent, Vector3 targetPosition) {
            if (parent == null)
            {
                return;
            }

            GameObject dotPrefab = prefab;
            if (dotPrefab == null)
            {
                dotPrefab = BezierLineConfig.DefaultBezierLinePartPrefab.gameObject;
            }
            
            GameObject go = PrefabUtility.InstantiatePrefab(dotPrefab) as GameObject;
            if (go == null)
            {
                return;
            }
            
            go.transform.parent = parent.transform;
            go.name = $"[{coords.col},{coords.row}][{go.name}]";
            go.transform.position = Vector3.zero;
            
            IBezierLinePart bezierLineDot = go.GetComponent<IBezierLinePart>();
            bezierLineDot.SetLinePointPosition(targetPosition);
            Vector3 lastDotPosition = parent.StartPointTransform.position;
            if (parent.Dots.Count != 0)
            {
                int lastDotIndex = parent.Dots.Count - 1;
                lastDotPosition = parent.transform.TransformPoint(parent.Dots[lastDotIndex].GetLineDotPosition);
            }
            
            Vector3 centerPos = (lastDotPosition + targetPosition) / 2;
            bezierLineDot.SetBezierControlPointPosition(centerPos);
            parent.AddPoint(bezierLineDot);
        }

        public BezierLine InstantiateNewLinePrefab((int col, int row) coords, Transform parentTransform)
        {
            GameObject go = PrefabUtility.InstantiatePrefab(BezierLineConfig.DefaultBezierLinePrefab.gameObject) as GameObject;
            if (go == null)
            {
                return null;
            }
            
            go.transform.parent = parentTransform;
            go.name = $"[{coords.col},{coords.row}][{go.name}]";
            go.transform.position = new Vector3(0.5f, 0.5f, 0);
            
            Vector3 tilePosition = parentTransform.GridToWorldCoordinates(coords.col, coords.row);
            BezierLine bezierLine = go.GetComponent<BezierLine>();
            bezierLine.StartPointTransform.position = tilePosition;
            
            return bezierLine;
        }
    }
}