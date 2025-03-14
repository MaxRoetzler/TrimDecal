using UnityEngine;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine.UIElements;

namespace TrimMesh.Editor
{
    [CustomEditor(typeof(TrimMesh))]
    public class TrimMeshEditor : UnityEditor.Editor
    {
        [SerializeField]
        private VisualTreeAsset m_UxmlEditor;
        [SerializeField]
        private VisualTreeAsset m_UxmlOverlay;

        private SplineView m_View;
        private SplineModel m_Model;
        private TrimMesh m_TrimMesh;
        private TrimMeshOverlay m_Overlay;

        // Fields for testing ...
        private Vector3 m_CreateSplinePositionA;
        private Vector3 m_CreateSplinePositionB;
        private int m_DeleteSplineIndex;
        private int m_ExtendSplineIndex;
        private int m_ExtendSplineVertex;
        private Vector3 m_ExtendSplinePosition;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Spline", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("Box");
            m_CreateSplinePositionA = EditorGUILayout.Vector3Field("Position A", m_CreateSplinePositionA);
            m_CreateSplinePositionB = EditorGUILayout.Vector3Field("Position B", m_CreateSplinePositionB);
            if (GUILayout.Button("Create Spline"))
            {
                m_Model.CreateSpline(m_CreateSplinePositionA, m_CreateSplinePositionB);
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical("Box");
            m_DeleteSplineIndex = EditorGUILayout.IntField("Spline Index", m_DeleteSplineIndex);
            if (GUILayout.Button("Delete Spline"))
            {
                m_Model.RemoveSpline(m_DeleteSplineIndex);
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical("Box");
            m_ExtendSplineIndex = EditorGUILayout.IntField("Spline Index", m_ExtendSplineIndex);
            m_ExtendSplineVertex = EditorGUILayout.IntField("Vertex Index", m_ExtendSplineVertex);
            m_ExtendSplinePosition = EditorGUILayout.Vector3Field("Position", m_ExtendSplinePosition);

            if (GUILayout.Button("Extend Spline"))
            {
                m_Model.ExtendSpline(m_ExtendSplineIndex, m_ExtendSplineVertex, m_ExtendSplinePosition);
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        /////////////////////////////////////////////////////////////

        private void SetupOverlay()
        {
            if (SceneView.lastActiveSceneView != null)
            {
                if (SceneView.lastActiveSceneView.TryGetOverlay("TrimMeshOverlay", out Overlay overlay))
                {
                    m_Overlay = overlay as TrimMeshOverlay;
                    m_Overlay.Setup(m_UxmlOverlay, m_TrimMesh, m_Model, m_View);
                }
            }
        }

        private void ShowOverlay()
        {
            if (m_Overlay != null)
            {
                m_Overlay.displayed = true;
            }
        }

        private void HideOverlay()
        {
            if (m_Overlay != null)
            {
                m_Overlay.displayed = false;
            }
        }

        private void DuringSceneGUI(SceneView sceneView)
        {
            m_View.Update();
        }

        private void OnUndoRedo()
        {
            m_Model.Update();
        }

        /////////////////////////////////////////////////////////////

        private void OnEnable()
        {
            TrimMesh trimMesh = (TrimMesh)target;

            m_Model = new(trimMesh);
            m_View = new(m_Model, trimMesh.transform);

            Undo.undoRedoPerformed += OnUndoRedo;
            SceneView.duringSceneGui += DuringSceneGUI;

            SetupOverlay();
            ShowOverlay();
        }

        /////////////////////////////////////////////////////////////

        private void OnDisable()
        {
            m_View.Dispose();

            HideOverlay();

            Undo.undoRedoPerformed -= OnUndoRedo;
            SceneView.duringSceneGui -= DuringSceneGUI;
        }
    }
}