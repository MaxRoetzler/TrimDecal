using UnityEngine;

namespace TrimMesh.Editor
{
    public class SplineOperation
    {
        private SplineView m_View;
        private SplineModel m_Model;
        private ISplineOperation m_Operation;
        private ISplineOperation[] m_Operations;

        /////////////////////////////////////////////////////////////

        public SplineOperation(SplineModel model, SplineView view)
        {
            m_View = view;
            m_Model = model;
            m_Operations = new ISplineOperation[]
            {
                new OperationCreate(),
                new OperationVertexMove(),
                new OperationVertexRemove(),
                new OperationVertexDissolve(),
                new OperationVertexConnect(),
                new OperationSegmentRemove(),
            };
        }

        /////////////////////////////////////////////////////////////

        public void SceneGUI(Event e)
        {
            if (!e.alt && (e.type == EventType.KeyDown || e.type == EventType.MouseDown))
            {
                if (m_Operation == null)
                {
                    foreach (ISplineOperation operation in m_Operations)
                    {
                        if (operation.CanEnter(e, m_View.selection))
                        {
                            m_Operation = operation;
                            m_Operation.Setup(e, m_View.selection, m_Model);
                            m_Operation.onCompleted += () => m_Operation = null;
                            e.Use();
                            break;
                        }
                    }
                }
            }

            m_Operation?.Perform(e, m_View.selection, m_Model);
        }
    }
}