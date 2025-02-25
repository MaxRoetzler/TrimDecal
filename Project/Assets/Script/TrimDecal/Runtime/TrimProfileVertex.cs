using System;
using UnityEngine;

namespace TrimDecal
{
    [Serializable]
    public class TrimProfileVertex
    {
        [SerializeField]
        private Vector2 m_Position;
        [SerializeField]
        private Vector2 m_Texcoord;

        /////////////////////////////////////////////////////////////////
        
        public Vector2 position
        {
            get => m_Position;
        }

        public Vector2 texcoord
        {
            get => m_Texcoord;
        }
    }
}