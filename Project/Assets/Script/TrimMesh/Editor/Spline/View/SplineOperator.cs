using UnityEngine;

namespace TrimMesh.Editor
{
    public class SplineOperator
    {
        private SplineSelection m_Selection;
        private SplineSerializer m_Serializer;
        private Operator[] m_Operators;
        private Operator m_Current;

        /////////////////////////////////////////////////////////////

        public SplineOperator(SplineSelection selection, SplineSerializer serializer)
        {
            m_Selection = selection;
            m_Serializer = serializer;
        }

        /////////////////////////////////////////////////////////////
        
        public void Update(Event e)
        {

        }
    }
}