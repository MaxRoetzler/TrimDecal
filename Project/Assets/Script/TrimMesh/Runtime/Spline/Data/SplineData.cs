using UnityEngine;
using System;

namespace TrimMesh
{
    [Serializable]
    public struct SplineData
    {
        [SerializeField]
        private SplineSegmentData[] m_Segments;
    }
}