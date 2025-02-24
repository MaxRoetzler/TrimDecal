using System;
using UnityEngine;
using Unity.Mathematics;

namespace Project
{
    [Serializable]
    public class TrimPoint
    {
        [SerializeField]
        private float3 m_Position;

        private float3 m_Bisector;
        private float3 m_Bitangent;
        private float3 m_TangentIn;
        private float3 m_TangentOut;

        /////////////////////////////////////////////////////////////////

        public float3 position
        {
            get => m_Position;
            set => m_Position = value;
        }

        public float3 bisector
        {
            get => m_Bisector;
            set => m_Bisector = value;
        }

        public float3 bitangent
        {
            get => m_Bitangent;
            set => m_Bitangent = value;
        }

        public float3 tangentIn
        {
            get => m_TangentIn;
            set => m_TangentIn = value;
        }

        public float3 tangentOut
        {
            get => m_TangentOut;
            set => m_TangentOut = value;
        }

        /////////////////////////////////////////////////////////////////

        public TrimPoint(float3 position)
        {
            m_Position = position;
            m_Bisector = float3.zero;
            m_Bitangent = float3.zero;
            m_TangentIn = float3.zero;
            m_TangentOut = float3.zero;
        }
    }
}