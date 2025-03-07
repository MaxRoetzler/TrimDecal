using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TrimMesh.Editor
{
    public class Serializer
    {
        private bool m_IsDirty;
        private SerializedProperty m_Splines;
        private SerializedProperty m_Vertices;
        private SerializedObject m_SerializedObject;

        private const string k_NameOfPosition = "m_Position";
        private const string k_NameOfSegments = "m_Segments";

        /////////////////////////////////////////////////////////////

        public Serializer(TrimMesh trimMesh)
        {
            m_SerializedObject = new SerializedObject(trimMesh);
            m_Splines = m_SerializedObject.FindProperty(nameof(m_Splines));
            m_Vertices = m_SerializedObject.FindProperty(nameof(m_Vertices));
        }

        /////////////////////////////////////////////////////////////

        /// <summary>
        /// Creates a new spline, spline segment and two vertices at the specified positions.
        /// </summary>
        /// <param name="positionA">The first vertex position.</param>
        /// <param name="positionB">The second vertex position.</param>
        public void CreateSpline(Vector3 positionA, Vector3 positionB)
        {
            // TODO : Check if vertices already exist

            SplineVertex vertexA = CreateVertex(positionA);
            SplineVertex vertexB = CreateVertex(positionB);
            SplineSegment segment = new(vertexA, vertexB);

            CreateSpline(segment);
            m_IsDirty = true;
        }

        /// <summary>
        /// Deletes the spline and its segments. Removes all un-referenced vertices.
        /// </summary>
        /// <param name="splineIndex">The spline index to delete.</param>
        public void DeleteSpline(int splineIndex)
        {
            m_Splines.DeleteArrayElementAtIndex(splineIndex);
            RemoveIsolatedVertices();

            m_IsDirty = true;
        }

        /// <summary>
        /// Adds a new spline segment to the taraget spline, using the specified target vertex and position.
        /// </summary>
        /// <param name="splineIndex">The index of the spline to add the segment to.</param>
        /// <param name="startVertex">The existing vertex to start new spline segment from.</param>
        /// <param name="position">The position of the new spline segment.</param>
        public void AddSegment(int splineIndex, SplineVertex startVertex, Vector3 position)
        {
            SerializedProperty spline;
            SerializedProperty element;
            SerializedProperty segments;

            SplineSegment segment;
            SplineVertex vertex = CreateVertex(position);

            spline = m_Splines.GetArrayElementAtIndex(splineIndex);
            segments = spline.FindPropertyRelative(k_NameOfSegments);

            for (int i = 0; i < segments.arraySize; i++)
            {
                segment = segments.GetArrayElementAtIndex(i).managedReferenceValue as SplineSegment;

                if (segment.vertexA == startVertex)
                {
                    segments.InsertArrayElementAtIndex(i);
                    element = segments.GetArrayElementAtIndex(i);
                    element.managedReferenceValue = new SplineSegment(vertex, startVertex);
                    break;
                }

                if (segment.vertexB == startVertex)
                {
                    segments.InsertArrayElementAtIndex(i + 1);
                    element = segments.GetArrayElementAtIndex(i + 1);
                    element.managedReferenceValue = new SplineSegment(startVertex, vertex);
                    break;
                }
            }

            m_IsDirty = true;
        }

        /// <summary>
        /// Apply the serialized property changes when needed.
        /// </summary>
        public void ApplyModifiedProperties()
        {
            if (m_IsDirty)
            {
                m_SerializedObject.ApplyModifiedProperties();
                m_IsDirty = false;
            }
        }

        /////////////////////////////////////////////////////////////

        private SplineVertex CreateVertex(Vector3 position)
        {
            SerializedProperty element;
            SplineVertex vertex = new(position);

            int index = m_Vertices.arraySize;
            m_Vertices.InsertArrayElementAtIndex(index);
            element = m_Vertices.GetArrayElementAtIndex(index);
            element.managedReferenceValue = vertex;

            return vertex;
        }

        private int CreateSpline(SplineSegment segment)
        {
            SerializedProperty spline;
            SerializedProperty segments;

            int index = m_Splines.arraySize;
            m_Splines.InsertArrayElementAtIndex(index);
            spline = m_Splines.GetArrayElementAtIndex(index);

            segments = spline.FindPropertyRelative(k_NameOfSegments);
            segments.arraySize = 1;
            segments.GetArrayElementAtIndex(0).managedReferenceValue = segment;

            return index;
        }

        private void RemoveIsolatedVertices()
        {
            SerializedProperty spline;
            SerializedProperty element;
            SerializedProperty segments;
            HashSet<SplineVertex> vertices = new();

            // Collect all referenced vertices
            for (int i = 0; i < m_Splines.arraySize; i++)
            {
                spline = m_Splines.GetArrayElementAtIndex(i);
                segments = spline.FindPropertyRelative(k_NameOfSegments);

                for (int j = 0; j < segments.arraySize; j++)
                {
                    element = segments.GetArrayElementAtIndex(j);
                    if (element.managedReferenceValue is SplineSegment segment)
                    {
                        vertices.Add(segment.vertexA);
                        vertices.Add(segment.vertexB);
                    }
                }
            }

            // Delete all unreferenced vertices
            for (int i = m_Vertices.arraySize - 1; i >= 0; i--)
            {
                element = m_Vertices.GetArrayElementAtIndex(i);
                if (element.managedReferenceValue is SplineVertex vertex && !vertices.Contains(vertex))
                {
                    m_Vertices.DeleteArrayElementAtIndex(i);
                }
            }
        }
    }
}
