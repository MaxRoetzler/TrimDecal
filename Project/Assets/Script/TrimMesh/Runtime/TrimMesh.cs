using UnityEngine;

namespace TrimMesh
{
    public class TrimMesh : MonoBehaviour
    {
        [SerializeField]
        public Spline[] m_Splines = new Spline[0];
        [SerializeReference]
        private SplineVertex[] m_Vertices = new SplineVertex[0];

        /////////////////////////////////////////////////////////////
        
        public SplineVertex[] vertices
        {
            get => m_Vertices;
        }

        public int vertexCount
        {
            get => m_Vertices.Length;
        }

        public Spline[] splines
        {
            get => m_Splines;
        }

        public int splineCount
        {
            get => m_Splines.Length;
        }
    }
}