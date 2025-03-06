using UnityEditor;
using UnityEngine;

namespace TrimDecal.Editor
{
    public class HandleVertexMove : Handle
    {
        public HandleVertexMove(HandleData data, TrimDecalSerializer serializer) : base(data, serializer) { }

        /////////////////////////////////////////////////////////////////

        public override bool CanEnter(Event e)
        {
            return e.type == EventType.MouseDown && m_Data.vertexIndex > -1;
        }

        public override void Preview(Event e)
        {
            Handles.color = Color.white;
            Handles.DotHandleCap(0, m_Data.position, Quaternion.identity, 0.02f, EventType.Repaint);

            if (m_Data.positionPrev != null)
            {
                Handles.DrawDottedLine(m_Data.position, m_Data.positionPrev.Value, k_DottedLineSpace);
            }

            if (m_Data.positionNext != null)
            {
                Handles.DrawDottedLine(m_Data.position, m_Data.positionNext.Value, k_DottedLineSpace);
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
                if (m_Data.IsClosedMesh())
                {
                    m_Serializer.SetShapeClosed(m_Data.shapeIndex, true);
                    m_Serializer.RemoveVertex(m_Data.shapeIndex, m_Data.vertexIndex);
                }
                else
                {
                    m_Serializer.SetVertexPosition(m_Data.shapeIndex, m_Data.vertexIndex, m_Data.position);
                }

                e.Use();
                NotifyHandleCompleted();
            }
        }
    }
}
