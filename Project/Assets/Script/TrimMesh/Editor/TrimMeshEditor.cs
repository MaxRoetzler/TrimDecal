using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System;
using Random = UnityEngine.Random;
using JetBrains.Annotations;

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
        private int m_DeleteSegmentSplineIndex;
        private string m_DeleteSegmentIndices;
        private string m_DeleteVertexIndices;
        private string m_ConnectVertexIndices;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            m_Serializer.Update();

            EditorGUILayout.LabelField("Create Spline", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("Box");
            if (GUILayout.Button("Create"))
            {
                m_Serializer.CreateSpline(Random.insideUnitSphere, Random.insideUnitSphere);
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Delete Spline", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("Box");
            m_DeleteSplineIndex = EditorGUILayout.IntField("Spline", m_DeleteSplineIndex);
            if (GUILayout.Button("Delete"))
            {
                m_Serializer.DeleteSpline(m_DeleteSplineIndex);
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Append Segment", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.BeginHorizontal();
            m_AddSegmentSplineIndex = EditorGUILayout.IntField("Spline", m_AddSegmentSplineIndex);
            m_AddSegmentVertexIndex = EditorGUILayout.IntField("Vertex", m_AddSegmentVertexIndex);
            EditorGUILayout.EndHorizontal();
            if (GUILayout.Button("Append"))
            {
                m_Serializer.AppendSegment(m_AddSegmentSplineIndex, m_TrimMesh.vertices[m_AddSegmentVertexIndex], Random.insideUnitSphere);
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Connect Vertices", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.BeginHorizontal();
            m_ConnectVertexIndices = EditorGUILayout.TextField("Vertices", m_ConnectVertexIndices);
            EditorGUILayout.EndHorizontal();
            if (GUILayout.Button("Connect"))
            {
                int[] indices = m_ConnectVertexIndices.Split(',').Select(selector: Int32.Parse).ToArray();
                m_Serializer.ConnectVertices(indices[0], indices[1]);
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Delete Segments", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.BeginHorizontal();
            m_DeleteSegmentSplineIndex = EditorGUILayout.IntField("Spline", m_DeleteSegmentSplineIndex);
            m_DeleteSegmentIndices = EditorGUILayout.TextField("Segments", m_DeleteSegmentIndices);
            EditorGUILayout.EndHorizontal();
            if (GUILayout.Button("Delete"))
            {
                HashSet<int> indices = m_DeleteSegmentIndices.Split(',').Select(selector: Int32.Parse).ToHashSet();
                m_Serializer.DeleteSegments(m_DeleteSegmentSplineIndex, indices);
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Delete Vertices", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.BeginHorizontal();
            m_DeleteVertexIndices = EditorGUILayout.TextField("Vertices", m_DeleteVertexIndices);
            EditorGUILayout.EndHorizontal();
            if (GUILayout.Button("Delete"))
            {
                HashSet<int> indices = m_DeleteVertexIndices.Split(',').Select(selector: Int32.Parse).ToHashSet();
                m_Serializer.DeleteVertices(indices);
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