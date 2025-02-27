using UnityEngine.Rendering;
using UnityEngine;
using UnityEditor;

namespace TrimDecal.Editor
{
    public class TrimDecalHandle
    {
        private const float k_InteractionDistance = 20.0f;

        private Plane m_Plane;
        private TrimDecal m_Decal;
        private TrimPropertyContext m_Context;

        private int m_ShapeSelection = -1;
        private int m_VertexSelection = -1;

        /////////////////////////////////////////////////////////////////

        public TrimDecalHandle(TrimDecal decal, TrimPropertyContext context)
        {
            m_Plane = new();
            m_Decal = decal;
            m_Context = context;
        }

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

        /////////////////////////////////////////////////////////////////

        private void DrawVertex(Event e, TrimShape shape, int i)
        {
            Vector3 position = shape[i].position;
            bool isSelected = m_VertexSelection == i;

            int controlID = GUIUtility.GetControlID(FocusType.Passive);
            float handleSize = HandleUtility.GetHandleSize(position) * 0.05f;

            Handles.color = GetSelectionColor(isSelected);
            Handles.DotHandleCap(controlID, position, Quaternion.identity, 0.02f, EventType.Repaint);

            // Ignore viewport navigation
            if (e.alt || e.button != 0)
            {
                return;
            }

            // Mouse click, select, add or delete vertex
            if (e.type == EventType.MouseDown)
            {
                float mouseDistance = HandleUtility.DistanceToCircle(position, handleSize * 1.5f);
                if (mouseDistance < handleSize * 2.0f)
                {
                    // Delete vertex
                    if (e.control)
                    {
                        m_Context.RemoveVertex(m_ShapeSelection, i);
                        m_VertexSelection = -1;
                        e.Use();
                    }
                    // Append new vertex
                    else if (e.shift && !shape.isClosed && i == shape.count - 1)
                    {
                        if (RaycastPlane(e.mousePosition, out Vector3 mousePosition))
                        {
                            m_VertexSelection = shape.count;
                            m_Context.InsertVertex(m_ShapeSelection, shape.count, mousePosition);
                            e.Use();
                        }
                    }
                    // Select vertex
                    else
                    {
                        m_VertexSelection = i;
                        e.Use();
                    }
                }
            }

            // Mouse drag, move selected vertex
            if (e.type == EventType.MouseDrag && isSelected)
            {
                if (RaycastPlane(e.mousePosition, out Vector3 mousePosition))
                {
                    m_Context.SetVertexPosition(m_ShapeSelection, m_VertexSelection, mousePosition);
                    e.Use();
                }
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

        private Color GetSelectionColor(bool isSelected)
        {
            return isSelected ? Color.yellow : Color.gray;
        }
    }
}
