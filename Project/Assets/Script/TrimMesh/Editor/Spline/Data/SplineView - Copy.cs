using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace TrimMesh.Editor
{
    public class SplineView
    {
        private SplineModel m_Model;
        private Matrix4x4 m_Matrix;

        /////////////////////////////////////////////////////////////
        
        public SplineView(SplineModel model, Transform transform)
        {
            m_Model = model;
            m_Matrix = transform.localToWorldMatrix;
        }

        /////////////////////////////////////////////////////////////
        
        public void Update()
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

            foreach(Spline spline in m_Model.splines)
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

            for (int i = 0; i < m_Model.vertexCount; i++)
            {
                SplineVertex vertex = m_Model.vertices[i];

                Handles.DotHandleCap(0, vertex.position, Quaternion.identity, 0.02f, EventType.Repaint);
                Handles.Label(vertex.position, $"{i}");
            }

            for (int i = 0; i < m_Model.splineCount; i++)
            {
                Spline spline = m_Model.splines[i];

                for (int j = 0; j < spline.segmentCount; j++)
                {
                    SplineSegment segment = spline.segments[j];
                    Vector3 center = (segment.vertexA.position + segment.vertexB.position) / 2.0f;
                    Handles.Label(center, $"{j}");
                }
            }
        }
    }
}
