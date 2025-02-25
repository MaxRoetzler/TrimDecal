using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace TrimDecal.Editor
{
    public class TrimShapeHandle
    {
        private const float k_ShapeSelectionDistance = 20.0f;

        private int m_Selection = -1;

        private TrimPropertyContext m_Context;
        private TrimVertexHandle m_VertexHandle;

        /////////////////////////////////////////////////////////////////

        public event SelectionChangedHandler onSelectionChanged;

        /////////////////////////////////////////////////////////////////

        public TrimShapeHandle(SerializedObject serializedObject)
        {
            m_Context = new(serializedObject);
            m_VertexHandle = new(m_Context);
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
                    if (mouseDistance < closestShapeDistance && mouseDistance < k_ShapeSelectionDistance)
                    {
                        closestShapeDistance = mouseDistance;
                        closestShapeIndex = i;
                    }
                }

                Handles.zTest = CompareFunction.Always;
                Handles.color = isSelected ? Color.yellow : Color.gray;
                Handles.DrawAAPolyLine(positions);
            }

            // Check mouse clicks
            if (!e.alt && e.type == EventType.MouseDown && e.button == 0)
            {
                m_VertexHandle.ClearSelection();
                m_Selection = closestShapeIndex;
                e.Use();
            }

            m_Context.ApplyModifiedProperties();
        }
    }
}