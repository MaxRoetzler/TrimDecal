using UnityEngine.Rendering;
using UnityEngine;
using UnityEditor;

namespace TrimDecal.Editor
{
    public class TrimDecalHandle
    {
        private const float k_ShapeSelectDistance = 10.0f;

        private IHandle m_Handle;
        private IHandle[] m_Handles;
        private MouseCursor m_Cursor;
        private HandleData m_Data;
        private TrimDecal m_Decal;

        /////////////////////////////////////////////////////////////////

        public TrimDecalHandle(TrimDecal decal, TrimSerializer serializer)
        {
            m_Decal = decal;
            m_Data = new(decal);
            m_Handles = new HandleBase[]
            {
                new HandleVertexDelete(m_Data, serializer),
                new HandleVertexInsert(m_Data, serializer),
                new HandleVertexMove(m_Data, serializer),
                new HandleShapeCreate(m_Data, serializer),
            };
        }

        /////////////////////////////////////////////////////////////////

        public void Draw()
        {
            if (m_Decal.count == 0)
            {
                return;
            }

            Event e = Event.current;
            int controlID = GUIUtility.GetControlID(FocusType.Passive);
            HandleUtility.AddDefaultControl(controlID);
            m_Data.controlID = controlID;

            if (e.type == EventType.Repaint)
            {
                // Draw Scene View handles
                DrawHandles(e);
                m_Handle?.Preview(e);
            }

            // Skip layout update & viewport navigation
            if (e.type == EventType.Layout || e.alt || e.button != 0)
            {
                return;
            }

            if (e.type == EventType.MouseDown)
            {
                GetHandleContext(e);

                foreach (HandleBase handle in m_Handles)
                {
                    if (handle.CanEnter(e))
                    {
                        m_Handle = handle;
                        m_Handle.onCompleted += () => m_Handle = null;
                        m_Handle.Start(e);
                        break;
                    }
                }
            }

            m_Handle?.Perform(e);
        }

        /////////////////////////////////////////////////////////////////

        private void DrawHandles(Event e)
        {
            int controlID = GUIUtility.GetControlID(FocusType.Passive);
            HandleUtility.AddDefaultControl(controlID);
            Handles.zTest = CompareFunction.Always;

            for (int i = 0; i < m_Decal.count; i++)
            {
                if (m_Decal[i].count < 2)
                {
                    continue;
                }

                TrimShape shape = m_Decal[i];
                int vertexCount = shape.count;
                bool isShapeSelected = i == m_Data.shapeIndex;
                int closedCount = vertexCount + (shape.isClosed ? 1 : 0);
                Vector3[] positions = new Vector3[closedCount];

                for (int j = 0; j < closedCount; j++)
                {
                    positions[j] = shape[j % vertexCount].position;

                    if (isShapeSelected && j < vertexCount)
                    {
                        Handles.color = GetSelectionColor(j == m_Data.vertexIndex);
                        Handles.DotHandleCap(controlID, positions[j], Quaternion.identity, 0.02f, EventType.Repaint);
                    }
                }

                Handles.color = GetSelectionColor(isShapeSelected);
                Handles.DrawAAPolyLine(positions);
            }
        }

        private void GetHandleContext(Event e)
        {
            int controlID = GUIUtility.GetControlID(FocusType.Passive);
            HandleUtility.AddDefaultControl(controlID);

            if (e.type == EventType.MouseDown)
            {
                float handleSize = 0.0f;
                float mouseDistance = 0.0f;
                float closestShapeDistance = float.MaxValue;
                int closestShapeIndex = -1;

                for (int i = 0; i < m_Decal.count; i++)
                {
                    if (m_Decal[i].count < 2)
                    {
                        continue;
                    }

                    TrimShape shape = m_Decal[i];
                    int vertexCount = shape.count;
                    int closedCount = vertexCount + (shape.isClosed ? 1 : 0);

                    for (int j = 0; j < vertexCount; j++)
                    {
                        Vector3 position = shape[j].position;
                        handleSize = HandleUtility.GetHandleSize(position) * 0.05f;
                        mouseDistance = HandleUtility.DistanceToCircle(position, handleSize * 2.0f);

                        // Check for vertex click
                        if (mouseDistance < handleSize * 2.0f)
                        {
                            m_Data.shapeIndex = i;
                            m_Data.vertexIndex = j;
                            return;
                        }

                        // Track closest clicked segment
                        Vector3 positionA = shape[(j + 0) % vertexCount].position;
                        Vector3 positionB = shape[(j + 1) % vertexCount].position;
                        mouseDistance = HandleUtility.DistanceToLine(positionA, positionB);

                        if (mouseDistance < closestShapeDistance)
                        {
                            closestShapeDistance = mouseDistance;
                            closestShapeIndex = i;
                        }
                    }

                    if (closestShapeDistance < k_ShapeSelectDistance)
                    {
                        m_Data.shapeIndex = i;
                        m_Data.vertexIndex = -1;
                        return;
                    }
                }
                m_Data.shapeIndex = -1;
                m_Data.vertexIndex = -1;
            }
        }

        private Color GetSelectionColor(bool state)
        {
            return state ? Color.yellow : Color.gray;
        }
    }
}
