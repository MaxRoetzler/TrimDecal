using UnityEngine;

namespace TrimMesh.Editor
{

    public class OperationDeleteVertex : ISplineOperation
    {
        public event ActionCompletedHandler onActionCompleted;

        public bool CanEnter(Event e, SplineSelection selection)
        {
            return e.keyCode == KeyCode.Delete && selection.mode == SelectMode.Vertex && selection.count > 0;
        }

        public void Setup() { }

        public void Perform(Event e, SplineSelection selection, SplineModel model)
        {
            model.RemoveVertex(selection.vertexMask);
            onActionCompleted();
            e.Use();
        }
    }
}