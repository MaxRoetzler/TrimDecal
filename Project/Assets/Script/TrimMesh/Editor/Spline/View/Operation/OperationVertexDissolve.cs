using UnityEngine;

namespace TrimMesh.Editor
{

    public class OperationVertexDissolve : ISplineOperation
    {
        public event OperationCompletedHandler onCompleted;

        public bool CanEnter(Event e, SplineSelection selection)
        {
            return e.keyCode == KeyCode.Backspace && selection.mode == SelectMode.Vertex && selection.count > 0;
        }

        public void Setup(Event e, SplineSelection selection, SplineModel model) { }

        public void Perform(Event e, SplineSelection selection, SplineModel model)
        {
            model.DissolveVertex(selection.vertexMask);
            onCompleted();
            e.Use();
        }
    }
}