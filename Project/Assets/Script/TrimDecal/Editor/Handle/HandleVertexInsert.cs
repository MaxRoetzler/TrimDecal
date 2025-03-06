using UnityEditor;
using UnityEngine;

namespace TrimDecal.Editor
{
    public class HandleVertexInsert : Handle
    {
        public HandleVertexInsert(HandleData data, TrimDecalSerializer serializer) : base(data, serializer) { }

        /////////////////////////////////////////////////////////////////

        public override bool CanEnter(Event e)
        {
            return e.type == EventType.MouseDown && e.shift && m_Data.vertexIndex > -1;
        }

        public override void Preview(Event e)
        {
            TrimShape shape = m_Data.decal[m_Data.shapeIndex];
            bool isPrevious = m_Data.IsPreviousPosition();

            Handles.color = Color.white;
            Handles.DotHandleCap(-1, m_Data.position, Quaternion.identity, 0.02f, EventType.Repaint);
            Handles.DrawDottedLine(shape[m_Data.vertexIndex].position, m_Data.position, k_DottedLineSpace);

            if (m_Data.vertexIndex != 0 && m_Data.vertexIndex != shape.count - 1)
            {
                Handles.DrawDottedLine(m_Data.position, isPrevious ? m_Data.positionPrev.Value : m_Data.positionNext.Value, k_DottedLineSpace);
            }
            else if (m_Data.vertexIndex == 0 && !isPrevious)
            {
                Handles.DrawDottedLine(m_Data.position, m_Data.positionNext.Value, k_DottedLineSpace);
            }
            else if (m_Data.vertexIndex == shape.count - 1 && isPrevious)
            {
                Handles.DrawDottedLine(m_Data.position, m_Data.positionPrev.Value, k_DottedLineSpace);
            }
        }

        public override void Perform(Event e)
        {
            EventType eventType = e.GetTypeForControl(m_ControlID);

            if (eventType == EventType.MouseDrag && GUIUtility.hotControl == m_ControlID)
            {
                RaycastUtility.RaycastPlane(m_Data.plane, e.mousePosition, out RaycastHit hit);
                m_Data.position = hit.point;
                e.Use();
            }

            if (eventType == EventType.MouseUp && GUIUtility.hotControl == m_ControlID)
            {
                int vertexIndex;
                bool isPrevious = m_Data.IsPreviousPosition();
                TrimShape shape = m_Data.decal[m_Data.shapeIndex];

                // Don't wrap index for last vertex
                if (m_Data.vertexIndex == shape.count - 1)
                {
                    vertexIndex = isPrevious ? m_Data.vertexIndex : shape.count;
                }
                else
                {
                    vertexIndex = isPrevious ? m_Data.vertexIndex : (m_Data.vertexIndex + 1) % shape.count;
                }

                m_Serializer.InsertVertex(m_Data.shapeIndex, vertexIndex, m_Data.position);

                if (m_Data.IsClosedMesh())
                {
                    m_Serializer.SetShapeClosed(m_Data.shapeIndex, true);
                }

                e.Use();
                NotifyHandleCompleted();
            }
        }
    }
}