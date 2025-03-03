using UnityEditor;
using UnityEngine;

namespace TrimDecal.Editor
{
    public class HandleVertexInsert : HandleBase
    {
        public HandleVertexInsert(HandleData data, TrimSerializer serializer) : base(data, serializer) { }

        /////////////////////////////////////////////////////////////////

        public override bool CanEnter(Event e)
        {
            return e.shift && m_Data.vertexIndex > -1;
        }

        public override void Preview(Event e)
        {
            Handles.DotHandleCap(m_Data.controlID, m_Data.position, Quaternion.identity, 0.02f, EventType.Repaint);
        }

        /////////////////////////////////////////////////////////////////
        
        protected override void OnEnter(Event e)
        {
            Debug.Log("Enter Vertex Insert");
        }

        protected override void OnExit(Event e)
        {
            Debug.Log("Exit Vertex Insert");
        }

        /*
        private void PreviewInsertAction()
       {
           // TODO : Needs to draw two lines for closed shapes...

           TrimShape shape = m_Decal[m_ShapeSelection];
           bool isInTangent = IsPointingAtInTangent();

           Handles.color = Color.white;
           Handles.DotHandleCap(-1, m_Preview.position, Quaternion.identity, 0.02f, EventType.Repaint);
           Handles.DrawDottedLine(shape[m_VertexSelection].position, m_Preview.position, k_DottedLineSpace);

           if (m_VertexSelection != 0 && m_VertexSelection != shape.count - 1)
           {
               Handles.DrawDottedLine(m_Preview.position, isInTangent ? m_Preview.positionIn.Value : m_Preview.positionOut.Value, k_DottedLineSpace);
           }
           else if (m_VertexSelection == 0 && !isInTangent)
           {
               Handles.DrawDottedLine(m_Preview.position, m_Preview.positionOut.Value, k_DottedLineSpace);
           }
           else if (m_VertexSelection == shape.count - 1 && isInTangent)
           {
               Handles.DrawDottedLine(m_Preview.position, m_Preview.positionIn.Value, k_DottedLineSpace);
           }

           m_Preview.isValid = true;
       }

       private void RealizeInsertAction()
       {
           if (m_Preview.isValid)
           {
               int index;
               bool isInTangent = IsPointingAtInTangent();
               TrimShape shape = m_Decal[m_ShapeSelection];

               // Don't wrap index for last vertex
               if (m_VertexSelection == shape.count - 1)
               {
                   index = isInTangent ? m_VertexSelection : shape.count;
               }
               else
               {
                   index = isInTangent ? m_VertexSelection : (m_VertexSelection + 1) % shape.count;
               }

               m_Property.InsertVertex(m_ShapeSelection, index, m_Preview.position);

               if (IsClosedMesh())
               {
                   m_Property.SetShapeClosed(m_ShapeSelection, true);
               }
           }
       }

       private void SetupInsertAction()
       {
           GetPreviewPositions();
           PreviewAction = PreviewInsertAction;
           RealizeAction = RealizeInsertAction;
       }
*/
    }
}
