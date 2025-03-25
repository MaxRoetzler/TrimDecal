using UnityEngine;

namespace TrimMesh.Editor
{

    public class OperationVertexRemove : ISplineOperation
    {
        public event OperationCompletedHandler onCompleted;

        public bool CanEnter(Event e, SplineSelection selection)
        {
            return e.keyCode == KeyCode.Delete && selection.mode == SelectMode.Vertex && selection.count > 0;
        }

        public void Setup(Event e, SplineSelection selection, SplineModel model) { }

        public void Perform(Event e, SplineSelection selection, SplineModel model)
        {
            model.RemoveVertex(selection.vertexMask);
            onCompleted();
            e.Use();
        }
    }
}