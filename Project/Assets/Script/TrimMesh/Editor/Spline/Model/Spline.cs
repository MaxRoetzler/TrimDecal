using System.Collections.Generic;

namespace TrimMesh
{
    public class Spline
    {
        private List<SplineSegment> m_Segments;

        /////////////////////////////////////////////////////////////

        public Spline()
        {
            m_Segments = new();
        }

        /////////////////////////////////////////////////////////////

        public List<SplineSegment> segments
        {
            get => m_Segments;
        }

        public int segmentCount
        {
            get => m_Segments.Count;
        }
    }
}