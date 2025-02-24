using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Project
{
    [Serializable]
    public class TrimShape
    {
        [SerializeField]
        private List<TrimPoint> m_Points = new(0);
        [SerializeField]
        private float m_GridSize = 3.0f;
        [SerializeField]
        private float3 m_Origin = float3.zero;
        [SerializeField]
        private float3 m_Normal = new(0f, 1f, 0f);
        [SerializeField]
        private bool m_IsFlipped = false;
        [SerializeField]
        private bool m_IsClosed = false;

        /////////////////////////////////////////////////////////////////

        public int count
        {
            get => m_Points.Count;
        }

        public TrimPoint this[int i]
        {
            get => m_Points[i];
        }

        public float3 origin
        {
            get => m_Origin;
        }

        public float3 normal
        {
            get => m_Normal;
        }

        public float gridSize
        {
            get => m_GridSize;
        }

        public bool isClosed
        {
            get => m_IsClosed && m_Points.Count > 2;
        }

        public bool isFlipped
        {
            get => m_IsFlipped;
        }

        /////////////////////////////////////////////////////////////////

        public void Update()
        {
            int pointCount = m_Points.Count;

            for (int i = 0; i < pointCount; i++)
            {
                TrimPoint point = m_Points[i];

                if (i == 0) // First point
                {
                    if (isClosed)
                    {
                        point.tangentOut = math.normalize(m_Points[i + 1].position - point.position);
                        point.tangentIn = math.normalize(m_Points[pointCount - 1].position - point.position);
                    }
                    else
                    {
                        point.tangentOut = math.normalize(m_Points[i + 1].position - point.position);
                        point.tangentIn = -point.tangentOut;
                    }
                }
                else if (i == pointCount - 1) // Last point
                {
                    if (isClosed)
                    {
                        point.tangentOut = math.normalize(m_Points[0].position - point.position);
                        point.tangentIn = math.normalize(m_Points[i - 1].position - point.position);
                    }
                    else
                    {
                        point.tangentOut = math.normalize(point.position - m_Points[i - 1].position);
                        point.tangentIn = -point.tangentOut;
                    }
                }
                else // Segment point(s)
                {
                    point.tangentIn = math.normalize(m_Points[i - 1].position - point.position);
                    point.tangentOut = math.normalize(m_Points[i + 1].position - point.position);
                }


                float angle = math.acos(math.dot(point.tangentIn, point.tangentOut));
                float scale = 1.0f / math.sin(angle / 2.0f);
                float sign = math.cross(point.tangentIn, point.tangentOut).y > 0 ? 1 : -1; // combine with is flipped

                point.bitangent = math.cross(m_Normal, point.tangentOut);
                point.bisector = math.cross(m_Normal, math.normalize(point.tangentOut - point.tangentIn)) * scale;
                point.bisector *= m_IsFlipped ? -1 : 1;
            }
        }
    }
}