using System;
using Unity.Mathematics;
using UnityEngine;

namespace TrimDecal
{
    [Serializable]
    public class TrimShape
    {
        [SerializeField]
        private TrimShapeVertex[] m_Vertices = new TrimShapeVertex[0];
        [SerializeField]
        private Vector3 m_Normal = new(0f, 1f, 0f);
        [SerializeField]
        private bool m_IsFlipped = false;
        [SerializeField]
        private bool m_IsClosed = false;

        /////////////////////////////////////////////////////////////////

        public int count
        {
            get => m_Vertices.Length;
        }

        public TrimShapeVertex this[int i]
        {
            get => m_Vertices[i];
        }

        public float3 normal
        {
            get => m_Normal;
        }

        public bool isClosed
        {
            get => m_IsClosed && m_Vertices.Length > 2;
        }

        public bool isFlipped
        {
            get => m_IsFlipped;
        }

        /////////////////////////////////////////////////////////////////

        public void Update()
        {
            int vertexCount = m_Vertices.Length;
            if (vertexCount < 2)
            {
                return;
            }

            for (int i = 0; i < vertexCount; i++)
            {
                TrimShapeVertex vertex = m_Vertices[i];

                if (i == 0) // First point
                {
                    if (isClosed)
                    {
                        vertex.tangentOut = math.normalize(m_Vertices[i + 1].position - vertex.position);
                        vertex.tangentIn = math.normalize(m_Vertices[vertexCount - 1].position - vertex.position);
                    }
                    else
                    {
                        vertex.tangentOut = math.normalize(m_Vertices[i + 1].position - vertex.position);
                        vertex.tangentIn = -vertex.tangentOut;
                    }
                }
                else if (i == vertexCount - 1) // Last point
                {
                    if (isClosed)
                    {
                        vertex.tangentOut = math.normalize(m_Vertices[0].position - vertex.position);
                        vertex.tangentIn = math.normalize(m_Vertices[i - 1].position - vertex.position);
                    }
                    else
                    {
                        vertex.tangentOut = math.normalize(vertex.position - m_Vertices[i - 1].position);
                        vertex.tangentIn = -vertex.tangentOut;
                    }
                }
                else // Segment point(s)
                {
                    vertex.tangentIn = math.normalize(m_Vertices[i - 1].position - vertex.position);
                    vertex.tangentOut = math.normalize(m_Vertices[i + 1].position - vertex.position);
                }

                float dot = math.dot(vertex.tangentIn, vertex.tangentOut);
                dot = math.clamp(dot, -1.0f, 1.0f);

                float angle = math.acos(dot);
                float halfAngle = math.sin(angle / 2.0f);
                float scale = (math.abs(halfAngle) > 1e-6f) ? (1.0f / halfAngle) : 1.0f;
                float sign = math.cross(vertex.tangentIn, vertex.tangentOut).y > 0 ? 1 : -1; // combine with is flipped

                vertex.bitangent = math.cross(m_Normal, vertex.tangentOut);
                vertex.bisector = math.cross(m_Normal, math.normalize(vertex.tangentOut - vertex.tangentIn)) * scale;
                vertex.bisector *= m_IsFlipped ? -1 : 1;
            }
        }
    }
}