using UnityEngine;

namespace TrimMesh.Editor
{
    public class OperationSegmentRemove : ISplineOperation
    {
        public event OperationCompletedHandler onCompleted;

        public bool CanEnter(Event e, SplineSelection selection)
        {
            return e.keyCode == KeyCode.Delete && selection.mode == SelectMode.Segment && selection.count > 0;
        }

        public void Setup(Event e, SplineSelection selection, SplineModel model) { }

        public void Perform(Event e, SplineSelection selection, SplineModel model)
        {
            model.RemoveSegment(selection.segmentMask);
            e.Use();
            onCompleted();
        }
    }
}