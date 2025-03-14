using System.Collections;
using System.Collections.Generic;
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
        private BitArray m_VertexMask;
        private BitArray m_SegmentMask;

        /////////////////////////////////////////////////////////////

        public SelectionHandler(SplineModel model)
        {
            m_Model = model;
            m_Mode = SelectMode.None;

            AllocateData();
            UpdateSelectionMask = GetVertexSelection;
        }

        /////////////////////////////////////////////////////////////

        public enum SelectionType { Default, Additive, Subtractive }

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

        /////////////////////////////////////////////////////////////

        private delegate void MarqueeSelectionHandler(SelectionType type);
        private MarqueeSelectionHandler UpdateSelectionMask;

        public delegate void SelectModeChangedHandler(SelectMode mode);
        public SelectModeChangedHandler onModeChanged;

        /////////////////////////////////////////////////////////////

        public void Update(Event e)
        {
            m_ControlId = GUIUtility.GetControlID(FocusType.Passive);
            EventType eventType = e.GetTypeForControl(m_ControlId);

            DetectSelectionInput(e);

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
                        UpdateSelectionMask(GetSelectionType(e));
                        m_SelectionRect = Rect.zero;

                        GUIUtility.hotControl = 0;
                        e.Use();
                    }
                }
            }
        }

        public void AllocateData()
        {
            m_VertexMask = new(m_Model.vertexCount);
            m_SegmentMask = new(m_Model.segmentCount);
        }

        /////////////////////////////////////////////////////////////

        public void SetVertexMode()
        {
            m_Mode = SelectMode.Vertex;
            Deselect();

            UpdateSelectionMask = GetVertexSelection;
            onModeChanged(m_Mode);
        }

        public void SetSegmentMode()
        {
            m_Mode = SelectMode.Segment;
            Deselect();

            UpdateSelectionMask = GetSegmentSelection;
            onModeChanged(m_Mode);
        }

        public void SetSplineMode()
        {
            m_Mode = SelectMode.Spline;
            Deselect();

            UpdateSelectionMask = GetSplineSelection;
            onModeChanged(m_Mode);
        }

        public void Deselect()
        {
            m_VertexMask.SetAll(false);
            m_SegmentMask.SetAll(false);
        }

        /////////////////////////////////////////////////////////////

        private void GetVertexSelection(SelectionType type)
        {
            for (int i = 0; i < m_Model.vertexCount; i++)
            {
                bool isSelected = m_SelectionRect.Contains(HandleUtility.WorldToGUIPoint(m_Model.vertices[i].position));

                switch (type)
                {
                    case SelectionType.Default: m_VertexMask[i] = isSelected; break;
                    case SelectionType.Additive: m_VertexMask[i] |= isSelected; break;
                    case SelectionType.Subtractive: m_VertexMask[i] &= !isSelected; break;
                }
            }
        }

        private void GetSegmentSelection(SelectionType type)
        {
            for (int i = 0; i < m_Model.segmentCount; i++)
            {
                float3 position = (m_Model.segments[i].vertexA.position + m_Model.segments[i].vertexB.position) * 0.5f;
                bool isSelected = m_SelectionRect.Contains(HandleUtility.WorldToGUIPoint(position));

                switch (type)
                {
                    case SelectionType.Default: m_SegmentMask[i] = isSelected; break;
                    case SelectionType.Additive: m_SegmentMask[i] |= isSelected; break;
                    case SelectionType.Subtractive: m_SegmentMask[i] &= !isSelected; break;
                }
            }
        }

        private void GetSplineSelection(SelectionType type)
        {
            HashSet<Spline> selectedSplines = new();

            for (int i = 0; i < m_Model.segmentCount; i++)
            {
                float3 position = (m_Model.segments[i].vertexA.position + m_Model.segments[i].vertexB.position) * 0.5f;
                bool isSelected = m_SelectionRect.Contains(HandleUtility.WorldToGUIPoint(position));

                if (isSelected)
                {
                    selectedSplines.Add(m_Model.segments[i].spline);
                }
            }

            for (int i = 0; i < m_Model.segmentCount; i++)
            {
                SplineSegment segment = m_Model.segments[i];
                bool isSelected = selectedSplines.Contains(segment.spline);

                switch (type)
                {
                    case SelectionType.Default: m_SegmentMask[i] = isSelected; break;
                    case SelectionType.Additive: m_SegmentMask[i] |= isSelected; break;
                    case SelectionType.Subtractive: m_SegmentMask[i] &= !isSelected; break;
                }
            }
        }

        private void UpdateSelectionRect()
        {
            m_SelectionRect.xMin = math.min(m_SelectionStart.x, m_SelectionEnd.x);
            m_SelectionRect.xMax = math.max(m_SelectionStart.x, m_SelectionEnd.x);
            m_SelectionRect.yMin = math.min(m_SelectionStart.y, m_SelectionEnd.y);
            m_SelectionRect.yMax = math.max(m_SelectionStart.y, m_SelectionEnd.y);
        }

        private void DetectSelectionInput(Event e)
        {
            if (e.type == EventType.KeyDown)
            {
                switch (e.keyCode)
                {
                    case KeyCode.Alpha1: SetVertexMode(); e.Use(); break;
                    case KeyCode.Alpha2: SetSegmentMode(); e.Use(); break;
                    case KeyCode.Alpha3: SetSplineMode(); e.Use(); break;
                }
            }
        }

        private SelectionType GetSelectionType(Event e)
        {
            return e.control ? SelectionType.Subtractive : e.shift ? SelectionType.Additive : SelectionType.Default;
        }
    }
}