using UnityEditor;
using UnityEngine;

namespace TrimDecal.Editor
{
    public class HandleVertexDelete : HandleBase
    {
        public HandleVertexDelete(HandleData data, TrimSerializer serializer) : base(data, serializer) { }

        /////////////////////////////////////////////////////////////////

        public override bool CanEnter(Event e)
        {
            return e.control && m_Data.vertexIndex > -1;
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
            if (e.type == EventType.MouseUp)
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

                NotifyHandleCompleted();
            }
        }
    }
}
