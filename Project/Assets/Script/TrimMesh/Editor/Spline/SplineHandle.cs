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

            foreach(SplineVertex vertex in m_TrimMesh.vertices)
            {
                Handles.DotHandleCap(0, vertex.position, Quaternion.identity, 0.02f, EventType.Repaint);
            }
        }
    }
}
