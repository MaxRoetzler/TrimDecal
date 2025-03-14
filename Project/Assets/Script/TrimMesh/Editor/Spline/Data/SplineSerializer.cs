using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Unity.Mathematics;

namespace TrimMesh
{
    public class SplineSerializer
    {
        private const string k_NameOfContainer = "m_Container";
        private const string k_NameOfSegments = "m_Segments";
        private const string k_NameOfVertices = "m_Vertices";
        private const string k_NameOfSplines = "m_Splines";
        private const string k_NameOfVertexA = "m_VertexA";
        private const string k_NameOfVertexB = "m_VertexB";

        private SerializedObject m_SerializedObject;
        private SerializedProperty m_SplineContainer;
        private SerializedProperty m_Vertices;
        private SerializedProperty m_Splines;

        /////////////////////////////////////////////////////////////

        public SplineSerializer(TrimMesh trimMesh)
        {
            m_SerializedObject = new SerializedObject(trimMesh);
            m_SplineContainer = m_SerializedObject.FindProperty(k_NameOfContainer);
            m_Vertices = m_SplineContainer.FindPropertyRelative(k_NameOfVertices);
            m_Splines = m_SplineContainer.FindPropertyRelative(k_NameOfSplines);
        }

        /////////////////////////////////////////////////////////////

        public void Deserialize(List<Spline> splines, List<SplineSegment> segments, List<SplineVertex> vertices)
        {
            m_SerializedObject.Update(); // Can get out of sync with Undo/Redo

            splines.Clear();
            vertices.Clear();
            segments.Clear();
            Dictionary<int, SplineVertex> vertexLookup = new();

            // Load vertex data
            for (int i = 0; i < m_Vertices.arraySize; i++)
            {
                float3 position = m_Vertices.GetArrayElementAtIndex(i).vector3Value;
                SplineVertex newVertex = new(position);

                vertices.Add(newVertex);
                vertexLookup.Add(i, newVertex);
            }

            // Load spline data
            for (int i = 0; i < m_Splines.arraySize; i++)
            {
                Spline newSpline = new();
                splines.Add(newSpline);

                SerializedProperty propSpline = m_Splines.GetArrayElementAtIndex(i);
                SerializedProperty propSegments = propSpline.FindPropertyRelative(k_NameOfSegments);

                for (int j = 0; j < propSegments.arraySize; j++)
                {
                    SerializedProperty propSegment = propSegments.GetArrayElementAtIndex(j);
                    int indexA = propSegment.FindPropertyRelative(k_NameOfVertexA).intValue;
                    int indexB = propSegment.FindPropertyRelative(k_NameOfVertexB).intValue;

                    SplineVertex vertexA = vertexLookup[indexA];
                    SplineVertex vertexB = vertexLookup[indexB];
                    SplineSegment newSegment = new(vertexA, vertexB, newSpline);

                    segments.Add(newSegment);
                    vertexA.segments.Add(newSegment);
                    vertexB.segments.Add(newSegment);
                    newSpline.segments.Add(newSegment);
                }
            }
            Debug.Log($"Deserialized, Vertices: {vertices.Count}, Segments: {segments.Count} Splines: {splines.Count}");
        }

        public void Serialize(SplineModel splineModel)
        {
            m_Splines.arraySize = splineModel.splineCount;
            m_Vertices.arraySize = splineModel.vertexCount;
            Dictionary<SplineVertex, int> vertexLookup = new();

            // Write vertex data
            for (int i = 0; i < splineModel.vertexCount; i++)
            {
                SerializedProperty vertexProperty = m_Vertices.GetArrayElementAtIndex(i);
                vertexProperty.vector3Value = splineModel.vertices[i].position;
                vertexLookup.Add(splineModel.vertices[i], i);
            }

            // Write spline data
            for (int i = 0; i < splineModel.splineCount; i++)
            {
                Spline spline = splineModel.splines[i];
                SerializedProperty splineProperty = m_Splines.GetArrayElementAtIndex(i);
                SerializedProperty segmentsProperty = splineProperty.FindPropertyRelative(k_NameOfSegments);

                segmentsProperty.arraySize = spline.segmentCount;

                for (int j = 0; j < spline.segmentCount; j++)
                {
                    SerializedProperty segmentProperty = segmentsProperty.GetArrayElementAtIndex(j);
                    segmentProperty.FindPropertyRelative(k_NameOfVertexA).intValue = vertexLookup[spline.segments[j].vertexA];
                    segmentProperty.FindPropertyRelative(k_NameOfVertexB).intValue = vertexLookup[spline.segments[j].vertexB];
                }
            }
            m_SerializedObject.ApplyModifiedProperties();
            Debug.Log($"Serialized, Vertices: {m_Vertices.arraySize}, Splines: {m_Splines.arraySize}");
        }
    }
}
