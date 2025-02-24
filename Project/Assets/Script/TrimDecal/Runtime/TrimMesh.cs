using Unity.Mathematics;
using UnityEngine;

namespace Project
{
    public static class TrimMesh
    {
        public static void Build(TrimShape shape, float2[] profile, ref Mesh mesh)
        {
            int segmentCount = shape.count - (shape.isClosed ? 0 : 1);
            int profileCount = profile.Length - 1;

            // Create unique vertices per quad
            int vertexCount = segmentCount * profileCount * 4;
            int triangleCount = segmentCount * profileCount * 6;

            int[] triangles = new int[triangleCount];
            Vector3[] normals = new Vector3[vertexCount];
            Vector3[] vertices = new Vector3[vertexCount];
            Vector2[] uv = new Vector2[vertexCount];

            int vertexId = 0;
            int triangleId = 0;

            for (int i = 0; i < segmentCount; i++)
            {
                TrimPoint pointL = shape[i];
                TrimPoint pointR = shape[(i + 1) % shape.count];

                for (int j = 0; j < profileCount; j++)
                {
                    float2 profileB = profile[j];
                    float2 profileT = profile[j + 1];

                    // Vertex coordinates (unique vertices per quad)
                    float3 posBL = pointL.position + (pointL.bisector * profileB.x) + (shape.normal * profileB.y);
                    float3 posBR = pointR.position + (pointR.bisector * profileB.x) + (shape.normal * profileB.y);
                    float3 posTL = pointL.position + (pointL.bisector * profileT.x) + (shape.normal * profileT.y);
                    float3 posTR = pointR.position + (pointR.bisector * profileT.x) + (shape.normal * profileT.y);

                    vertices[vertexId + 0] = posBL;
                    vertices[vertexId + 1] = posBR;
                    vertices[vertexId + 2] = posTL;
                    vertices[vertexId + 3] = posTR;

                    // UV Coordinates
                    float uvEdgeB = math.distance(posBL, posBR);
                    float uvEdgeT = math.distance(posTL, posTR);

                    float uvOffsetL = math.dot(pointL.bisector - pointL.bitangent, pointL.tangentOut);
                    float uvOffsetR = math.dot(pointR.bisector - pointR.bitangent, pointR.tangentIn);
                    bool flipV = (j % 2) == 1;
                    float uvB = flipV ? 0f : 1f;
                    float uvT = flipV ? 1f : 0f;

                    float2 uvBL = new( uvOffsetL * profileB.x, uvB);
                    float2 uvBR = new((uvOffsetL * profileB.x + uvOffsetR * profileT.x) + uvEdgeB, uvB);
                    float2 uvTL = new( uvOffsetL * profileT.x, uvT);
                    float2 uvTR = new((uvOffsetL * profileT.x + uvOffsetR * profileT.x) + uvEdgeT, uvT);

                    uv[vertexId + 0] = uvBL;
                    uv[vertexId + 1] = uvBR;
                    uv[vertexId + 2] = uvTL;
                    uv[vertexId + 3] = uvTR;

                    // Triangle vertex indices CW (flipped) or CCW
                    if (shape.isFlipped)
                    {
                        triangles[triangleId + 0] = vertexId + 0;
                        triangles[triangleId + 1] = vertexId + 1;
                        triangles[triangleId + 2] = vertexId + 2;

                        triangles[triangleId + 3] = vertexId + 1;
                        triangles[triangleId + 4] = vertexId + 3;
                        triangles[triangleId + 5] = vertexId + 2;
                    }
                    else
                    {
                        triangles[triangleId + 0] = vertexId + 0;
                        triangles[triangleId + 1] = vertexId + 2;
                        triangles[triangleId + 2] = vertexId + 1;

                        triangles[triangleId + 3] = vertexId + 1;
                        triangles[triangleId + 4] = vertexId + 2;
                        triangles[triangleId + 5] = vertexId + 3;
                    }

                    triangleId += 6;
                    vertexId += 4;
                }
            }

            mesh.Clear();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uv;

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
        }
    }
}