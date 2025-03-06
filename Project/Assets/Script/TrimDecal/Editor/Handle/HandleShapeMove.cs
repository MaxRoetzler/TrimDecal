using UnityEditor;
using UnityEngine;

namespace TrimDecal.Editor
{
    public class HandleShapeMove : Handle
    {
        private int m_CachedControlID;
        private Vector3 m_Offset;
        private Vector3 m_Origin;
        private TrimShape m_Shape;

        /////////////////////////////////////////////////////////////////

        public HandleShapeMove(HandleData data, TrimDecalSerializer serializer) : base(data, serializer) { }

        /////////////////////////////////////////////////////////////////

        public override bool CanEnter(Event e)
        {
            return e.type == EventType.KeyDown && e.keyCode == KeyCode.W && m_Data.shapeIndex > -1;
        }

        public override void Perform(Event e)
        {
            m_Offset = Handles.PositionHandle(m_Offset, Quaternion.identity);
            EventType eventType = e.GetTypeForControl(m_ControlID);

            int currentHotControl = GUIUtility.hotControl;
            if (m_CachedControlID != 0 && currentHotControl == 0)
            {
                Vector3 delta = m_Offset - m_Origin;

                for (int i = 0; i < m_Shape.count; i++)
                {
                    m_Serializer.SetVertexPosition(m_Data.shapeIndex, i, (Vector3)m_Shape[i].position + delta);
                }

                NotifyHandleCompleted();
                e.Use();
            }
            m_CachedControlID = currentHotControl;
        }

        public override void Preview(Event e)
        {
            Handles.color = Color.white;

            int vertexCount = m_Shape.count;
            int closedCount = vertexCount + (m_Shape.isClosed ? 1 : 0);
            Vector3 delta = m_Offset - m_Origin;

            for (int i = 0; i < closedCount; i++)
            {
                Vector3 positionA = m_Shape[i % vertexCount].position;
                Vector3 positionB = m_Shape[(i + 1) % vertexCount].position;
                Handles.DrawDottedLine(positionA + delta, positionB + delta, k_DottedLineSpace);
            }
        }

        public override void Start(Event e)
        {
            m_Data.Setup();
            m_Shape = m_Data.decal[m_Data.shapeIndex];
            m_Origin = m_Shape[0].position;
            m_Offset = m_Shape[0].position;
        }
    }
}
