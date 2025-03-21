using UnityEngine;

namespace TrimMesh.Editor
{
    public delegate void ActionCompletedHandler();

    public interface ISplineOperation
    {
        public event ActionCompletedHandler onActionCompleted;

        public bool CanEnter(Event e, SplineSelection selection);
        public void Setup();
        public void Perform(Event e, SplineSelection selection, SplineModel model);
    }
}