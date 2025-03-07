using System;
using UnityEngine;

namespace TrimMesh
{
    [Serializable]
    public class SplineVertex
    {
        [SerializeField]
        private Vector3 m_Position;

        /////////////////////////////////////////////////////////////

        public SplineVertex(Vector3 position)
        {
            m_Position = position;
        }

        /////////////////////////////////////////////////////////////

        public Vector3 position
        {
            get => m_Position;
        }
    }
}