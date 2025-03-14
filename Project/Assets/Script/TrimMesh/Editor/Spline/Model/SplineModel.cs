using Unity.Mathematics;
using System.Collections.Generic;

namespace TrimMesh
{
    public class SplineModel
    {
        private const float k_MergeVertexThreshold = 0.01f;

        private List<Spline> m_Splines;
        private List<SplineVertex> m_Vertices;
        private List<SplineSegment> m_Segments;
        private SplineSerializer m_Serializer;

        public SplineModel(TrimMesh trimMesh)
        {
            m_Splines = new();
            m_Vertices = new();
            m_Segments = new();
            m_Serializer = new(trimMesh);
            m_Serializer.Deserialize(m_Splines, m_Segments, m_Vertices);
        }

        /////////////////////////////////////////////////////////////

        public delegate void ModelChangedHandler();
        public ModelChangedHandler onDataChanged;

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

        public void Update()
        {
            m_Serializer.Deserialize(m_Splines, m_Segments, m_Vertices);
            onDataChanged();
        }

        public void CreateSpline(float3 positionA, float3 positionB)
        {
            Spline spline = new();
            SplineVertex vertexA = new(positionA);
            SplineVertex vertexB = new(positionB);
            SplineSegment segment = new(vertexA, vertexB, spline);

            spline.segments.Add(segment);
            vertexA.segments.Add(segment);
            vertexB.segments.Add(segment);

            m_Segments.Add(segment);
            m_Vertices.Add(vertexA);
            m_Vertices.Add(vertexB);
            m_Splines.Add(spline);

            m_Serializer.Serialize(this);
        }

        public void RemoveSpline(int index)
        {
            Spline spline = m_Splines[index];

            foreach (SplineSegment segment in spline.segments)
            {
                m_Vertices.Remove(segment.vertexA);
                m_Vertices.Remove(segment.vertexB);
                m_Segments.Add(segment);
            }

            m_Splines.RemoveAt(index);
            m_Serializer.Serialize(this);
        }

        public void ExtendSpline(int splineIndex, int vertexIndex, float3 position)
        {
            Spline spline = m_Splines[splineIndex];
            SplineVertex vertexA = m_Vertices[vertexIndex];
            SplineVertex vertexB = new(position);
            SplineSegment segment = new(vertexA, vertexB, spline);

            spline.segments.Add(segment);
            vertexB.segments.Add(segment);

            m_Vertices.Add(vertexB);
            m_Segments.Add(segment);

            m_Serializer.Serialize(this);
        }

        /////////////////////////////////////////////////////////////

        private void RemoveIsolatedVertices()
        {
            List<SplineVertex> isolatedVertices = new();
            foreach (SplineVertex vertex in m_Vertices)
            {
                if (vertex.segments.Count == 0)
                {
                    isolatedVertices.Add(vertex);
                }
            }

            isolatedVertices.ForEach(x => m_Vertices.Remove(x));
        }
    }
}