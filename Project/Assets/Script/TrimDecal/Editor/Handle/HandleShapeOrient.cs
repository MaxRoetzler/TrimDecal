using UnityEditor;
using UnityEngine;

namespace TrimDecal.Editor
{
    public class HandleShapeOrient : Handle
    {
        private Vector3 m_Origin;
        private TrimShape m_Shape;
        private int[] m_ControlIDs;
        private Vector3[] m_Normals;
        private Quaternion[] m_Directions;

        /////////////////////////////////////////////////////////////////

        public HandleShapeOrient(HandleData data, TrimDecalSerializer serializer) : base(data, serializer) { }

        /////////////////////////////////////////////////////////////////

        public override bool CanEnter(Event e)
        {
            return e.type == EventType.KeyDown && e.keyCode == KeyCode.E && m_Data.shapeIndex > -1;
        }

        public override void Perform(Event e)
        {
            for (int i = 0; i < 6; i++)
            {
                if (e.type == EventType.Layout)
                {
                    Handles.ArrowHandleCap(m_ControlIDs[i], m_Origin, m_Directions[i], 0.5f, EventType.Layout);
                }
                else if (e.type == EventType.MouseDown)
                {
                    if (m_ControlIDs[i] == HandleUtility.nearestControl)
                    {
                        m_Serializer.SetShapeNormal(m_Data.shapeIndex, m_Normals[i]);
                        NotifyHandleCompleted();
                        e.Use();
                        return;
                    }
                }
            }
        }

        public override void Preview(Event e)
        {
            for (int i = 0; i < 6; i++)
            {
                Handles.ArrowHandleCap(m_ControlIDs[i], m_Origin, m_Directions[i], 0.5f, EventType.Repaint);
            }
        }

        public override void Start(Event e)
        {
            m_Data.Setup();
            m_ControlIDs = new int[6]
            {
                GUIUtility.GetControlID(FocusType.Passive),
                GUIUtility.GetControlID(FocusType.Passive),
                GUIUtility.GetControlID(FocusType.Passive),
                GUIUtility.GetControlID(FocusType.Passive),
                GUIUtility.GetControlID(FocusType.Passive),
                GUIUtility.GetControlID(FocusType.Passive),
            };

            m_Directions = new Quaternion[6]
            {
                Quaternion.LookRotation(Vector3.right),
                Quaternion.LookRotation(Vector3.left),
                Quaternion.LookRotation(Vector3.forward),
                Quaternion.LookRotation(Vector3.back),
                Quaternion.LookRotation(Vector3.up),
                Quaternion.LookRotation(Vector3.down),
            };

            m_Normals = new Vector3[6]
            {
                Vector3.right,
                Vector3.left,
                Vector3.forward,
                Vector3.back,
                Vector3.up,
                Vector3.down,
            };

            m_Shape = m_Data.decal[m_Data.shapeIndex];
            m_Origin = m_Shape[0].position;
        }
    }
}
