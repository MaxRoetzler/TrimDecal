using UnityEngine;
using UnityEditor;

namespace TrimDecal.Editor
{
    public class TrimVertexHandle
    {
        private int m_Selection;
        private bool m_IsMouseDrag;

        private Plane m_Plane;
        private TrimPropertyContext m_Context;

        /////////////////////////////////////////////////////////////////
        
        public TrimVertexHandle(TrimPropertyContext context)
        {
            m_Context = context;

            m_IsMouseDrag = false;
            m_Selection = -1;
            m_Plane = new();
        }

        /////////////////////////////////////////////////////////////////

        public void DrawVertex(int i)
        {
            m_Context.SelectVertex(i);
            Vector3 position = m_Context.position;

            Event e = Event.current;
            int controlID = GUIUtility.GetControlID(FocusType.Passive);
            float handleSize = HandleUtility.GetHandleSize(position) * 0.05f;

            Handles.color = i == m_Selection ? Color.yellow : Color.gray;
            Handles.DotHandleCap(controlID, position, Quaternion.identity, 0.02f, EventType.Repaint);

            // Still stupid
            m_Plane = new(m_Context.normal, position);

            // Don't mess with viewport navigation
            if (e.alt)
            {
                return;
            }

            if (e.type == EventType.MouseDown && e.button == 0)
            {
                float distance = HandleUtility.DistanceToCircle(position, handleSize * 1.5f);
                if (distance < handleSize * 2.0f)
                {
                    if (e.control)
                    {
                        m_Context.RemoveVertex(i);
                        m_Selection = -1;
                        e.Use();
                    }

                    if (e.shift && !m_Context.isClosed && i == (m_Context.vertexCount - 1))
                    {
                        Vector3 worldPosition = GetMouseWorldPosition(e.mousePosition, position);
                        m_Context.AddPosition(worldPosition);
                        m_IsMouseDrag = true;
                        m_Selection = i + 1;

                        e.Use();
                    }
                    else
                    {
                        m_IsMouseDrag = true;
                        m_Selection = i;

                        e.Use();
                    }
                }
            }
            
            if (m_IsMouseDrag && m_Selection == i)
            {
                if (e.type == EventType.MouseDrag && e.button == 0)
                {
                    Vector3 worldPosition = GetMouseWorldPosition(e.mousePosition, position);
                    m_Context.position = worldPosition;

                    e.Use();
                }

                if (e.type == EventType.MouseUp && e.button == 0)
                {
                    m_IsMouseDrag = false;
                    e.Use();
                }
            }
        }

        public void ClearSelection()
        {
            m_Selection = -1;
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