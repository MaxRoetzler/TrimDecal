using UnityEngine;

namespace TrimMesh.Editor
{
    public class OperationDeleteSegment : ISplineOperation
    {
        public event ActionCompletedHandler onActionCompleted;

        public bool CanEnter(Event e, SplineSelection selection)
        {
            return e.keyCode == KeyCode.Delete && selection.mode == SelectMode.Segment && selection.count > 0;
        }

        public void Setup() { }

        public void Perform(Event e, SplineSelection selection, SplineModel model)
        {
            Debug.Log("Delete segment");
            e.Use();
            onActionCompleted();
        }
    }
}