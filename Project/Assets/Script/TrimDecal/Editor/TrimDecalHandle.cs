using UnityEngine.Rendering;
using UnityEngine;
using UnityEditor;

namespace TrimDecal.Editor
{
    public class TrimDecalHandle
    {
        private const float k_VertexMergeDistance = 0.05f;
        private const float k_InteractionDistance = 20.0f;
        private const float k_DottedLineSpace = 2.0f;

        private Plane m_Plane;
        private Preview m_Preview;

        private TrimDecal m_Decal;
        private TrimPropertyContext m_Property;

        private int m_ShapeSelection;
        private int m_VertexSelection;

        /////////////////////////////////////////////////////////////////

        public TrimDecalHandle(TrimDecal decal, TrimPropertyContext property)
        {
            m_Decal = decal;
            m_Property = property;

            m_Plane = new();
            m_Preview = new();

            m_ShapeSelection = -1;
            m_VertexSelection = -1;
        }

        /////////////////////////////////////////////////////////////////

        private delegate void PreviewActionHandler();
        private delegate void RealizeActionHandler();

        private PreviewActionHandler PreviewAction;
        private RealizeActionHandler RealizeAction;

        /////////////////////////////////////////////////////////////////

        public void Draw()
        {
            if (m_Decal.count == 0)
            {
                return;
            }

            DrawShapes(m_Decal);
        }

        /////////////////////////////////////////////////////////////////

        private void PreviewMoveAction()
        {
            Handles.color = Color.white;
            Handles.DotHandleCap(-1, m_Preview.position, Quaternion.identity, 0.02f, EventType.Repaint);

            if (m_Preview.positionIn != null)
            {
                Handles.DrawDottedLine(m_Preview.position, m_Preview.positionIn.Value, k_DottedLineSpace);
            }

            if (m_Preview.positionOut != null)
            {
                Handles.DrawDottedLine(m_Preview.position, m_Preview.positionOut.Value, k_DottedLineSpace);
            }

            // TODO : Validate in/out line segments, check for overlaps and intersections
            m_Preview.isValid = true;
        }

        private void RealizeMoveAction()
        {
            if (m_Preview.isValid)
            {
                if (IsClosedMesh())
                {
                    m_Property.SetShapeClosed(m_ShapeSelection, true);
                    m_Property.RemoveVertex(m_ShapeSelection, m_VertexSelection);
                    return;
                }
                m_Property.SetVertexPosition(m_ShapeSelection, m_VertexSelection, m_Preview.position);
            }
        }

        private void SetupMoveAction()
        {
            GetPreviewPositions();
            PreviewAction = PreviewMoveAction;
            RealizeAction = RealizeMoveAction;
        }

        /////////////////////////////////////////////////////////////////

        private void PreviewInsertAction()
        {
            TrimShape shape = m_Decal[m_ShapeSelection];

            Handles.color = Color.white;
            Handles.DotHandleCap(-1, m_Preview.position, Quaternion.identity, 0.02f, EventType.Repaint);
            Handles.DrawDottedLine(shape[m_VertexSelection].position, m_Preview.position, k_DottedLineSpace);

            m_Preview.isValid = true;
        }

        private void RealizeInsertAction()
        {
            // TODO : Doesn't work for first or last index

            if (m_Preview.isValid)
            {
                TrimShape shape = m_Decal[m_ShapeSelection];

                int indexA = (m_VertexSelection + 1) % shape.count;
                int indexB = m_VertexSelection == 0 ? shape.count - 1 : m_VertexSelection - 1;
                Vector3 positionA = shape[indexA].position;
                Vector3 positionB = shape[indexB].position;
                Vector3 position = shape[m_VertexSelection].position;

                Vector3 toA = (positionA - position).normalized;
                Vector3 toB = (positionB - position).normalized;
                Vector3 toP = (m_Preview.position - position).normalized;

                float dotA = Vector3.Dot(toA, toP);
                float dotB = Vector3.Dot(toB, toP);

                m_Property.InsertVertex(m_ShapeSelection, (dotA < dotB) ? m_VertexSelection : indexA, m_Preview.position);

                if (IsClosedMesh())
                {
                    m_Property.SetShapeClosed(m_ShapeSelection, true);
                }
            }
        }

