using UnityEngine;
using UnityEditor;

namespace TrimMesh.Editor
{
    [CustomEditor(typeof(TrimMesh))]
    public class TrimMeshEditor : UnityEditor.Editor
    {
        private SplineView m_View;
        private SplineModel m_Model;
        private Vector3 m_VertexAPosition;
        private Vector3 m_VertexBPosition;
        private int m_SplineIndex;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Parameters", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("Box");
            m_SplineIndex = EditorGUILayout.IntField("Spline Index", m_SplineIndex);
            m_VertexAPosition = EditorGUILayout.Vector3Field("Vertex A", m_VertexAPosition);
            m_VertexBPosition = EditorGUILayout.Vector3Field("Vertex B", m_VertexBPosition);
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();

            if (GUILayout.Button("Create Spline"))
            {
                m_Model.CreateSpline(m_VertexAPosition, m_VertexBPosition);
            }

            if (GUILayout.Button("Delete Spline"))
            {
                m_Model.RemoveSpline(m_SplineIndex);
            }
        }

        /////////////////////////////////////////////////////////////

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
        }

        /////////////////////////////////////////////////////////////

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndoRedo;
            SceneView.duringSceneGui -= DuringSceneGUI;
        }
    }
}