namespace TrimMesh
{
    public class SplineSegment
    {
        public Spline spline;
        public SplineVertex vertexA;
        public SplineVertex vertexB;

        public SplineSegment(SplineVertex vertexA, SplineVertex vertexB, Spline spline)
        {
            this.vertexA = vertexA;
            this.vertexB = vertexB;
            this.spline = spline;
        }
    }
}