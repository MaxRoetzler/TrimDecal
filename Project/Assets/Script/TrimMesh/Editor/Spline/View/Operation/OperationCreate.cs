using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace TrimMesh.Editor
{
    public class OperationCreate : ISplineOperation
    {
        private Plane m_Plane;
        private float3 m_PositionA;
        private float3 m_PositionB;
        private bool m_HasPositionA;
        private SplineVertex m_VertexB;
        private SplineVertex m_VertexA;

        /////////////////////////////////////////////////////////////

        public event OperationCompletedHandler onCompleted;

        /////////////////////////////////////////////////////////////

        public bool CanEnter(Event e, SplineSelection selection)
        {
            return e.control && e.keyCode == KeyCode.C;
        }

        public void Setup(Event e, SplineSelection selection, SplineModel model)
        {
            m_VertexA = default;
            m_VertexB = default;
            m_PositionA = default;
            m_PositionB = default;
            m_HasPositionA = false;
            m_Plane = new Plane(Vector3.up, 0f);

            selection.SetVertexMode();
        }

        public void Perform(Event e, SplineSelection selection, SplineModel model)
        {
            EventType eventType = e.type;
            RaycastHit hit;

            if (eventType == EventType.Repaint)
            {
                if (m_HasPositionA)
                {
                    Handles.color = SplineConstant.colorDefault;
                    Handles.DrawDottedLine(m_PositionA, m_PositionB, SplineConstant.lineDotGap);
                }
            }

            if (eventType == EventType.MouseDown && !e.alt)
            {
                if (e.button == 0)
                {
                    if (!m_HasPositionA)
                    {
                        if (selection.nearestVertex != -1)
                        {
                            m_VertexA = model.vertices[selection.nearestVertex];
                            m_PositionA = m_VertexA.position;
                            m_PositionB = m_PositionA;
                            m_HasPositionA = true;
                        }
                        else if (RaycastUtility.RaycastPlane(e.mousePosition, m_Plane, out hit))
                        {
                            m_PositionA = hit.point;
                            m_PositionB = m_PositionA;
                            m_HasPositionA = true;
                        }
                        else
                        {
                            onCompleted();
                        }
                    }
                    else
                    {
                        if (selection.nearestVertex != -1)
                        {
                            m_VertexB = model.vertices[selection.nearestVertex];
                        }
                        CreateSegment(model);
                    }
                    e.Use();
                }
                else if (e.button == 1)
                {
                    e.Use();
                    onCompleted();
                }
            }
            else if (eventType == EventType.MouseMove)
            {
                if (RaycastUtility.RaycastPlane(e.mousePosition, m_Plane, out hit))
                {
                    m_PositionB = hit.point;
                }
                e.Use();
            }
            else if (eventType == EventType.KeyDown && e.keyCode == KeyCode.Escape)
            {
                e.Use();
                onCompleted();
            }
        }

        /////////////////////////////////////////////////////////////

        private void CreateSegment(SplineModel model)
        {
            bool hasVertexA = m_VertexA != null;
            bool hasVertexB = m_VertexB != null;
            int segmentsA = hasVertexA ? m_VertexA.segmentCount : 0;
            int segmentsB = hasVertexB ? m_VertexB.segmentCount : 0;

            // Case 0) A is new, B is new : Create new spline and continue
            // A •---• B
            if (!hasVertexA && !hasVertexB)
            {
                Spline spline = model.CreateSpline(m_PositionA, m_PositionB);
                SplineSegment segment = spline.segments[0];

                m_VertexA = segment.vertexB;
                m_PositionA = m_VertexA.position;
                m_PositionB = m_PositionA;
                return;
            }

            // Case 1) A is middle, B is new : Create new spline and continue
            //   |
            // A •---• B
            //   |
            if (segmentsA > 1 && !hasVertexB)
            {
                Spline spline = model.CreateSpline(m_VertexA, m_PositionB);
                SplineSegment segment = spline.segments[0];

                m_VertexA = segment.vertexB;
                m_PositionA = m_VertexA.position;
                m_PositionB = m_PositionA;
                return;
            }

            // Case 2) A is new, B is middle: Create new spline and exit
            //       |
            // A •---• B
            //       |
            if (!hasVertexA && segmentsB > 1)
            {
                model.CreateSpline(m_PositionA, m_VertexB);
                onCompleted();
                return;
            }

            // Case 3) A is middle, B is middle: Create new spline and exit
            //   |   |
            // A •---• B
            //   |   |
            if (segmentsA > 1 && segmentsB > 1)
            {
                model.CreateSpline(m_VertexA, m_VertexB);
                onCompleted();
                return;
            }

            // Case 4) A is end, B is new: Append new segment to A and continue
            // A •---• B
            //   |
            if (!hasVertexB)
            {
                Spline spline = model.GetSplineFromVertex(m_VertexA);
                SplineSegment segment = model.AppendSegment(spline, m_VertexA, m_PositionB);

                m_VertexA = m_VertexA != segment.vertexA ? segment.vertexA : segment.vertexB;
                m_PositionA = m_VertexA.position;
                m_PositionB = m_PositionA;
                m_VertexB = null;
                return;
            }

            // Case 5) A is new, B is end: Append new segment to B and exit
            // A •---• B
            //       |
            if (!hasVertexA)
            {
                Spline spline = model.GetSplineFromVertex(m_VertexB);
                model.AppendSegment(spline, m_VertexB, m_PositionA);
                onCompleted();
                return;
            }

            // Case 6) A is mid, B is end: Append new segment to B and exit
            //   |
            // A •---• B
            //   |   |
            if (segmentsA > 1 && segmentsB == 1)
            {
                Spline spline = model.GetSplineFromVertex(m_VertexB);
                model.AppendSegment(spline, m_VertexB, m_VertexA);
                onCompleted();
                return;
            }

            // Case 7) A is end, B is mid: Append new segment to A and exit
            //       |
            // A •---• B
            //   |   |
            if (segmentsA == 1 && segmentsB > 1)
            {
                Spline spline = model.GetSplineFromVertex(m_VertexA);
                model.AppendSegment(spline, m_VertexA, m_PositionB);
                onCompleted();
                return;
            }

            // Case 8, 9) A is end, B is end
            // A •---• B
            //   |   |
            if (segmentsA == 1 && segmentsB == 1)
            {
                Spline splineA = model.GetSplineFromVertex(m_VertexA);
                Spline splineB = model.GetSplineFromVertex(m_VertexB);

                // Case 8) Same spline, append segment and exit
                if (splineA == splineB)
                {
                    model.AppendSegment(splineA, m_VertexA, m_VertexB);
                }
                // Case 9) Different splines, merge splines and exit
                else
                {
                    model.MergeSplines(splineA, m_VertexA, splineB, m_VertexB);
                }

                onCompleted();
                return;
            }
        }
    }
}