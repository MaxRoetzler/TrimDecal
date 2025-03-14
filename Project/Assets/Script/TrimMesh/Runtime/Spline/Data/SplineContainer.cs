using System;
using UnityEngine;

namespace TrimMesh
{
    [Serializable]
    public class SplineContainer
    {
        [SerializeField]
        private Vector3[] m_Vertices;
        [SerializeField]
        private SplineData[] m_Splines;
    }
}