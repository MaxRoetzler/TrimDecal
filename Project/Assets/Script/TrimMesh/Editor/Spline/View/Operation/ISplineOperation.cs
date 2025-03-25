using UnityEngine;

namespace TrimMesh.Editor
{
    public delegate void OperationCompletedHandler();

    public interface ISplineOperation
    {
        public event OperationCompletedHandler onCompleted;

        public bool CanEnter(Event e, SplineSelection selection);
        public void Setup(Event e, SplineSelection selection, SplineModel model);
        public void Perform(Event e, SplineSelection selection, SplineModel model);
    }
}