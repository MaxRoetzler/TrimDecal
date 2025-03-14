using UnityEngine;
using System;

namespace TrimMesh
{
    [Serializable]
    public struct SplineSegmentData
    {
        [SerializeField]
        private int m_VertexA;
        [SerializeField]
        private int m_VertexB;
    }
}