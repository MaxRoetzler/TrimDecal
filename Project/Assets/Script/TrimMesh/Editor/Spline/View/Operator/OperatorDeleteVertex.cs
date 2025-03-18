using UnityEngine;

namespace TrimMesh.Editor
{
    public class OperatorDeleteVertex : Operator
    {
        protected override KeyCode keyCode => KeyCode.Delete;

        /////////////////////////////////////////////////////////////

        public override bool CanEnter(Event e, SplineSelection selection)
        {
            return e.keyCode == keyCode && selection.mode == SelectMode.Vertex;
        }

        public override void Execute(SplineSerializer serializer)
        {
            Debug.Log("Delete Selected Vertices");
        }

        public override void Preview(Event e)
        {

        }
    }
}