        private void SetupInsertAction()
        {
            GetPreviewPositions();
            PreviewAction = PreviewInsertAction;
            RealizeAction = RealizeInsertAction;
        }

        /////////////////////////////////////////////////////////////////

        private void PreviewDeleteAction()
        {
            TrimShape shape = m_Decal[m_ShapeSelection];
            Handles.color = Color.red;

            if (m_Preview.positionIn != null)
            {
                Handles.DrawAAPolyLine(3.0f, new Vector3[] { shape[m_VertexSelection].position, m_Preview.positionIn.Value });
            }

            if (m_Preview.positionOut != null)
            {
                Handles.DrawAAPolyLine(3.0f, new Vector3[] { shape[m_VertexSelection].position, m_Preview.positionOut.Value });
            }
        }

        private void RealizeDeleteAction()
        {
            TrimShape shape = m_Decal[m_ShapeSelection];

            if (shape.count <= 2)
            {
                m_Property.RemoveShape(m_ShapeSelection);
                m_VertexSelection = -1;
                m_ShapeSelection = -1;
                return;
            }

            m_Property.RemoveVertex(m_ShapeSelection, m_VertexSelection);
            m_VertexSelection = -1;
        }

        private void SetupDeleteAction()
        {
            GetPreviewPositions();
            PreviewAction = PreviewDeleteAction;
            RealizeAction = RealizeDeleteAction;
        }

        /////////////////////////////////////////////////////////////////

        private bool IsClosedMesh()
        {
            TrimShape shape = m_Decal[m_ShapeSelection];
            int lastIndex = shape.count - 1;

            if (m_VertexSelection == 0 && Vector3.Distance(m_Preview.position, shape[lastIndex].position) < k_VertexMergeDistance)
            {
                return true;
            }

            if (m_VertexSelection == lastIndex && Vector3.Distance(m_Preview.position, shape[0].position) < k_VertexMergeDistance)
            {
                return true;
            }
            return false;
        }

        private void GetPreviewPositions()
        {
            TrimShape shape = m_Decal[m_ShapeSelection];
            m_Preview.position = shape[m_VertexSelection].position;

            if (m_VertexSelection == 0)
            {
                m_Preview.positionIn = shape.isClosed ? shape[shape.count - 1].position : null;
                m_Preview.positionOut = shape[m_VertexSelection + 1].position;
            }
            else if (m_VertexSelection == shape.count - 1)
            {
                m_Preview.positionIn = shape[m_VertexSelection - 1].position;
                m_Preview.positionOut = shape.isClosed ? shape[0].position : null;
            }
            else
            {
                m_Preview.positionIn = shape[m_VertexSelection - 1].position;
                m_Preview.positionOut = shape[m_VertexSelection + 1].position;
            }

            PreviewAction = PreviewMoveAction;
            RealizeAction = RealizeMoveAction;
        }

        private void ResetActions()
        {
            m_Preview.isValid = false;
            PreviewAction = null;
            RealizeAction = null;
        }

        /////////////////////////////////////////////////////////////////

