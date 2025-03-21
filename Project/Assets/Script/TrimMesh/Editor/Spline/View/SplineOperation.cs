using UnityEngine;

namespace TrimMesh.Editor
{
    public class SplineOperation
    {
        private SplineModel m_Model;
        private SplineSelection m_Selection;
        private ISplineOperation m_Operation;
        private ISplineOperation[] m_Operations;

        /////////////////////////////////////////////////////////////

        public SplineOperation(SplineModel model, SplineSelection selection)
        {
            m_Model = model;
            m_Selection = selection;
            m_Operations = new ISplineOperation[]
            {
                new OperationCreate(),
                new OperationDeleteVertex(),
                new OperationDeleteSegment(),
            };
        }

        /////////////////////////////////////////////////////////////

        public void SceneGUI(Event e)
        {
            if (e.isKey && e.type == EventType.KeyDown)
            {
                if (m_Operation == null)
                {
                    foreach (ISplineOperation operation in m_Operations)
                    {
                        if (operation.CanEnter(e, m_Selection))
                        {
                            m_Operation = operation;
                            m_Operation.Setup();
                            m_Operation.onActionCompleted += () => m_Operation = null;
                            e.Use();
                            break;
                        }
                    }
                }
            }

            m_Operation?.Perform(e, m_Selection, m_Model);
        }
    }
}