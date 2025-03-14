using UnityEngine.Rendering;
using UnityEditor;
using UnityEngine;

namespace TrimMesh.Editor
{
    public class SplineView
    {
        private Matrix4x4 m_Matrix;
        private SplineModel m_Model;
        private SelectionHandler m_Selection;

        /////////////////////////////////////////////////////////////

        public SplineView(SplineModel model, SelectionHandler selector, Transform transform)
        {
            m_Model = model;
            m_Matrix = transform.localToWorldMatrix;
            m_Selection = selector;
            m_Selection.onModeChanged += SetSelectionMode;

            HandleInteraction = HandleVertices;
        }

        /////////////////////////////////////////////////////////////

        private delegate void InteractionHandler();
        private InteractionHandler HandleInteraction;

        /////////////////////////////////////////////////////////////

        public void Update()
        {
            Event e = Event.current;
            Handles.matrix = m_Matrix;
            Handles.zTest = CompareFunction.Always;

            DrawSpline();
            DetectSelectionInput(e);
            m_Selection.Update(e);
            HandleInteraction();
        }

        public void Dispose()
        {
            m_Selection.onModeChanged -= SetSelectionMode;
        }

        /////////////////////////////////////////////////////////////

        private void SetSelectionMode(SelectMode mode)
        {
            switch (mode)
            {
                case SelectMode.Vertex:
                    HandleInteraction = HandleVertices;
                    break;
                case SelectMode.Segment:
                    HandleInteraction = HandleSegments;
                    break;
                case SelectMode.Spline:
                    HandleInteraction = HandleSplines;
                    break;
            }
        }

        private void DetectSelectionInput(Event e)
        {
            if (e.type == EventType.KeyDown)
            {
                switch (e.keyCode)
                {
                    case KeyCode.Alpha1: m_Selection.SetVertexMode(); e.Use(); break;
                    case KeyCode.Alpha2: m_Selection.SetSegmentMode(); e.Use(); break;
                    case KeyCode.Alpha3: m_Selection.SetSplineMode(); e.Use(); break;
                }
            }
        }

        /////////////////////////////////////////////////////////////

        private void DrawSpline()
        {
            Handles.color = Color.gray;

            for (int i = 0; i < m_Model.vertexCount; i++)
            {
                SplineVertex vertex = m_Model.vertices[i];
                float handleSize = HandleUtility.GetHandleSize(vertex.position) * 0.04f;

                Handles.color = GetSelectionColor(m_Selection.vertexMask[i]);
                Handles.DotHandleCap(0, vertex.position, Quaternion.identity, handleSize, EventType.Repaint);
            }

            for (int i = 0; i < m_Model.splineCount; i++)
            {
                Spline spline = m_Model.splines[i];

                for (int j = 0; j < spline.segmentCount; j++)
                {
                    SplineSegment segment = spline.segments[j];
                    Vector3 positionA = segment.vertexA.position;
                    Vector3 positionB = segment.vertexB.position;

                    Handles.DrawLine(positionA, positionB);
                }
            }
        }

        private void HandleVertices()
        {
            for (int i = 0; i < m_Model.vertexCount; i++)
            {
                SplineVertex vertex = m_Model.vertices[i];
                float handleSize = HandleUtility.GetHandleSize(vertex.position) * 0.04f;

                Handles.color = GetSelectionColor(m_Selection.vertexMask[i]);
                Handles.DotHandleCap(0, vertex.position, Quaternion.identity, handleSize, EventType.Repaint);
            }
        }

        private void HandleSegments()
        {
            for (int i = 0; i < m_Model.splineCount; i++)
            {
                Spline spline = m_Model.splines[i];

                for (int j = 0; j < spline.segmentCount; j++)
                {
                    Vector3 positionA = spline.segments[j].vertexA.position;
                    Vector3 positionB = spline.segments[j].vertexB.position;

                    Handles.color = GetSelectionColor(m_Selection.vertexMask[i]);
                    Handles.DrawLine(positionA, positionB);

                    SplineSegment segment = spline.segments[j];
                    Vector3 center = (positionA + positionB) / 2.0f;
                    Handles.Label(center, $"{j}");
                }
            }
        }

        private void HandleSplines()
        {
            Handles.color = Color.red;

            foreach (Spline spline in m_Model.splines)
            {
                foreach (SplineSegment segment in spline.segments)
                {
                    Vector3 positionA = segment.vertexA.position;
                    Vector3 positionB = segment.vertexB.position;

                    Handles.DrawLine(positionA, positionB);
                }
            }
        }

        private Color GetSelectionColor(bool state)
        {
            return state ? Color.cyan : Color.gray;
        }
    }
}
