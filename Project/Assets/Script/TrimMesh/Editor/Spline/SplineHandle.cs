using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace TrimMesh.Editor
{
    public class SplineHandle
    {
        private TrimMesh m_TrimMesh;
        private Matrix4x4 m_Matrix;

        /////////////////////////////////////////////////////////////
        
        public SplineHandle(TrimMesh trimMesh)
        {
            m_TrimMesh = trimMesh;
            m_Matrix = trimMesh.transform.localToWorldMatrix;
        }

        /////////////////////////////////////////////////////////////
        
        public void Draw()
        {
            Handles.matrix = m_Matrix;
            Handles.zTest = CompareFunction.Always;

            DrawSplines();
            DrawVertices();
        }

        /////////////////////////////////////////////////////////////

        private void DrawSplines()
        {
            Handles.color = Color.yellow;

            foreach(Spline spline in m_TrimMesh.splines)
            {
                foreach(SplineSegment segment in spline.segments)
                {
                    Vector3 positionA = segment.vertexA.position;
                    Vector3 positionB = segment.vertexB.position;

                    Handles.DrawLine(positionA, positionB);
                }
            }
        }

        private void DrawVertices()
        {
            Handles.color = Color.gray;

            for (int i = 0; i < m_TrimMesh.vertexCount; i++)
            {
                SplineVertex vertex = m_TrimMesh.vertices[i];

                Handles.DotHandleCap(0, vertex.position, Quaternion.identity, 0.02f, EventType.Repaint);
                Handles.Label(vertex.position, $"{i}");
            }

            for (int i = 0; i < m_TrimMesh.splineCount; i++)
            {
                Spline spline = m_TrimMesh.splines[i];

                for (int j = 0; j < spline.segments.Length; j++)
                {
                    SplineSegment segment = spline.segments[j];
                    Vector3 center = (segment.vertexA.position + segment.vertexB.position) / 2.0f;
                    Handles.Label(center, $"{j}");
                }
            }
        }
    }
}
