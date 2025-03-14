using System.Collections.Generic;
using Unity.Mathematics;

namespace TrimMesh
{
    public class SplineVertex
    {
        public float3 position;
        public HashSet<SplineSegment> segments;

        public SplineVertex(float3 position)
        {
            segments = new();
            this.position = position;
        }
    }
}