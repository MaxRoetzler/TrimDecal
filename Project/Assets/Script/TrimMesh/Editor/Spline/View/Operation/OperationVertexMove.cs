using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace TrimMesh.Editor
{
    public class OperationVertexMove : ISplineOperation
    {
        private Plane m_Plane;
        private float3 m_Offset;
        private float3 m_EndPoint;
        private float3 m_StartPoint;
        private HashSet<SplineVertex> m_Vertices;
        private HashSet<SplineSegment> m_Segments;

        /////////////////////////////////////////////////////////////

        public event OperationCompletedHandler onCompleted;

        /////////////////////////////////////////////////////////////

        public bool CanEnter(Event e, SplineSelection selection)
        {
            return e.type == EventType.MouseDown && selection.nearestVertex > -1;
        }

        public void Setup(Event e, SplineSelection selection, SplineModel model)
        {
            m_StartPoint = model.vertices[selection.nearestVertex].position;
            m_EndPoint = m_StartPoint;
            m_Offset = Vector3.zero;

            m_Plane = new Plane(Vector3.up, m_StartPoint);
            m_Vertices = new();
            m_Segments = new();

            selection.SelectNearestVertex();
            PopulatePreview(selection, model);
            e.Use();
        }

        public void Perform(Event e, SplineSelection selection, SplineModel model)
        {
            EventType eventType = e.type;
            RaycastHit hit;

            if (eventType == EventType.Repaint)
            {
                Handles.color = SplineConstant.colorDefault;
                foreach (SplineSegment segment in m_Segments)
                {
                    float3 positionA = segment.vertexA.position;
                    float3 positionB = segment.vertexB.position;

                    if (m_Vertices.Contains(segment.vertexA))
                    {
                        positionA += m_Offset;
                    }

                    if (m_Vertices.Contains(segment.vertexB))
                    {
                        positionB += m_Offset;
                    }

                    Handles.DrawDottedLine(positionA, positionB, SplineConstant.lineDotGap);
                }
            }
            else if (eventType == EventType.MouseDrag)
            {
                if (RaycastUtility.RaycastPlane(e.mousePosition, m_Plane, out hit))
                {
                    m_EndPoint = hit.point;
                    m_Offset = m_EndPoint - m_StartPoint;
                    e.Use();
                }
            }
            else if (eventType == EventType.MouseUp)
            {
                if (e.button == 0)
                {
                    model.TranslateVertex(selection.vertexMask, m_Offset);
                    e.Use();
                    onCompleted();
                }
                else if (e.button == 1)
                {
                    e.Use();
                    onCompleted();
                }
            }
        }

        /////////////////////////////////////////////////////////////

        private void PopulatePreview(SplineSelection selection, SplineModel model)
        {
            for (int i = 0; i < model.vertexCount; i++)
            {
                if (selection.vertexMask[i])
                {
                    SplineVertex vertex = model.vertices[i];
                    m_Vertices.Add(vertex);

                    foreach (SplineSegment segment in vertex.segments)
                    {
                        m_Segments.Add(segment);
                    }
                }
            }
        }
    }
}