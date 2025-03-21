using System.Collections.Generic;
using System.Collections;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace TrimMesh.Editor
{
    // TODO : Add Double Click On Element to Select All

    public class SplineSelection
    {
        private const float k_VertexSelectionDistance = 0.12f;
        private const float k_SegmentSelectionDistance = 20.0f;
        private const float k_MarqueeSelectionThreshold = 1.0f;

        private int m_ControlId;
        private SelectMode m_Mode;
        private Rect m_SelectionRect;
        private Vector3 m_SelectionEnd;
        private Vector3 m_SelectionStart;

        private SplineModel m_Model;
        private BitArray m_VertexMask;
        private BitArray m_SegmentMask;
        private int m_NearestVertex;
        private int m_NearestSegment;
        private int m_Count;

        /////////////////////////////////////////////////////////////

        public SplineSelection(SplineModel model)
        {
            m_Model = model;
            m_NearestVertex = -1;
            m_NearestSegment = -1;
            m_Mode = SelectMode.Vertex;

            GetSelectionMask = GetVertexSelection;
            GetSelectionHover = FindNearestVertex;

            AllocateBitmasks(model);
        }

        /////////////////////////////////////////////////////////////

        public enum SelectionType { Default, Additive, Subtractive }

        /////////////////////////////////////////////////////////////

        public int count
        {
            get => m_Count;
        }

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

        public int nearestVertex
        {
            get => m_NearestVertex;
        }

        public int nearestSegment
        {
            get => m_NearestSegment;
        }

        /////////////////////////////////////////////////////////////

        private delegate bool SelectionHandler(int index);
        private delegate void SelectionChangedHandler(SelectionType type, bool selectNearest);
        private SelectionChangedHandler GetSelectionMask;

        private delegate void SelectionHoverHandler();
        private SelectionHoverHandler GetSelectionHover;

        public delegate void SelectModeChangedHandler(SelectMode mode);
        public SelectModeChangedHandler onSelectionModeChanged;

        /////////////////////////////////////////////////////////////

        public void Update(Event e)
        {
            m_ControlId = GUIUtility.GetControlID(FocusType.Passive);
            EventType eventType = e.GetTypeForControl(m_ControlId);

            DetectSelectionInput(e);
            GetSelectionHover();

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
                        GetSelectionMask(GetSelectionType(e), GetSelectionMode());
                        m_SelectionRect = Rect.zero;
                        GUIUtility.hotControl = 0;
                        e.Use();
                    }
                }
            }
        }

        public void AllocateBitmasks(SplineModel model)
        {
            m_Count = 0;
            m_VertexMask = new(m_Model.vertexCount);
            m_SegmentMask = new(m_Model.segmentCount);
        }

        /////////////////////////////////////////////////////////////

        public void SetVertexMode()
        {
            m_Mode = SelectMode.Vertex;
            Deselect();

            GetSelectionMask = GetVertexSelection;
            GetSelectionHover = FindNearestVertex;
            onSelectionModeChanged(m_Mode);
        }

        public void SetSegmentMode()
        {
            m_Mode = SelectMode.Segment;
            Deselect();

            GetSelectionMask = GetSegmentSelection;
            GetSelectionHover = FindNearestSegment;
            onSelectionModeChanged(m_Mode);
        }

        public void SetSplineMode()
        {
            m_Mode = SelectMode.Spline;
            Deselect();

            GetSelectionMask = GetSplineSelection;
            GetSelectionHover = FindNearestSegment;
            onSelectionModeChanged(m_Mode);
        }

        public void Deselect()
        {
            m_Count = 0;
            m_NearestVertex = -1;
            m_NearestSegment = -1;
            m_VertexMask.SetAll(false);
            m_SegmentMask.SetAll(false);
        }

        /////////////////////////////////////////////////////////////

        private bool VertexNearestSelection(int i)
        {
            Vector3 position = m_Model.vertices[i].position;
            float handleSize = HandleUtility.GetHandleSize(position) * k_VertexSelectionDistance;

            return HandleUtility.DistanceToCircle(m_Model.vertices[i].position, handleSize) < handleSize;
        }

        private bool VertexMarqueeSelection(int i)
        {
            return m_SelectionRect.Contains(HandleUtility.WorldToGUIPoint(m_Model.vertices[i].position));
        }

        private void GetVertexSelection(SelectionType type, bool selectNearest)
        {
            SelectionHandler selector = selectNearest ? VertexNearestSelection : VertexMarqueeSelection;

            for (int i = 0; i < m_Model.vertexCount; i++)
            {
                bool isSelected = selector(i);
                UpdateSelectionMask(type, m_VertexMask, i, isSelected);
            }
        }

        private void FindNearestVertex()
        {
            m_NearestVertex = -1;

            for (int i = 0; i < m_Model.vertexCount; i++)
            {
                Vector3 position = m_Model.vertices[i].position;
                float handleSize = HandleUtility.GetHandleSize(position) * k_VertexSelectionDistance;

                if (HandleUtility.DistanceToCircle(m_Model.vertices[i].position, handleSize) < handleSize)
                {
                    m_NearestVertex = i;
                    GUI.changed = true;
                    return;
                }
            }
        }

        /////////////////////////////////////////////////////////////

        private bool SegmentNearestSelection(int i)
        {
            SplineSegment segment = m_Model.segments[i];
            return HandleUtility.DistanceToLine(segment.vertexA.position, segment.vertexB.position) < k_SegmentSelectionDistance;
        }

        private bool SegmentMarqueeSelection(int i)
        {
            Vector3 position = (m_Model.segments[i].vertexA.position + m_Model.segments[i].vertexB.position) * 0.5f;
            return m_SelectionRect.Contains(HandleUtility.WorldToGUIPoint(position));
        }

        private void GetSegmentSelection(SelectionType type, bool selectNearest)
        {
            SelectionHandler selector = selectNearest ? SegmentNearestSelection : SegmentMarqueeSelection;

            for (int i = 0; i < m_Model.segmentCount; i++)
            {
                bool isSelected = selector(i);
                UpdateSelectionMask(type, m_SegmentMask, i, isSelected);
            }
        }

        private void FindNearestSegment()
        {

        }

        /////////////////////////////////////////////////////////////

        private void GetSplineSelection(SelectionType type, bool selectNearest)
        {
            SelectionHandler selector = selectNearest ? SegmentNearestSelection : SegmentMarqueeSelection;
            HashSet<Spline> splineSelection = new();

            for (int i = 0; i < m_Model.segmentCount; i++)
            {
                bool isSelected = selector(i);
                UpdateSelectionMask(type, m_SegmentMask, i, isSelected);

                if (isSelected)
                {
                    splineSelection.Add(m_Model.segments[i].spline);
                }
            }

            for (int i = 0; i < m_Model.segmentCount; i++)
            {
                if (splineSelection.Contains(m_Model.segments[i].spline))
                {
                    m_SegmentMask[i] = true;
                }
            }
        }

        /////////////////////////////////////////////////////////////

        private void UpdateSelectionMask(SelectionType type, BitArray array, int i, bool isSelected)
        {
            bool wasSelected = array[i];

            switch (type)
            {
                case SelectionType.Default: array[i] = isSelected; break;
                case SelectionType.Additive: array[i] |= isSelected; break;
                case SelectionType.Subtractive: array[i] &= !isSelected; break;
            }

            if (wasSelected != array[i])
            {
                m_Count += array[i] ? 1 : -1;
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

        private bool GetSelectionMode()
        {
            return math.length(m_SelectionStart - m_SelectionEnd) < k_MarqueeSelectionThreshold;
        }

        private SelectionType GetSelectionType(Event e)
        {
            return e.control ? SelectionType.Subtractive : e.shift ? SelectionType.Additive : SelectionType.Default;
        }
    }
}