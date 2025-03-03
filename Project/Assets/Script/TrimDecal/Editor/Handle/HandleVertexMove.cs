using UnityEditor;
using UnityEngine;

namespace TrimDecal.Editor
{
    public class HandleVertexMove : HandleBase
    {
        public HandleVertexMove(HandleData data, TrimSerializer serializer) : base(data, serializer) { }

        /////////////////////////////////////////////////////////////////

        public override bool CanEnter(Event e)
        {
            return m_Data.vertexIndex > -1;
        }

        public override void Preview(Event e)
        {
            Handles.DotHandleCap(m_Data.controlID, m_Data.position, Quaternion.identity, 0.02f, EventType.Repaint);
        }

        /////////////////////////////////////////////////////////////////

        protected override void OnEnter(Event e)
        {
            Debug.Log("Enter Vertex Move");
        }

        protected override void OnExit(Event e)
        {
            Debug.Log("Exit Vertex Move");
        }

        /*
        private void PreviewMoveAction()
       {
           Handles.color = Color.white;
           Handles.DotHandleCap(-1, m_Preview.position, Quaternion.identity, 0.02f, EventType.Repaint);

           if (m_Preview.positionIn != null)
           {
               Handles.DrawDottedLine(m_Preview.position, m_Preview.positionIn.Value, k_DottedLineSpace);
           }

           if (m_Preview.positionOut != null)
           {
               Handles.DrawDottedLine(m_Preview.position, m_Preview.positionOut.Value, k_DottedLineSpace);
           }

           // TODO : Validate in/out line segments, check for overlaps and intersections
           m_Preview.isValid = true;
       }

       private void RealizeMoveAction()
       {
           if (m_Preview.isValid)
           {
               if (IsClosedMesh())
               {
                   m_Property.SetShapeClosed(m_ShapeSelection, true);
                   m_Property.RemoveVertex(m_ShapeSelection, m_VertexSelection);
                   return;
               }
               m_Property.SetVertexPosition(m_ShapeSelection, m_VertexSelection, m_Preview.position);
           }
       }

       private void SetupMoveAction()
       {
           GetPreviewPositions();
           PreviewAction = PreviewMoveAction;
           RealizeAction = RealizeMoveAction;
       }
*/
    }
}
