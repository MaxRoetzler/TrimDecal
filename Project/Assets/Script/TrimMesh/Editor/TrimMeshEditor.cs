using UnityEngine;
using UnityEditor;

namespace TrimMesh.Editor
{
    [CustomEditor(typeof(TrimMesh))]
    public class TrimMeshEditor : UnityEditor.Editor
    {
        private TrimMesh m_TrimMesh;
        private SplineHandle m_Handle;
        private Serializer m_Serializer;
        private int m_DeleteSplineIndex;
        private int m_AddSegmentSplineIndex;
        private int m_AddSegmentVertexIndex;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            m_Serializer.Update();

            EditorGUILayout.BeginVertical("Box");
            if (GUILayout.Button("Add Spline"))
            {
                m_Serializer.CreateSpline(Random.insideUnitSphere, Random.insideUnitSphere);
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical("Box");
            m_DeleteSplineIndex = EditorGUILayout.IntField("Spline", m_DeleteSplineIndex);
            if (GUILayout.Button("Delete Spline"))
            {
                m_Serializer.DeleteSpline(m_DeleteSplineIndex);
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.BeginHorizontal();
            m_AddSegmentSplineIndex = EditorGUILayout.IntField("Spline", m_AddSegmentSplineIndex);
            m_AddSegmentVertexIndex = EditorGUILayout.IntField("Vertex", m_AddSegmentVertexIndex);
            EditorGUILayout.EndHorizontal();
            if (GUILayout.Button("Add Segment"))
            {
                m_Serializer.AddSegment(m_AddSegmentSplineIndex, m_TrimMesh.vertices[m_AddSegmentVertexIndex], Random.insideUnitSphere);
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            m_Serializer.ApplyModifiedProperties();
        }

        /////////////////////////////////////////////////////////////

        private void DuringSceneGUI(SceneView sceneView)
        {
            m_Handle.Draw();
        }

        private void OnEnable()
        {
            m_TrimMesh = (TrimMesh)target;
            m_Serializer = new(m_TrimMesh);
            m_Handle = new(m_TrimMesh);

            SceneView.duringSceneGui += DuringSceneGUI;
        }

        /////////////////////////////////////////////////////////////

        private void OnDisable()
        {
            SceneView.duringSceneGui -= DuringSceneGUI;
        }

        /*
        private void AddSpline()
        {
            int index = m_Splines.arraySize;
            m_Splines.InsertArrayElementAtIndex(index);
            m_Spline = m_Splines.GetArrayElementAtIndex(index);
        }

        private void AddSegment()
        {
            m_Spline = m_Splines.GetArrayElementAtIndex(0);
            m_Segments = m_Spline.FindPropertyRelative(nameof(m_Segments));

            int v = Random.Range(0, m_Vertices.arraySize);
            SplineVertex vertexA = m_Vertices.GetArrayElementAtIndex(v).managedReferenceValue as SplineVertex;
            SplineVertex vertexB = m_Vertices.GetArrayElementAtIndex(v).managedReferenceValue as SplineVertex;

            int index = m_Segments.arraySize;
            m_Segments.InsertArrayElementAtIndex(index);
            m_Segment = m_Segments.GetArrayElementAtIndex(index);
            m_Segment.managedReferenceValue = new SplineSegment(vertexA, vertexB);
        }

        private void AddVertex()
        {
            int index = m_Vertices.arraySize;
            m_Vertices.InsertArrayElementAtIndex(index);
            m_Vertex = m_Vertices.GetArrayElementAtIndex(index);
            m_Vertex.managedReferenceValue = new SplineVertex(Vector3.zero);
        }

        private void SetVertexPosition()
        {
            SerializedProperty vertexProp = m_Vertices.GetArrayElementAtIndex(Random.Range(0, m_Vertices.arraySize));
            SerializedProperty positionProp = vertexProp.FindPropertyRelative("m_Position");
            positionProp.vector3Value = Random.insideUnitSphere;
            serializedObject.ApplyModifiedProperties();
        }

        private void ShuffleVertexReferences()
        {
            m_Spline = m_Splines.GetArrayElementAtIndex(Random.Range(0, m_Splines.arraySize));
            m_Segments = m_Spline.FindPropertyRelative(nameof(m_Segments));
            m_Segment = m_Segments.GetArrayElementAtIndex(Random.Range(0, m_Segments.arraySize));

            SplineVertex vertA = m_Vertices.GetArrayElementAtIndex(Random.Range(0, m_Vertices.arraySize)).managedReferenceValue as SplineVertex;
            SplineVertex vertB = m_Vertices.GetArrayElementAtIndex(Random.Range(0, m_Vertices.arraySize)).managedReferenceValue as SplineVertex;

            m_Segment.FindPropertyRelative(nameof(m_VertexA)).managedReferenceValue = vertA;
            m_Segment.FindPropertyRelative(nameof(m_VertexB)).managedReferenceValue = vertB;
        }

        private void OnEnable()
        {
            m_Splines = serializedObject.FindProperty(nameof(m_Splines));
            m_Vertices = serializedObject.FindProperty(nameof(m_Vertices));
        }
        */
    }
}