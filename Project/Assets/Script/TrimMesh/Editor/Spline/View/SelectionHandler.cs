using System.Collections;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace TrimMesh.Editor
{
    public class SelectionHandler
    {
        private int m_ControlId;
        private SelectMode m_Mode;
        private Rect m_SelectionRect;
        private Vector3 m_SelectionStart;
        private Vector3 m_SelectionEnd;

        private SplineModel m_Model;
        private BitArray m_SplineMask;
        private BitArray m_VertexMask;
        private BitArray m_SegmentMask;

        /////////////////////////////////////////////////////////////

        public SelectionHandler(SplineModel model)
        {
            m_Model = model;
            m_Mode = SelectMode.None;

            m_VertexMask = new(m_Model.vertexCount);
            m_SegmentMask = new(m_Model.vertexCount); // TODO : Fix! Use global segment list ..
            m_SplineMask = new(m_Model.splineCount);

            UpdateSelection = SelectVertices;
        }

        /////////////////////////////////////////////////////////////

        public SelectMode mode
        {
            get => m_Mode;
        }

        public BitArray vertexMask
        {
            get => m_VertexMask;
        }

        public BitArray segmentMask
        {
            get => m_SegmentMask;
        }

        public BitArray splineMask
        {
            get => m_SplineMask;
        }

        /////////////////////////////////////////////////////////////

        private delegate void MarqueeSelectionHandler();
        private MarqueeSelectionHandler UpdateSelection;

        public delegate void SelectModeChangedHandler(SelectMode mode);
        public SelectModeChangedHandler onModeChanged;

        /////////////////////////////////////////////////////////////

        public void Update(Event e)
        {
            m_ControlId = GUIUtility.GetControlID(FocusType.Passive);
            EventType eventType = e.GetTypeForControl(m_ControlId);

            if (eventType == EventType.Repaint)
            {
                Handles.BeginGUI();
                EditorGUI.DrawRect(m_SelectionRect, new Color(0.3686275f, 0.4666667f, 0.6078432f, 0.2f));
                Handles.EndGUI();
            }
            else if (e.isMouse && !e.alt && e.button == 0)
            {
                if (eventType == EventType.MouseDown)
                {
                    GUIUtility.hotControl = m_ControlId;
                    m_SelectionStart = e.mousePosition;
                    m_SelectionEnd = e.mousePosition;

                    e.Use();
                }

                if (GUIUtility.hotControl == m_ControlId)
                {
                    if (eventType == EventType.MouseDrag)
                    {
                        m_SelectionEnd = e.mousePosition;
                        UpdateSelectionRect();
                        e.Use();
                    }

                    if (eventType == EventType.MouseUp)
                    {
                        UpdateSelection();
                        m_SelectionRect = Rect.zero;

                        GUIUtility.hotControl = 0;
                        e.Use();
                    }
                }
            }
        }

        public void Deselect()
        {
            m_VertexMask = new(m_Model.vertexCount);
            m_VertexMask = new(m_Model.vertexCount);
            m_VertexMask = new(m_Model.vertexCount);
        }

        public void SetVertexMode()
        {
            m_Mode = SelectMode.Vertex;
            m_VertexMask = new(m_Model.vertexCount);
            onModeChanged(m_Mode);
        }

        public void SetSegmentMode()
        {
            m_Mode = SelectMode.Segment;
            m_VertexMask = new(m_Model.vertexCount);
            onModeChanged(m_Mode);
        }

        public void SetSplineMode()
        {
            m_Mode = SelectMode.Spline;
            m_VertexMask = new(m_Model.splineCount);
            onModeChanged(m_Mode);
        }

        /////////////////////////////////////////////////////////////

        private void SelectVertices()
        {
            for (int i = 0; i < m_Model.vertexCount; i++)
            {
                m_VertexMask[i] = m_SelectionRect.Contains(HandleUtility.WorldToGUIPoint(m_Model.vertices[i].position));
            }
        }

        private void SelectSegments()
        {
            // ...
        }

        private void SelectSplines()
        {
            // ...
        }

        private void UpdateSelectionRect()
        {
            m_SelectionRect.xMin = math.min(m_SelectionStart.x, m_SelectionEnd.x);
            m_SelectionRect.xMax = math.max(m_SelectionStart.x, m_SelectionEnd.x);
            m_SelectionRect.yMin = math.min(m_SelectionStart.y, m_SelectionEnd.y);
            m_SelectionRect.yMax = math.max(m_SelectionStart.y, m_SelectionEnd.y);
        }
    }
}