using System.Collections.Generic;
using System.Collections;
using Unity.Mathematics;

namespace TrimMesh
{
    public class SplineModel
    {
        private List<Spline> m_Splines;
        private List<SplineVertex> m_Vertices;
        private List<SplineSegment> m_Segments;

        public SplineModel()
        {
            m_Splines = new();
            m_Vertices = new();
            m_Segments = new();
        }

        /////////////////////////////////////////////////////////////

        public delegate void ModelChangedHandler(SplineModel model);
        public ModelChangedHandler onModelChanged;

        /////////////////////////////////////////////////////////////

        public List<Spline> splines
        {
            get => m_Splines;
        }

        public int splineCount
        {
            get => m_Splines.Count;
        }

        public List<SplineSegment> segments
        {
            get => m_Segments;
        }

        public int segmentCount
        {
            get => m_Segments.Count;
        }

        public List<SplineVertex> vertices
        {
            get => m_Vertices;
        }

        public int vertexCount
        {
            get => m_Vertices.Count;
        }

        /////////////////////////////////////////////////////////////

        public Spline CreateSpline(float3 positionA, float3 positionB)
        {
            Spline spline = new();
            SplineVertex vertexA = new(positionA);
            SplineVertex vertexB = new(positionB);
            SplineSegment segment = new(vertexA, vertexB, spline);

            spline.segments.Add(segment);
            vertexA.segments.Add(segment);
            vertexB.segments.Add(segment);

            m_Vertices.Add(vertexA);
            m_Vertices.Add(vertexB);
            m_Segments.Add(segment);
            m_Splines.Add(spline);

            onModelChanged(this);
            return spline;
        }

        public void RemoveSpline(int index)
        {
            Spline spline = m_Splines[index];

            foreach (SplineSegment segment in spline.segments)
            {
                m_Vertices.Remove(segment.vertexA);
                m_Vertices.Remove(segment.vertexB);
                m_Segments.Remove(segment);
            }

            m_Splines.RemoveAt(index);
            onModelChanged(this);
        }

        public SplineSegment AppendSegment(Spline spline, SplineVertex vertexA, float3 position)
        {
            SplineVertex vertexB = new(position);
            SplineSegment segment = new(vertexA, vertexB, spline);

            spline.segments.Add(segment);
            vertexA.segments.Add(segment);
            vertexB.segments.Add(segment);

            m_Vertices.Add(vertexB);
            m_Segments.Add(segment);

            onModelChanged(this);
            return segment;
        }

        public void RemoveVertex(BitArray vertexMask)
        {
            HashSet<SplineVertex> verticesToRemove = new();
            HashSet<SplineSegment> segmentsToRemove = new();

            for (int i = 0; i < m_Vertices.Count; i++)
            {
                if (vertexMask[i])
                {
                    SplineVertex vertex = m_Vertices[i];
                    verticesToRemove.Add(vertex);

                    foreach (SplineSegment segment in vertex.segments)
                    {
                        SplineVertex connectedVertex = vertex != segment.vertexA ? segment.vertexA : segment.vertexB;
                        connectedVertex.segments.Remove(segment);
                        segmentsToRemove.Add(segment);

                        if (connectedVertex.segments.Count == 0)
                        {
                            verticesToRemove.Add(connectedVertex);
                        }
                    }
                }
            }

            foreach (SplineSegment segment in segmentsToRemove)
            {
                segment.spline.segments.Remove(segment);
                m_Segments.Remove(segment);
            }

            foreach (SplineVertex vertex in verticesToRemove)
            {
                m_Vertices.Remove(vertex);
            }

            for (int i = m_Splines.Count - 1; i >= 0; i--)
            {
                if (m_Splines[i].segmentCount == 0)
                {
                    m_Splines.RemoveAt(i);
                }
            }

            onModelChanged(this);
        }

        public Spline GetSplineFromVertex(SplineVertex vertex)
        {
            foreach (SplineSegment segment in vertex.segments)
            {
                return m_Splines[m_Splines.IndexOf(segment.spline)];
            }
            return default;
        }
    }
}