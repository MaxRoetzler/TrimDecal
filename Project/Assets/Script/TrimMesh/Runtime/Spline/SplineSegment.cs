using System;
using UnityEngine;

namespace TrimMesh
{
    [Serializable]
    public class SplineSegment
    {
        [SerializeReference] 
        private SplineVertex m_VertexA;
        [SerializeReference] 
        private SplineVertex m_VertexB;

        /////////////////////////////////////////////////////////////

        public SplineSegment(SplineVertex vertexA, SplineVertex vertexB)
        {
            m_VertexA = vertexA;
            m_VertexB = vertexB;
        }

        /////////////////////////////////////////////////////////////

        public SplineVertex vertexA
        {
            get => m_VertexA;
        }

        public SplineVertex vertexB
        {
            get => m_VertexB;
        }
    }
}