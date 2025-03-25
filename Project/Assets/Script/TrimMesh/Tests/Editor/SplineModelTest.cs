using System.Collections;
using Unity.Mathematics;
using NUnit.Framework;

namespace TrimMesh.Test
{
    [TestFixture]
    public class SplineModelTest
    {
        [Test]
        public void CreateSegment_Should_CreateNewSpline()
        {
            SplineModel model = GetSplineModel();
            float3 positionA = new(0, 0, 0);
            float3 positionB = new(1, 0, 0);

            Spline createdSpline = model.CreateSpline(positionA, positionB);
            Assert.AreEqual(1, model.splines.Count, "Expected 1 spline.");
            Assert.AreEqual(1, model.segments.Count, "Expected 1 segment.");
            Assert.AreEqual(2, model.vertices.Count, "Expected 2 vertices.");

            SplineSegment segment = model.segments[0];
            Assert.AreSame(createdSpline, segment.spline, "Segment should be linked to the created spline.");
            Assert.AreSame(segment, model.vertices[0].segments.Contains(segment) ? segment : null, "Vertex A should contain the segment.");
            Assert.AreSame(segment, model.vertices[1].segments.Contains(segment) ? segment : null, "Vertex B should contain the segment.");
        }

        [Test]
        public void MergeSplines_Should_CombineSplines()
        {
            SplineModel model = GetSplineModel();

            Spline splineA = model.CreateSpline(positionA: new float3(0, 0, 1), positionB: new float3(1, 0, 1));
            Spline splineB = model.CreateSpline(positionA: new float3(0, 0, -1), positionB: new float3(1, 0, -1));
            model.MergeSplines(splineA, model.vertices[0], splineB, model.vertices[2]);

            Assert.AreEqual(1, model.splines.Count, "Expected 1 spline.");
            Assert.AreEqual(3, model.segments.Count, "Expected 3 segments.");
            Assert.AreEqual(4, model.vertices.Count, "Expected 4 vertices.");
        }

        [Test]
        public void RemoveSpline_Should_RemoveSplineSegmentsAndVertices()
        {
            SplineModel model = GetSplineModel();

            float3 positionA = new(0, 0, 0);
            float3 positionB = new(1, 0, 0);
            model.CreateSpline(positionA: positionA, positionB: positionB);

            Assert.AreEqual(1, model.splines.Count, "Expected 1 spline after creation.");
            Assert.AreEqual(1, model.segments.Count, "Expected 1 segment after creation.");
            Assert.AreEqual(2, model.vertices.Count, "Expected 2 vertices after creation.");

            model.RemoveSpline(0);
            Assert.AreEqual(0, model.splines.Count, "Expected no splines after removal.");
            Assert.AreEqual(0, model.segments.Count, "Expected no segments after removal.");
            Assert.AreEqual(0, model.vertices.Count, "Expected no vertices after removal.");
        }

        [Test]
        public void RemoveSpline_Should_NotAffectOtherSplines()
        {
            SplineModel model = GetSplineModel();

            model.CreateSpline(positionA: new float3(0, 0, 0), positionB: new float3(1, 1, 1));
            model.CreateSpline(positionA: new float3(2, 2, 2), positionB: new float3(3, 3, 3));

            Assert.AreEqual(2, model.splines.Count, "Expected 2 splines.");
            Assert.AreEqual(2, model.segments.Count, "Expected 2 segments.");
            Assert.AreEqual(4, model.vertices.Count, "Expected 4 vertices.");

            model.RemoveSpline(0);
            Assert.AreEqual(1, model.splines.Count, "Expected 1 remaining spline.");
            Assert.AreEqual(1, model.segments.Count, "Expected 1 remaining segment.");
            Assert.AreEqual(2, model.vertices.Count, "Expected 2 remaining vertices.");

            Spline remainingSpline = model.splines[0];
            Assert.NotNull(remainingSpline, "Remaining spline should not be null.");
            Assert.AreEqual(1, remainingSpline.segmentCount, "Remaining spline should have 1 segment.");
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

            Assert.AreEqual(2, model.vertices.Count, "Expected 2 vertices to remain.");
            Assert.AreEqual(1, model.segments.Count, "Expected 1 segment to remain.");
            Assert.AreEqual(1, model.splines.Count, "Expected 1 spline to remain.");
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

            Assert.AreEqual(2, model.vertices.Count, "Expected 2 vertices to be removed, and 2 remain.");
            Assert.AreEqual(1, model.segments.Count, "Expected 2 segments to be reomoved, and 1 remains.");
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

            BitArray mask = new(new[] { true, false });
            model.RemoveSegment(mask);

            Assert.AreEqual(2, model.vertices.Count, "Expected 2 vertices to remain.");
            Assert.AreEqual(1, model.segments.Count, "Expected 1 segment to remain.");
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

            Assert.AreEqual(4, model.vertices.Count, "Expected 4 vertices to remain.");
            Assert.AreEqual(2, model.segments.Count, "Expected 2 segments to remain.");
            Assert.AreEqual(2, model.splines.Count, "Expected 2 splines to remain, due to the split.");
        }

        /////////////////////////////////////////////////////////////

        private SplineModel GetSplineModel()
        {
            SplineModel model = new();
            model.onModelChanged += (_, _) => { };

            return model;
        }
    }
}