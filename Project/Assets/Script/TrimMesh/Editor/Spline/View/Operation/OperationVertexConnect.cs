using System.Collections.Generic;
using UnityEngine;

namespace TrimMesh.Editor
{
    public class OperationVertexConnect : ISplineOperation
    {
        private SplineVertex m_VertexA;
        private SplineVertex m_VertexB;

        /////////////////////////////////////////////////////////////

        public event OperationCompletedHandler onCompleted;

        /////////////////////////////////////////////////////////////

        public bool CanEnter(Event e, SplineSelection selection)
        {
            return e.type == EventType.KeyDown && e.shift && e.keyCode == KeyCode.C && selection.count == 2;
        }

        public void Setup(Event e, SplineSelection selection, SplineModel model) { }

        public void Perform(Event e, SplineSelection selection, SplineModel model)
        {
            List<int> vertexIndices = selection.GetSelectedVertexIndices();
            m_VertexA = model.vertices[vertexIndices[0]];
            m_VertexB = model.vertices[vertexIndices[1]];

            // Case 0) A is middle, B is end: Append segment to B
            //   |
            // A •---• B
            //   |   |
            if (m_VertexA.segmentCount > 1 && m_VertexB.segmentCount == 1)
            {
                Spline spline = model.GetSplineFromVertex(m_VertexB);
                model.AppendSegment(spline, m_VertexB, m_VertexA);
                e.Use();
                onCompleted();
                return;
            }

            // Case 1) A is end, B is middle: Append segment to A
            //       |
            // A •---• B
            //   |   |
            if (m_VertexA.segmentCount > 1 && m_VertexB.segmentCount == 1)
            {

                Spline spline = model.GetSplineFromVertex(m_VertexA);
                model.AppendSegment(spline, m_VertexA, m_VertexB);
                e.Use();
                onCompleted();
                return;
            }

            // Case 2) A is middle, B is middle: Create new spline
            //       |
            // A •---• B
            //   |   |
            if (m_VertexA.segmentCount > 1 && m_VertexB.segmentCount > 1)
            {
                model.CreateSpline(m_VertexA, m_VertexB);
                e.Use();
                onCompleted();
                return;
            }

            // Case 3, 4) A is end, B is end
            // A •---• B
            //   |   |

            Spline splineA = model.GetSplineFromVertex(m_VertexA);
            Spline splineB = model.GetSplineFromVertex(m_VertexB);

            // Case 3) Same spline, append segment
            if (splineA == splineB)
            {
                model.AppendSegment(splineA, m_VertexA, m_VertexB);
            }
            // Case 4) Different splines, merge splines
            else
            {
                model.MergeSplines(splineA, m_VertexA, splineB, m_VertexB);
            }

            e.Use();
            onCompleted();
        }
    }
}