using System.Collections;
using Unity.Mathematics;
using NUnit.Framework;

namespace TrimMesh.Test
{
    [TestFixture]
    public class SplineModelTest
    {
        [Test]
        public void CreateSpline_Should_AddSplineWithTwoVerticesAndOneSegment()
        {
            SplineModel model = GetSplineModel();

            float3 positionA = new(0, 0, 0);
            float3 positionB = new(1, 0, 0);

            Spline createdSpline = model.CreateSpline(positionA, positionB);
            Assert.AreEqual(1, model.splines.Count, "Expected exactly one spline.");
            Assert.AreEqual(1, model.segments.Count, "Expected exactly one segment.");
            Assert.AreEqual(2, model.vertices.Count, "Expected exactly two vertices.");

            SplineSegment segment = model.segments[0];
            Assert.AreSame(createdSpline, segment.spline, "Segment should be linked to the created spline.");
            Assert.AreSame(segment, model.vertices[0].segments.Contains(segment) ? segment : null, "Vertex A should contain the segment.");
            Assert.AreSame(segment, model.vertices[1].segments.Contains(segment) ? segment : null, "Vertex B should contain the segment.");
        }

        [Test]
        public void RemoveSpline_Should_RemoveSplineSegmentsAndVertices()
        {
            SplineModel model = GetSplineModel();

            float3 positionA = new(0, 0, 0);
            float3 positionB = new(1, 0, 0);
            model.CreateSpline(positionA, positionB);

            Assert.AreEqual(1, model.splines.Count, "Expected one spline after creation.");
            Assert.AreEqual(1, model.segments.Count, "Expected one segment after creation.");
            Assert.AreEqual(2, model.vertices.Count, "Expected two vertices after creation.");

            model.RemoveSpline(0);
            Assert.AreEqual(0, model.splines.Count, "Expected no splines after removal.");
            Assert.AreEqual(0, model.segments.Count, "Expected no segments after removal.");
            Assert.AreEqual(0, model.vertices.Count, "Expected no vertices after removal.");
        }

        [Test]
        public void RemoveSpline_Should_NotAffectOtherSplines()
        {
            SplineModel model = GetSplineModel();

            model.CreateSpline(new float3(0, 0, 0), new float3(1, 1, 1));
            model.CreateSpline(new float3(2, 2, 2), new float3(3, 3, 3));

            Assert.AreEqual(2, model.splines.Count, "Expected two splines.");
            Assert.AreEqual(2, model.segments.Count, "Expected two segments.");
            Assert.AreEqual(4, model.vertices.Count, "Expected four vertices.");

            model.RemoveSpline(0);
            Assert.AreEqual(1, model.splines.Count, "Expected one remaining spline.");
            Assert.AreEqual(1, model.segments.Count, "Expected one remaining segment.");
            Assert.AreEqual(2, model.vertices.Count, "Expected two remaining vertices.");

            Spline remainingSpline = model.splines[0];
            Assert.NotNull(remainingSpline, "Remaining spline should not be null.");
            Assert.AreEqual(1, remainingSpline.segmentCount, "Remaining spline should have one segment.");
        }

        [Test]
        public void AppendSegment_ShouldAddSegment()
        {
            SplineModel model = GetSplineModel();

            Spline spline = new();
            SplineVertex vertexA = new(new float3(0, 0, 0));
            float3 position = new(1, 0, 0);

            SplineSegment segment = model.AppendSegment(spline, vertexA, position);
            SplineVertex vertexB = segment.vertexB;

            Assert.IsNotNull(segment);
            Assert.Contains(segment, spline.segments, "Expected spline segments to contain new segment.");
            Assert.Contains(segment, model.segments, "Expected model segments to contain new segment.");
            Assert.Contains(segment, vertexA.segments, "Expected vertexA segments to contain new segment.");
            Assert.Contains(segment, vertexB.segments, "Expected vertexB segments to contain new segment.");
            Assert.AreEqual(position, segment.vertexB.position, "Expected vertex position to match.");
        }

        [Test]
        public void RemoveVertex_ShouldRemoveEmptySplines()
        {
            SplineModel model = GetSplineModel();
            float3 positionA = new(0, 0, 0);
            float3 positionB = new(1, 0, 0);
            float3 positionC = new(1, 0, 1);

            Spline spline = model.CreateSpline(positionA, positionB);
            model.AppendSegment(spline, model.vertices[1], positionC);

            BitArray mask = new(new[] { true, true, false });
            model.RemoveVertex(mask);

            Assert.IsEmpty(model.vertices, "Expected all vertices to be removed.");
            Assert.IsEmpty(model.segments, "Expected all segments to be removed.");
            Assert.IsEmpty(model.splines, "Expected all splines to be removed.");
        }

