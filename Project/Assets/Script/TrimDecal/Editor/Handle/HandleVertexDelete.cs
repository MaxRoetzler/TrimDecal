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
            Handles.DotHandleCap(m_Data.controlID, m_Data.position, Quaternion.identity, 0.02f, EventType.Repaint);
        }

        /////////////////////////////////////////////////////////////////

        protected override void OnEnter(Event e)
        {
            Debug.Log("Enter Vertex Delete");
        }

        protected override void OnExit(Event e)
        {
            Debug.Log("Exit Vertex Delete");
        }

        /*
       private void PreviewDeleteAction()
       {
           TrimShape shape = m_Decal[m_ShapeSelection];
           Handles.color = Color.red;

           if (m_Preview.positionIn != null)
           {
               Handles.DrawAAPolyLine(3.0f, new Vector3[] { shape[m_VertexSelection].position, m_Preview.positionIn.Value });
           }

           if (m_Preview.positionOut != null)
           {
               Handles.DrawAAPolyLine(3.0f, new Vector3[] { shape[m_VertexSelection].position, m_Preview.positionOut.Value });
           }
       }

       private void RealizeDeleteAction()
       {
           TrimShape shape = m_Decal[m_ShapeSelection];

           if (shape.count <= 2)
           {
               m_Property.RemoveShape(m_ShapeSelection);
               m_VertexSelection = -1;
               m_ShapeSelection = -1;
               return;
           }

           m_Property.RemoveVertex(m_ShapeSelection, m_VertexSelection);
           m_VertexSelection = -1;
       }

       private void SetupDeleteAction()
       {
           GetPreviewPositions();
           PreviewAction = PreviewDeleteAction;
           RealizeAction = RealizeDeleteAction;
       }
*/
    }
}
