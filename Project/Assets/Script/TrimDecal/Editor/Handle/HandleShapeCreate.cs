using UnityEngine;
using UnityEditor;

namespace TrimDecal.Editor
{
    public class HandleShapeCreate : HandleBase
    {
        private Vector3 m_Origin;
        private Vector3 m_Normal;

        /////////////////////////////////////////////////////////////////

        public HandleShapeCreate(HandleData data, TrimSerializer serializer) : base(data, serializer) { }

        /////////////////////////////////////////////////////////////////

        public override bool CanEnter(Event e)
        {
            return m_Data.vertexIndex == -1 && e.shift;
        }

        public override void Perform(Event e)
        {
            RaycastHit hit;

            if (e.type == EventType.MouseDown)
            {
                RaycastUtility.RaycastWorld(e.mousePosition, out hit);
                m_Normal = hit.normal;
                m_Origin = hit.point;
                m_Data.plane.SetNormalAndPosition(m_Normal, m_Origin);
            }

            if (e.type == EventType.MouseDrag)
            {
                RaycastUtility.RaycastPlane(m_Data.plane, e.mousePosition, out hit);
                m_Data.position = hit.point;
            }

            if (e.type == EventType.MouseUp)
            {
                m_Serializer.CreateShape(m_Normal, m_Origin, m_Data.position);

                m_Data.shapeIndex = m_Data.decal.count - 1;
                m_Data.vertexIndex = 1;

                m_Origin = Vector3.zero;
                m_Normal = Vector3.zero;

                NotifyHandleCompleted();
            }
        }

        public override void Preview(Event e)
        {
            Handles.color = Color.white;

            if (m_Origin != Vector3.zero && m_Data.position != Vector3.zero)
            {
                Handles.DotHandleCap(m_Data.controlID, m_Origin, Quaternion.identity, 0.02f, EventType.Repaint);
                Handles.DotHandleCap(m_Data.controlID, m_Data.position, Quaternion.identity, 0.02f, EventType.Repaint);
                Handles.DrawDottedLine(m_Origin, m_Data.position, k_DottedLineSpace);
            }
        }
    }
}