        [Test]
        public void RemoveVertex_ShouldRemoveMiddleVertexAndBothSegments()
        {
            SplineModel model = GetSplineModel();
            float3 positionA = new(0, 0, 0);
            float3 positionB = new(1, 0, 0);
            float3 positionC = new(1, 0, 1);

            Spline spline = model.CreateSpline(positionA, positionB);
            model.AppendSegment(spline, model.vertices[1], positionC);

            BitArray mask = new(new[] { false, true, false });
            model.RemoveVertex(mask);

            Assert.IsEmpty(model.vertices, "Expected all vertices to be removed.");
            Assert.IsEmpty(model.segments, "Expected all segments to be removed.");
            Assert.IsEmpty(model.splines, "Expected all splines to be removed.");
        }

        [Test]
        public void RemoveVertex_ShouldRemoveOnlyEndVertexAndSegment()
        {
            SplineModel model = GetSplineModel();
            float3 positionA = new(0, 0, 0);
            float3 positionB = new(1, 0, 0);
            float3 positionC = new(1, 0, 1);

            Spline spline = model.CreateSpline(positionA, positionB);
            model.AppendSegment(spline, model.vertices[1], positionC);

            BitArray mask = new(new[] { true, false, false });
            model.RemoveVertex(mask);

            Assert.AreEqual(2, model.vertices.Count, "Expected two vertices to remain.");
            Assert.AreEqual(1, model.segments.Count, "Expected one segment to remain.");
            Assert.AreEqual(1, model.splines.Count, "Expected one spline to remain.");
        }

        [Test]
        public void RemoveVertex_ShouldRemoveIsolatedVertex()
        {
            SplineModel model = GetSplineModel();
            float3 positionA = new(0, 0, 0);
            float3 positionB = new(1, 0, 0);
            float3 positionC = new(1, 0, 1);
            float3 positionD = new(0, 0, 1);

            Spline spline = model.CreateSpline(positionA, positionB);
            model.AppendSegment(spline, model.vertices[1], positionC);
            model.AppendSegment(spline, model.vertices[2], positionD);

            BitArray mask = new(new[] { false, true, false, false });
            model.RemoveVertex(mask);

            Assert.AreEqual(2, model.vertices.Count, "Expected two vertices to be removed, and two remain.");
            Assert.AreEqual(1, model.segments.Count, "Expected two segments to be reomoved, and one remains.");
            Assert.AreEqual(1, model.splines.Count, "Expected the splines to remain.");
        }

        [Test]
        public void RemoveSegment_ShouldRemoveEndSegment()
        {
            SplineModel model = GetSplineModel();
            float3 positionA = new(0, 0, 0);
            float3 positionB = new(1, 0, 0);
            float3 positionC = new(1, 0, 1);

            Spline spline = model.CreateSpline(positionA, positionB);
            model.AppendSegment(spline, model.vertices[1], positionC);

            BitArray mask = new(new[] { true, false});
            model.RemoveSegment(mask);

            Assert.AreEqual(2, model.vertices.Count, "Expected two vertices to remain.");
            Assert.AreEqual(1, model.segments.Count, "Expected one segment to remain.");
            Assert.AreEqual(1, model.splines.Count, "Expected the splines to remain.");
        }

        [Test]
        public void RemoveSegment_ShouldRemoveAllSegments()
        {
            SplineModel model = GetSplineModel();
            float3 positionA = new(0, 0, 0);
            float3 positionB = new(1, 0, 0);
            float3 positionC = new(1, 0, 1);

            Spline spline = model.CreateSpline(positionA, positionB);
            model.AppendSegment(spline, model.vertices[1], positionC);

            BitArray mask = new(new[] { true, true });
            model.RemoveSegment(mask);

            Assert.AreEqual(0, model.vertices.Count, "Expected no vertices to remain.");
            Assert.AreEqual(0, model.segments.Count, "Expected no segments to remain.");
            Assert.AreEqual(0, model.splines.Count, "Expected no splines to remain.");
        }

        [Test]
        public void RemoveSegment_ShouldSplitSplines_WhenMiddleSegmentDeleted()
        {
            SplineModel model = GetSplineModel();
            float3 positionA = new(0, 0, 0);
            float3 positionB = new(1, 0, 0);
            float3 positionC = new(1, 0, 1);
            float3 positionD = new(2, 0, 0);

            Spline spline = model.CreateSpline(positionA, positionB);
            model.AppendSegment(spline, model.vertices[1], positionC);
            model.AppendSegment(spline, model.vertices[2], positionD);

            BitArray mask = new(new[] { false, true, false });
            model.RemoveSegment(mask);

            Assert.AreEqual(4, model.vertices.Count, "Expected four vertices to remain.");
            Assert.AreEqual(2, model.segments.Count, "Expected two segments to remain.");
            Assert.AreEqual(2, model.splines.Count, "Expected two splines to remain, due to the split.");
        }

        /////////////////////////////////////////////////////////////

        private SplineModel GetSplineModel()
        {
            SplineModel model = new();
            model.onModelChanged += (_) => { };

            return model;
        }
    }
}