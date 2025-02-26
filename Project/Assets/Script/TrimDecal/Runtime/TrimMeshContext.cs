using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace TrimDecal
{
    public class TrimMeshContext : IDisposable
    {
        private Mesh m_Mesh;
        private List<Vector2> m_UVs;
        private List<Vector3> m_Vertices;
        private List<int> m_Triangles;

        /////////////////////////////////////////////////////////////////

        public TrimMeshContext(Mesh mesh)
        {
            m_Mesh = mesh;

            m_UVs = new();
            m_Triangles = new();
            m_Vertices = new();
        }

        /////////////////////////////////////////////////////////////////

        public void Add(TrimShape shape, TrimProfile profile)
        {
            int segmentCount = shape.count - (shape.isClosed ? 0 : 1);
            int profileCount = profile.count - 1;
            int vertexId = m_Vertices.Count;

            for (int i = 0; i < segmentCount; i++)
            {
                TrimShapeVertex pointL = shape[i];
                TrimShapeVertex pointR = shape[(i + 1) % shape.count];

                for (int j = 0; j < profileCount; j++)
                {
                    float2 profileB = profile[j].position;
                    float2 profileT = profile[j + 1].position;

                    // Vertex coordinates (unique vertices per quad)
                    float3 posBL = pointL.position + (pointL.bisector * profileB.x) + (shape.normal * profileB.y);
                    float3 posBR = pointR.position + (pointR.bisector * profileB.x) + (shape.normal * profileB.y);
                    float3 posTL = pointL.position + (pointL.bisector * profileT.x) + (shape.normal * profileT.y);
                    float3 posTR = pointR.position + (pointR.bisector * profileT.x) + (shape.normal * profileT.y);

                    m_Vertices.Add(posBL);
                    m_Vertices.Add(posBR);
                    m_Vertices.Add(posTL);
                    m_Vertices.Add(posTR);

                    // UV Coordinates
                    float uvEdgeB = math.distance(posBL, posBR);
                    float uvEdgeT = math.distance(posTL, posTR);

                    float uvOffsetL = math.dot(pointL.bisector - pointL.bitangent, pointL.tangentOut);
                    float uvOffsetR = math.dot(pointR.bisector - pointR.bitangent, pointR.tangentIn);

                    bool flipV = (j % 2) == 1;
                    float uvB = flipV ? 1f : 0f;
                    float uvT = flipV ? 0f : 1f;

                    float2 uvBL = new(uvOffsetL * profileB.x, uvB);
                    float2 uvBR = new((uvOffsetL * profileB.x + uvOffsetR * profileT.x) + uvEdgeB, uvB);
                    float2 uvTL = new(uvOffsetL * profileT.x, uvT);
                    float2 uvTR = new((uvOffsetL * profileT.x + uvOffsetR * profileT.x) + uvEdgeT, uvT);

                    m_UVs.Add(uvBL);
                    m_UVs.Add(uvBR);
                    m_UVs.Add(uvTL);
                    m_UVs.Add(uvTR);

                    // Triangle vertex indices CW or CCW
                    if (shape.isFlipped)
                    {
                        m_Triangles.Add(vertexId + 0);
                        m_Triangles.Add(vertexId + 1);
                        m_Triangles.Add(vertexId + 2);

                        m_Triangles.Add(vertexId + 1);
                        m_Triangles.Add(vertexId + 3);
                        m_Triangles.Add(vertexId + 2);
                    }
                    else
                    {
                        m_Triangles.Add(vertexId + 0);
                        m_Triangles.Add(vertexId + 2);
                        m_Triangles.Add(vertexId + 1);

                        m_Triangles.Add(vertexId + 1);
                        m_Triangles.Add(vertexId + 2);
                        m_Triangles.Add(vertexId + 3);
                    }

                    vertexId += 4;
                }
            }
        }

        public void Validate()
        {
            float magnitudeLimit = 10000f;

            for (int i = 0; i < m_Vertices.Count; i++)
            {
                if (float.IsNaN(m_Vertices[i].x) || float.IsNaN(m_Vertices[i].y) || float.IsNaN(m_Vertices[i].z) || float.IsInfinity(m_Vertices[i].x) || float.IsInfinity(m_Vertices[i].y) || float.IsInfinity(m_Vertices[i].z))
                {
                    Debug.LogError($"Invalid vertex at index {i}: {m_Vertices[i]}");
                }

                if (m_Vertices[i].magnitude > magnitudeLimit)
                {
                    Debug.LogError($"Vertex {i} is too far away: {m_Vertices[i]}");
                }
            }

            for (int i = 0; i < m_Triangles.Count; i += 3)
            {
                Vector3 v0 = m_Vertices[m_Triangles[i]];
                Vector3 v1 = m_Vertices[m_Triangles[i + 1]];
                Vector3 v2 = m_Vertices[m_Triangles[i + 2]];

                if (Vector3.Distance(v0, v1) < 0.0001f || Vector3.Distance(v1, v2) < 0.0001f || Vector3.Distance(v2, v0) < 0.0001f)
                {
                    Debug.LogError($"Degenerate triangle at index {i / 3}: {v0}, {v1}, {v2}");
                }
            }
        }

        public void Dispose()
        {
            m_Mesh.Clear();
            m_Mesh.SetVertices(m_Vertices);
            m_Mesh.SetUVs(0, m_UVs);
            m_Mesh.SetTriangles(m_Triangles, 0);

            m_Mesh.RecalculateNormals();
        }
    }
}