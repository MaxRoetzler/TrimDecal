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

        public SplineView(SplineModel model, Transform transform)
        {
            m_Matrix = transform.localToWorldMatrix;

            m_Model = model;
            m_Model.onDataChanged += OnModelDataChanged;

            m_Selection = new(model);
            m_Selection.onModeChanged += OnSelectionModeChanged;
        }

        /////////////////////////////////////////////////////////////

        private delegate void InteractionHandler();
        private InteractionHandler HandleInteraction;

        /////////////////////////////////////////////////////////////

        public SelectionHandler selection
        {
            get => m_Selection;
        }

        /////////////////////////////////////////////////////////////

        public void Update()
        {
            Event e = Event.current;
            Handles.matrix = m_Matrix;
            Handles.zTest = CompareFunction.Always;

            m_Selection.Update(e);
            DrawSpline();
        }

        public void Dispose()
        {
            m_Model.onDataChanged -= OnModelDataChanged;
            m_Selection.onModeChanged -= OnSelectionModeChanged;
        }

        /////////////////////////////////////////////////////////////

        private void OnSelectionModeChanged(SelectMode mode)
        {
            // Change input handling
        }

        private void OnModelDataChanged()
        {
            m_Selection.AllocateData();
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

            for (int i = 0; i < m_Model.segmentCount; i++)
            {
                SplineSegment segment = m_Model.segments[i];
                Vector3 positionA = segment.vertexA.position;
                Vector3 positionB = segment.vertexB.position;

                Handles.color = GetSelectionColor(m_Selection.segmentMask[i]);
                Handles.DrawLine(positionA, positionB);
            }
        }

        private Color GetSelectionColor(bool state)
        {
            return state ? Color.cyan : Color.gray;
        }
    }
}