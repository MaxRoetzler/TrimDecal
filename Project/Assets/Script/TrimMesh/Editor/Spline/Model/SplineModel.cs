using UnityEngine;
using Unity.Mathematics;
using System.Collections.Generic;

namespace TrimMesh
{
    public class SplineModel
    {
        private const float k_MergeVertexThreshold = 0.01f;

        private List<Spline> m_Splines;
        private List<SplineVertex> m_Vertices;
        private SplineSerializer m_Serializer;

        public SplineModel(TrimMesh trimMesh)
        {
            m_Splines = new();
            m_Vertices = new();
            m_Serializer = new(trimMesh);
            m_Serializer.Deserialize(m_Splines, m_Vertices);
        }

        /////////////////////////////////////////////////////////////

        public List<SplineVertex> vertices
        {
            get => m_Vertices;
        }

        public int vertexCount
        {
            get => m_Vertices.Count;
        }

        public List<Spline> splines
        {
            get => m_Splines;
        }

        public int splineCount
        {
            get => m_Splines.Count;
        }

        /////////////////////////////////////////////////////////////

        public void Update()
        {
            m_Serializer.Deserialize(m_Splines, m_Vertices);
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
            }

            m_Splines.RemoveAt(index);
            m_Serializer.Serialize(this);
        }

        /////////////////////////////////////////////////////////////

        private bool FindOrCreateVertex(float3 position, out SplineVertex vertex)
        {
            foreach (SplineVertex current in m_Vertices)
            {
                if (math.distancesq(position, current.position) > k_MergeVertexThreshold)
                {
                    vertex = current;
                    return true;
                }
            }
            vertex = new SplineVertex(position);
            return false;
        }

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

        public void LogData()
        {
            Debug.Log("===== Debug Validation of Intermediate Model =====");

            // Iterate through all splines
            foreach (var spline in m_Splines)
            {
                Debug.Log($"Spline: {spline}");

                // Iterate through each segment in the spline
                foreach (var segment in spline.segments)
                {
                    Debug.Log($"  Segment: {segment}");

                    // Debug the vertices of the segment
                    Debug.Log($"    Vertex A: {segment.vertexA.position}");
                    Debug.Log($"    Vertex B: {segment.vertexB.position}");

                    // Check if the vertices are properly assigned
                    if (segment.vertexA == null || segment.vertexB == null)
                    {
                        Debug.LogError($"ERROR: Segment has null vertex! Segment: {segment}");
                    }
                }

                // Check if any segments are missing from the spline (e.g., if they're not being added properly)
                if (spline.segments.Count == 0)
                {
                    Debug.LogWarning($"WARNING: Spline {spline} has no segments.");
                }
            }

            // Iterate through all vertices to check their segment relationships
            foreach (var vertex in m_Vertices)
            {
                Debug.Log($"Vertex: {vertex.position}");

                // Check if the vertex has any segments assigned
                if (vertex.segments.Count == 0)
                {
                    Debug.LogWarning($"WARNING: Vertex {vertex.position} has no segments.");
                }

                // Check for null relationships in the segments
                foreach (var segment in vertex.segments)
                {
                    if (segment.vertexA == null || segment.vertexB == null)
                    {
                        Debug.LogError($"ERROR: Vertex {vertex.position} has a segment with null vertices.");
                    }

                    // Check if the vertex is part of the segment correctly
                    if (segment.vertexA != vertex && segment.vertexB != vertex)
                    {
                        Debug.LogError($"ERROR: Vertex {vertex.position} is not part of the segment {segment}. Segment: {segment}");
                    }
                }
            }

            // Print summary
            Debug.Log("===== End of Debug Validation =====");
        }
    }
}