        private void DrawShapes(TrimDecal decal)
        {
            Event e = Event.current;
            int controlID = GUIUtility.GetControlID(FocusType.Passive);
            HandleUtility.AddDefaultControl(controlID);

            int closestShapeIndex = -1;
            int closestSegmentIndex = -1;
            float closestDistance = float.MaxValue;

            for (int i = 0; i < decal.count; i++)
            {
                TrimShape shape = decal[i];
                int vertexCount = shape.count;
                if (vertexCount < 2)
                {
                    continue;
                }

                bool isSelected = m_ShapeSelection == i;
                int closedCount = vertexCount + (shape.isClosed ? 1 : 0);
                Vector3[] vertexPositions = new Vector3[closedCount];
                Vector3 firstVertexPosition = shape[0].position;

                // Get raycast plane for each shape
                m_Plane.SetNormalAndPosition(shape.normal, firstVertexPosition);

                for (int j = 0; j < closedCount; j++)
                {
                    Vector3 positionA = shape[(j + 0) % vertexCount].position;
                    Vector3 positionB = shape[(j + 1) % vertexCount].position;
                    vertexPositions[j] = positionA;

                    if (isSelected && j < vertexCount)
                    {
                        DrawVertex(e, shape, j);
                    }

                    float mouseDistance = HandleUtility.DistanceToLine(positionA, positionB);
                    if (mouseDistance < closestDistance && mouseDistance < k_InteractionDistance)
                    {
                        closestDistance = mouseDistance;
                        closestSegmentIndex = j;
                        closestShapeIndex = i;
                    }
                }

                // Draw shape
                Handles.zTest = CompareFunction.Always;
                Handles.color = GetSelectionColor(isSelected);
                Handles.DrawAAPolyLine(vertexPositions);
            }

            // Ignore viewport navigation
            if (e.alt || e.button != 0)
            {
                return;
            }

            // Mouse click, select shape
            if (e.type == EventType.MouseDown)
            {
                m_VertexSelection = -1;
                m_ShapeSelection = closestShapeIndex;
                e.Use();
            }
        }

        private void DrawVertex(Event e, TrimShape shape, int i)
        {
            Vector3 position = shape[i].position;
            bool isSelected = m_VertexSelection == i;

            int controlID = GUIUtility.GetControlID(FocusType.Passive);
            float handleSize = HandleUtility.GetHandleSize(position) * 0.05f;

            // Draw handles
            if (e.type == EventType.Repaint)
            {
                Handles.color = GetSelectionColor(isSelected);
                Handles.DotHandleCap(controlID, position, Quaternion.identity, 0.02f, EventType.Repaint);

                PreviewAction?.Invoke();
            }

            // Ignore viewport navigation
            if (e.alt || e.button != 0)
            {
                return;
            }

            // Mouse Down, detect select, add or delete operations
            if (e.type == EventType.MouseDown)
            {
                float mouseDistance = HandleUtility.DistanceToCircle(position, handleSize * 1.5f);
                if (mouseDistance < handleSize * 2.0f)
                {
                    m_VertexSelection = i;

                    switch (e.modifiers)
                    {
                        case EventModifiers.Control: SetupDeleteAction(); break;
                        case EventModifiers.Shift: SetupInsertAction(); break;
                        default: SetupMoveAction(); break;
                    }
                    e.Use();
                }
            }

            // Mouse Drag, move selected vertex
            if (e.type == EventType.MouseDrag && isSelected)
            {
                if (RaycastPlane(e.mousePosition, out m_Preview.position))
                {
                    e.Use();
                }
            }

            // Mosue Up, apply serialized data changes
            if (e.type == EventType.MouseUp)
            {
                RealizeAction?.Invoke();
                ResetActions();
                e.Use();
            }
        }

        /////////////////////////////////////////////////////////////////

        private Vector3 SnapToGrid(Vector3 position)
        {
            Vector3 grid = EditorSnapSettings.move;
            return new Vector3()
            {
                x = Mathf.Round(position.x / grid.x) * grid.x,
                y = Mathf.Round(position.y / grid.y) * grid.y,
                z = Mathf.Round(position.z / grid.z) * grid.z,
            };
        }

        private bool RaycastPlane(Vector2 position, out Vector3 point)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(position);

            if (m_Plane.Raycast(ray, out float distance))
            {
                point = ray.GetPoint(distance);

                if (EditorSnapSettings.gridSnapEnabled)
                {
                    point = SnapToGrid(point);

                }
                return true;
            }
            point = default;
            return false;
        }

        private Color GetSelectionColor(bool state)
        {
            return state ? Color.yellow : Color.gray;
        }

        /////////////////////////////////////////////////////////////////

        private struct Preview
        {
            public bool isValid;
            public int vertexIndex;
            public Vector3 position;
            public Vector3? positionIn;
            public Vector3? positionOut;
        }
    }
}
