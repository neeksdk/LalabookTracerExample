using System.Collections.Generic;
using neeksdk.Scripts.Constants;
using neeksdk.Scripts.Extensions;
using neeksdk.Scripts.LevelCreator.Lines.Mono;
using UnityEngine;

namespace neeksdk.Scripts.LevelCreator
{
    public class FigureConstructor : MonoBehaviour {
        public bool isHillSelected = false, isHillSelectedRightOriented = false;

        [SerializeField] public string stageName;
        [SerializeField] public int stageId;
        [SerializeField] private List<BezierLine> _lineRenderers;

        public List<BezierLine> LineRenderers {
            get => _lineRenderers;
            set => _lineRenderers = value;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos() {
            Color oldColor = Gizmos.color;
            Gizmos.color = Color.black;
            GridGizmo();
            GridFrameGizmo();
            GridSelectionGizmo();
            Gizmos.color = oldColor;
        }

        private void GridSelectionGizmo() {
            Vector3 mousePosition = UnityEditor.HandleUtility.GUIPointToWorldRay(Event.current.mousePosition).origin;
            mousePosition.z = 0;
            Vector3 gridPos = mousePosition.WorldToGridCoordinates();

            int col = Mathf.FloorToInt(gridPos.x);
            int row = Mathf.FloorToInt(gridPos.y);

            Gizmos.color = Color.yellow;
        
            if (isHillSelected) {
                if (!GridExtensions.IsInsideGridBounds(col, row) || !GridExtensions.IsInsideGridBounds(col + 1, row) ||
                    !GridExtensions.IsInsideGridBounds(col, row + 1) || !GridExtensions.IsInsideGridBounds(col + 1, row + 1)) {
                    Gizmos.color = Color.red;
                }

                if (GridExtensions.IsInsideGridBounds(col, row)) {
                    DrawSelectedGizmoBox(col, row);
                }

                if (GridExtensions.IsInsideGridBounds(col, row + 1)) {
                    DrawSelectedGizmoBox(col, row + 1);
                }

                if (isHillSelectedRightOriented) {
                    if (GridExtensions.IsInsideGridBounds(col + 1, row)) {
                        DrawSelectedGizmoBox(col + 1, row);
                    }

                    if (GridExtensions.IsInsideGridBounds(col + 1, row + 1)) {
                        DrawSelectedGizmoBox(col + 1, row + 1);
                    }
                } else {
                    if (GridExtensions.IsInsideGridBounds(col - 1, row)) {
                        DrawSelectedGizmoBox(col - 1, row);
                    }

                    if (GridExtensions.IsInsideGridBounds(col - 1, row + 1)) {
                        DrawSelectedGizmoBox(col - 1, row + 1);
                    }
                }
            } else {
                if (GridExtensions.IsInsideGridBounds(col, row)) {
                    DrawSelectedGizmoBox(col, row);
                }
            }
        }

        private void DrawSelectedGizmoBox(int col, int row) {
            Gizmos.DrawLine(new Vector3(col, row, 0), new Vector3(col, (row + 1), 0));
            Gizmos.DrawLine(new Vector3(col, row, 0), new Vector3((col + 1), row, 0));
            Gizmos.DrawLine(new Vector3((col + 1), row, 0), new Vector3((col + 1), (row + 1), 0));
            Gizmos.DrawLine(new Vector3(col, (row + 1), 0), new Vector3((col + 1), (row + 1), 0));
        }

        private void OnDrawGizmosSelected() {
            Color oldColor = Gizmos.color;

            Gizmos.color = Color.yellow;
            GridFrameGizmo();
            Gizmos.color = oldColor;
        }

        private void GridFrameGizmo() {
            Gizmos.DrawLine(new Vector3(0, 0, 0), new Vector3(0, RedactorConstants.REDACTOR_HEIGHT, 0));
            Gizmos.DrawLine(new Vector3(0, 0, 0), new Vector3(RedactorConstants.REDACTOR_WIDTH, 0, 0));
            Gizmos.DrawLine(new Vector3(RedactorConstants.REDACTOR_WIDTH, 0, 0), new Vector3(RedactorConstants.REDACTOR_WIDTH, RedactorConstants.REDACTOR_HEIGHT, 0));
            Gizmos.DrawLine(new Vector3(0, RedactorConstants.REDACTOR_HEIGHT, 0), new Vector3(RedactorConstants.REDACTOR_WIDTH, RedactorConstants.REDACTOR_HEIGHT, 0));
        }

        private void GridGizmo() {
            for (int i = 0; i < RedactorConstants.REDACTOR_WIDTH; i++) {
                Gizmos.DrawLine(new Vector3(i, 0, 0), new Vector3(i, RedactorConstants.REDACTOR_HEIGHT, 0));
            }
            for (int j = 0; j < RedactorConstants.REDACTOR_HEIGHT; j++) {
                Gizmos.DrawLine(new Vector3(0, j, 0), new Vector3(RedactorConstants.REDACTOR_WIDTH, j, 0));
            }
        }
#endif
    }
}
