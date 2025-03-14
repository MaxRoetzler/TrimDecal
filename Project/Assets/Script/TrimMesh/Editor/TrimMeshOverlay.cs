using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine.UIElements;

namespace TrimMesh.Editor
{
    [Overlay(typeof(SceneView), "TrimMeshOverlay", "Trim Mesh", false)]
    public class TrimMeshOverlay : Overlay
    {
        private SplineView m_View;
        private SplineModel m_Model;
        private TrimMesh m_TrimMesh;
        private VisualTreeAsset m_Uxml;

        private Button m_ButtonModeVertex;
        private Button m_ButtonModeSegment;
        private Button m_ButtonModeSpline;

        /////////////////////////////////////////////////////////////

        public override VisualElement CreatePanelContent()
        {
            VisualElement root = new();
            m_Uxml.CloneTree(root);

            QueryElements(root);
            RegisterEvents();
            return root;
        }

        /////////////////////////////////////////////////////////////

        public void Setup(VisualTreeAsset uxml, TrimMesh trimMesh, SplineModel model, SplineView view)
        {
            m_Uxml = uxml;
            m_View = view;
            m_Model = model;
            m_TrimMesh = trimMesh;
        }

        public override void OnWillBeDestroyed()
        {
            base.OnWillBeDestroyed();
            UnregisterEvents();
        }

        /////////////////////////////////////////////////////////////

        private void OnSelectionModeChanged(SelectMode mode)
        {
            // Set button pressed style -.-
        }

        /////////////////////////////////////////////////////////////

        private void QueryElements(VisualElement root)
        {
            m_ButtonModeVertex = root.Q<Button>("mode_vertex");
            m_ButtonModeSegment = root.Q<Button>("mode_segment");
            m_ButtonModeSpline = root.Q<Button>("mode_spline");
        }

        private void RegisterEvents()
        {
            m_View.selection.onModeChanged += OnSelectionModeChanged;

            m_ButtonModeVertex.clicked += m_View.selection.SetVertexMode;
            m_ButtonModeSegment.clicked += m_View.selection.SetSegmentMode;
            m_ButtonModeSpline.clicked += m_View.selection.SetSplineMode;
        }

        private void UnregisterEvents()
        {
            if (m_View != null)
            {
                m_View.selection.onModeChanged -= OnSelectionModeChanged;
                m_ButtonModeVertex.clicked -= m_View.selection.SetVertexMode;
                m_ButtonModeSegment.clicked -= m_View.selection.SetSegmentMode;
                m_ButtonModeSpline.clicked -= m_View.selection.SetSplineMode;
            }
        }
    }
}