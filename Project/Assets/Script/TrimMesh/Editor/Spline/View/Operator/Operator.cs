using UnityEngine;

namespace TrimMesh.Editor
{
    public abstract class Operator
    {
        public delegate void OperatorCompletedHandler();
        public OperatorCompletedHandler onCompleted;

        /////////////////////////////////////////////////////////////

        protected abstract KeyCode keyCode { get; }

        /////////////////////////////////////////////////////////////

        public abstract bool CanEnter(Event e, SplineSelection selection);
        public abstract void Preview(Event e);
        public virtual void Execute(SplineSerializer serializer)
        {
            onCompleted?.Invoke();
        }
    }
}