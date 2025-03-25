using UnityEngine;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine.UIElements;
using System.Collections;

namespace TrimMesh.Editor
{
    [CustomEditor(typeof(TrimMesh))]
    public class TrimMeshEditor : UnityEditor.Editor
    {
        [SerializeField]
        private VisualTreeAsset m_UxmlEditor;
        [SerializeField]
        private VisualTreeAsset m_UxmlOverlay;

        private Tool m_LastTool;
        private SplineView m_View;
        private SplineModel m_Model;
        private TrimMesh m_TrimMesh;
        private TrimMeshOverlay m_Overlay;
        private SplineOperation m_Operator;
        private SplineSerializer m_Serializer;

        // Fields for testing ...
        private Vector3 m_CreateSplinePositionA;
        private Vector3 m_CreateSplinePositionB;
        private int m_DeleteSplineIndex;
        private int m_ExtendSplineIndex;
        private int m_ExtendSplineVertex;
        private Vector3 m_ExtendSplinePosition;
        private string m_DeleteVertexIndices;

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
                m_Model.CreateSpline(positionA: m_CreateSplinePositionA, positionB: m_CreateSplinePositionB);
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
                Spline spline = m_Model.splines[m_ExtendSplineIndex];
                SplineVertex vertex = m_Model.vertices[m_ExtendSplineVertex];
                m_Model.AppendSegment(spline, vertex, m_ExtendSplinePosition);
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical("Box");
            m_DeleteVertexIndices = EditorGUILayout.TextField("Vertex Indices", m_DeleteVertexIndices);

            if (GUILayout.Button("Delete Vertex"))
            {
                BitArray vertexMask = new(m_Model.vertexCount);
                string[] indices = m_DeleteVertexIndices.Split(',');
                Debug.Log(indices);

                foreach (string index in indices)
                {
                    int i = int.Parse(index);
                    vertexMask[i] = true;
                }

                m_Model.RemoveVertex(vertexMask);
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
            Event e = Event.current;
            m_Operator.SceneGUI(e);
            m_View.SceneGUI(e);
        }

        private void OnUndoRedo()
        {
            m_Serializer.Load(m_Model);
        }

        /////////////////////////////////////////////////////////////

        private void OnEnable()
        {
            TrimMesh trimMesh = (TrimMesh)target;

            m_Model = new();
            m_Serializer = new(m_Model, trimMesh);
            m_View = new(m_Model, trimMesh.transform);
            m_Operator = new(m_Model, m_View);

            m_LastTool = Tools.current;
            Tools.current = Tool.None;

            Undo.undoRedoPerformed += OnUndoRedo;
            SceneView.duringSceneGui += DuringSceneGUI;

            SetupOverlay();
            ShowOverlay();
            OnUndoRedo();
        }

        /////////////////////////////////////////////////////////////

        private void OnDisable()
        {
            HideOverlay();

            Tools.current = m_LastTool;
            Undo.undoRedoPerformed -= OnUndoRedo;
            SceneView.duringSceneGui -= DuringSceneGUI;
        }
    }
}