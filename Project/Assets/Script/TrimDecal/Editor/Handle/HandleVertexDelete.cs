using UnityEditor;
using UnityEngine;

namespace TrimDecal.Editor
{
    public class HandleVertexDelete : Handle
    {
        public HandleVertexDelete(HandleData data, TrimDecalSerializer serializer) : base(data, serializer) { }

        /////////////////////////////////////////////////////////////////

        public override bool CanEnter(Event e)
        {
            return e.type == EventType.MouseDown && e.control && m_Data.vertexIndex > -1;
        }

        public override void Preview(Event e)
        {
            TrimShape shape = m_Data.decal[m_Data.shapeIndex];
            Handles.color = Color.red;

            if (m_Data.positionPrev != null)
            {
                Handles.DrawAAPolyLine(3.0f, new Vector3[] { shape[m_Data.vertexIndex].position, m_Data.positionPrev.Value });
            }

            if (m_Data.positionNext != null)
            {
                Handles.DrawAAPolyLine(3.0f, new Vector3[] { shape[m_Data.vertexIndex].position, m_Data.positionNext.Value });
            }
        }

        public override void Perform(Event e)
        {
            EventType eventType = e.GetTypeForControl(m_ControlID);

            if (eventType == EventType.MouseUp && GUIUtility.hotControl == m_ControlID)
            {
                TrimShape shape = m_Data.decal[m_Data.shapeIndex];

                if (shape.count <= 2)
                {
                    m_Serializer.RemoveShape(m_Data.shapeIndex);
                    m_Data.vertexIndex = -1;
                    m_Data.shapeIndex = -1;
                }
                else
                {
                    m_Serializer.RemoveVertex(m_Data.shapeIndex, m_Data.vertexIndex);
                    m_Data.vertexIndex = -1;
                }

                e.Use();
                NotifyHandleCompleted();
            }
        }
    }
}
