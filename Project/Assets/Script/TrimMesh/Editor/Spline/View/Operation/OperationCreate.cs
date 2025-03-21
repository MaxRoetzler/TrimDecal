using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace TrimMesh.Editor
{
    // TODO : Handle consecutive segment creation
    public class OperationCreate : ISplineOperation
    {
        private Plane m_Plane;
        private float3 m_EndPoint;
        private float3 m_StartPoint;
        private bool m_HasStartPoint;
        private SplineVertex m_EndVertex;
        private SplineVertex m_StartVertex;

        /////////////////////////////////////////////////////////////

        public event ActionCompletedHandler onActionCompleted;

        /////////////////////////////////////////////////////////////

        public bool CanEnter(Event e, SplineSelection selection)
        {
            return e.control && e.keyCode == KeyCode.C;
        }

        public void Setup()
        {
            m_EndPoint = default;
            m_EndVertex = default;
            m_StartPoint = default;
            m_StartVertex = default;
            m_HasStartPoint = false;
            m_Plane = new Plane(Vector3.up, 0f);
        }

        public void Perform(Event e, SplineSelection selection, SplineModel model)
        {
            EventType eventType = e.type;
            RaycastHit hit;

            if (eventType == EventType.Repaint)
            {
                if (m_HasStartPoint)
                {
                    Handles.color = SplineConstant.colorDefault;
                    Handles.DrawDottedLine(m_StartPoint, m_EndPoint, SplineConstant.lineDotGap);
                }
            }

            if (eventType == EventType.MouseDown && !e.alt)
            {
                if (e.button == 0)
                {
                    if (!m_HasStartPoint)
                    {
                        if (selection.nearestVertex != -1)
                        {
                            m_StartVertex = model.vertices[selection.nearestVertex];
                            m_StartPoint = m_StartVertex.position;
                            m_EndPoint = m_StartPoint;
                            m_HasStartPoint = true;
                        }
                        else if (RaycastUtility.RaycastPlane(e.mousePosition, m_Plane, out hit))
                        {
                            m_StartPoint = hit.point;
                            m_EndPoint = m_StartPoint;
                            m_HasStartPoint = true;
                        }
                        else
                        {
                            onActionCompleted();
                        }
                    }
                    else
                    {
                        if (selection.nearestVertex != -1)
                        {
                            m_EndVertex = model.vertices[selection.nearestVertex];
                        }
                        CreateSegment(model);
                    }
                    e.Use();
                }
                else if (e.button == 1)
                {
                    e.Use();
                    onActionCompleted();
                }
            }
            else if (eventType == EventType.MouseMove)
            {
                if (RaycastUtility.RaycastPlane(e.mousePosition, m_Plane, out hit))
                {
                    m_EndPoint = hit.point;
                }
                e.Use();
            }
            else if (eventType == EventType.KeyDown && e.keyCode == KeyCode.Escape)
            {
                e.Use();
                onActionCompleted();
            }
        }

        /////////////////////////////////////////////////////////////

        private void CreateSegment(SplineModel model)
        {
            // Create new spline
            if (m_StartVertex == null && m_EndVertex == null)
            {
                Spline spline = model.CreateSpline(m_StartPoint, m_EndPoint);
                SplineSegment segment = spline.segments[0];

                m_StartVertex = segment.vertexB;
                m_StartPoint = m_StartVertex.position;
                m_EndPoint = m_StartPoint;
                return;
            }

            // Append segment to existing spline
            if (m_StartVertex != null && m_EndVertex == null)
            {
                Spline spline = model.GetSplineFromVertex(m_StartVertex);
                SplineSegment segment = model.AppendSegment(spline, m_StartVertex, m_EndPoint);

                m_StartVertex = m_StartVertex != segment.vertexA ? segment.vertexA : segment.vertexB;
                m_StartPoint = m_StartVertex.position;
                m_EndPoint = m_StartPoint;
                m_EndVertex = null;
                return;
            }
        }
    }
}