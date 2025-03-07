using System;
using UnityEngine;

namespace TrimMesh
{
    [Serializable]
    public class Spline
    {
        [SerializeReference]
        private SplineSegment[] m_Segments = new SplineSegment[0];

        /////////////////////////////////////////////////////////////
        
        public SplineSegment[] segments
        {
            get => m_Segments;
        }

        public SplineSegment this[int i]
        {
            get => m_Segments[i];
        }
    }
}