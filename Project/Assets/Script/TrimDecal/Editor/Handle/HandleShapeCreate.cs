using UnityEngine;
using UnityEditor;

namespace TrimDecal.Editor
{
    public class HandleShapeCreate : Handle
    {
        private Vector3 m_Origin;
        private Vector3 m_Normal;

        /////////////////////////////////////////////////////////////////

        public HandleShapeCreate(HandleData data, TrimDecalSerializer serializer) : base(data, serializer) { }

        /////////////////////////////////////////////////////////////////

        public override bool CanEnter(Event e)
        {
            return e.type == EventType.MouseDown && m_Data.vertexIndex == -1 && e.shift;
        }

        public override void Perform(Event e)
        {
            EventType eventType = e.GetTypeForControl(m_ControlID);
            RaycastHit hit;

            if (eventType == EventType.MouseDown && GUIUtility.hotControl == m_ControlID)
            {
                RaycastUtility.RaycastWorld(e.mousePosition, out hit);
                m_Normal = hit.normal;
                m_Origin = hit.point;
                m_Data.plane.SetNormalAndPosition(m_Normal, m_Origin);
                e.Use();
            }

            if (eventType == EventType.MouseDrag && GUIUtility.hotControl == m_ControlID)
            {
                RaycastUtility.RaycastPlane(m_Data.plane, e.mousePosition, out hit);
                m_Data.position = hit.point;
                e.Use();
            }

            if (eventType == EventType.MouseUp && GUIUtility.hotControl == m_ControlID)
            {
                m_Serializer.CreateShape(m_Normal, m_Origin, m_Data.position);

                m_Data.shapeIndex = m_Data.decal.count - 1;
                m_Data.vertexIndex = 1;

                m_Origin = Vector3.zero;
                m_Normal = Vector3.zero;

                e.Use();
                NotifyHandleCompleted();
            }
        }

        public override void Preview(Event e)
        {
            Handles.color = Color.white;

            if (m_Origin != Vector3.zero && m_Data.position != Vector3.zero)
            {
                Handles.DotHandleCap(m_ControlID, m_Origin, Quaternion.identity, 0.02f, EventType.Repaint);
                Handles.DotHandleCap(m_ControlID, m_Data.position, Quaternion.identity, 0.02f, EventType.Repaint);
                Handles.DrawDottedLine(m_Origin, m_Data.position, k_DottedLineSpace);
            }
        }
    }
}