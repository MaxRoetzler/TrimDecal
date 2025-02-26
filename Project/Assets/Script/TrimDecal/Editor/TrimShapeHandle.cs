using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace TrimDecal.Editor
{
    public class TrimShapeHandle
    {
        private const float k_ShapeSelectionDistance = 20.0f;

        private int m_Selection = -1;
        private Plane m_Plane;

        private TrimPropertyContext m_Context;
        private TrimVertexHandle m_VertexHandle;

        /////////////////////////////////////////////////////////////////

        public TrimShapeHandle(SerializedObject serializedObject)
        {
            m_Context = new(serializedObject);
            m_VertexHandle = new(m_Context);
            m_Plane = new();
        }

        /////////////////////////////////////////////////////////////////

        public void DrawShapes()
        {
            if (!m_Context.hasShapes)
            {
                return;
            }

            Event e = Event.current;
            int controlID = GUIUtility.GetControlID(FocusType.Passive);
            HandleUtility.AddDefaultControl(controlID);

            float closestShapeDistance = float.MaxValue;
            int closestShapeIndex = -1;
            float closestSegmentDistance = float.MaxValue;
            int closestSegmentIndex = -1;

            for (int i = 0; i < m_Context.shapeCount; i++)
            {
                m_Context.SelectShape(i);

                int vertexCount = m_Context.vertexCount;
                if (vertexCount < 2) continue;

                bool isSelected = m_Selection == i;
                int totalCount = vertexCount + (m_Context.isClosed ? 1 : 0);
                Vector3[] positions = new Vector3[totalCount];

                for (int j = 0; j < totalCount; j++)
                {
                    Vector3 positionA = m_Context.GetPosition(j % vertexCount);
                    Vector3 positionB = m_Context.GetPosition((j + 1) % vertexCount);
                    positions[j] = positionA;

                    if (isSelected && j < vertexCount)
                    {
                        m_VertexHandle.DrawVertex(j);
                    }

                    float mouseDistance = HandleUtility.DistanceToLine(positionA, positionB);
                    // Get closest shape
                    if (mouseDistance < closestShapeDistance && mouseDistance < k_ShapeSelectionDistance)
                    {
                        closestShapeDistance = mouseDistance;
                        closestShapeIndex = i;
                    }

                    // Get closest line segment
                    if (mouseDistance < closestSegmentDistance && mouseDistance < k_ShapeSelectionDistance)
                    {
                        closestSegmentDistance = mouseDistance;
                        closestSegmentIndex = j;
                    }
                }

                Handles.zTest = CompareFunction.Always;
                Handles.color = isSelected ? Color.yellow : Color.gray;
                Handles.DrawAAPolyLine(positions);
            }

            if (e.alt)
            {
                return;
            }

            if (e.type == EventType.MouseDown && e.button == 0)
            {
                if (e.shift)
                {
                    if (closestShapeIndex != -1 && closestSegmentIndex != -1)
                    {
                        m_Plane = new(m_Context.normal, m_Context.GetPosition(0));
                        Vector3 position = GetMouseWorldPosition(e.mousePosition, m_Context.position);
                        m_Context.AddPosition(position, closestSegmentIndex);
                    }
                }
                else
                {
                    m_VertexHandle.ClearSelection();
                    m_Selection = closestShapeIndex;
                    e.Use();
                }
            }

            m_Context.ApplyModifiedProperties();
        }

        /////////////////////////////////////////////////////////////////

        private Vector3 SnapToGrid(Vector3 position)
        {
            if (EditorSnapSettings.gridSnapEnabled)
            {
                Vector3 grid = EditorSnapSettings.move;

                return new Vector3()
                {
                    x = Mathf.Round(position.x / grid.x) * grid.x,
                    y = Mathf.Round(position.y / grid.y) * grid.y,
                    z = Mathf.Round(position.z / grid.z) * grid.z,
                };
            }
            return position;
        }

        private Vector3 GetMouseWorldPosition(Vector2 position, Vector3 origin)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(position);

            if (m_Plane.Raycast(ray, out float distance))
            {
                return SnapToGrid(ray.GetPoint(distance));
            }
            return origin;
        }
    }
}