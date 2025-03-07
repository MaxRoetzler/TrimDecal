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
        private const string k_NameOfVertexA = "m_VertexA";
        private const string k_NameOfVertexB = "m_VertexB";

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

            CreateSpline(vertexA, vertexB);
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
        /// <param name="referenceVertex">The existing vertex to start new spline segment from.</param>
        /// <param name="position">The position of the new spline segment.</param>
        public void AddSegment(int splineIndex, SplineVertex referenceVertex, Vector3 position)
        {
            SerializedProperty spline;
            SerializedProperty segment;
            SerializedProperty segments;

            SplineVertex vertexA;
            SplineVertex vertexB;
            SplineVertex newVertex = CreateVertex(position);

            spline = m_Splines.GetArrayElementAtIndex(splineIndex);
            segments = spline.FindPropertyRelative(k_NameOfSegments);

            for (int i = 0; i < segments.arraySize; i++)
            {
                segment = segments.GetArrayElementAtIndex(i);
                vertexA = segment.FindPropertyRelative(k_NameOfVertexA).managedReferenceValue as SplineVertex;
                vertexB = segment.FindPropertyRelative(k_NameOfVertexB).managedReferenceValue as SplineVertex;

                if (vertexA == referenceVertex)
                {
                    InsertSegment(segments, i, newVertex, vertexA);
                    break;
                }

                if (vertexB == referenceVertex)
                {
                    InsertSegment(segments, i + 1, vertexB, newVertex);
                    break;
                }
            }

            m_IsDirty = true;
        }

        public void DeleteSegment(int splineIndex, int segmentIndex)
        {
            SerializedProperty spline;
            SerializedProperty segment;
            SerializedProperty segments;

            spline = m_Splines.GetArrayElementAtIndex(splineIndex);
            segments = spline.FindPropertyRelative(k_NameOfSegments);
            segment = segments.GetArrayElementAtIndex(segmentIndex);

            bool splitSpline = segmentIndex > 0 && segmentIndex < segments.arraySize - 1;

            if (splitSpline)
            {
                // Create new spline
            }

            segments.DeleteArrayElementAtIndex(segmentIndex);
            RemoveIsolatedVertices();
            m_IsDirty = true;
        }

        public void Update()
        {
            m_SerializedObject.Update();
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

        private void InsertSegment(SerializedProperty segments, int i, SplineVertex vertexA, SplineVertex vertexB)
        {
            SerializedProperty segment;

            segments.InsertArrayElementAtIndex(i);
            segment = segments.GetArrayElementAtIndex(i);

            Debug.Log($"segments: {segments.arraySize}, segment: {segment}");

            segment.FindPropertyRelative(k_NameOfVertexA).managedReferenceValue = vertexA;
            segment.FindPropertyRelative(k_NameOfVertexB).managedReferenceValue = vertexB;
        }

        private void CreateSpline(SplineVertex vertexA, SplineVertex vertexB)
        {
            SerializedProperty spline;
            SerializedProperty segments;

            int index = m_Splines.arraySize;
            m_Splines.InsertArrayElementAtIndex(index);
            spline = m_Splines.GetArrayElementAtIndex(index);

            segments = spline.FindPropertyRelative(k_NameOfSegments);
            segments.arraySize = 0;
            InsertSegment(segments, 0, vertexA, vertexB);
        }

        // TODO : In progress
        private void CreateSpline(SerializedProperty newSegments)
        {
            SerializedProperty spline;
            SerializedProperty segments;

            int index = m_Splines.arraySize;
            m_Splines.InsertArrayElementAtIndex(index);
            spline = m_Splines.GetArrayElementAtIndex(index);

            segments = spline.FindPropertyRelative(k_NameOfSegments);
            segments.arraySize = newSegments.arraySize;
        }

        private void RemoveIsolatedVertices()
        {
            SerializedProperty spline;
            SerializedProperty segment;
            SerializedProperty segments;

            SplineVertex vertexA;
            SplineVertex vertexB;
            HashSet<SplineVertex> vertices = new();

            // Collect all referenced vertices
            for (int i = 0; i < m_Splines.arraySize; i++)
            {
                spline = m_Splines.GetArrayElementAtIndex(i);
                segments = spline.FindPropertyRelative(k_NameOfSegments);

                for (int j = 0; j < segments.arraySize; j++)
                {
                    segment = segments.GetArrayElementAtIndex(j);

                    vertexA = segment.FindPropertyRelative(k_NameOfVertexA).managedReferenceValue as SplineVertex;
                    vertexB = segment.FindPropertyRelative(k_NameOfVertexB).managedReferenceValue as SplineVertex;

                    vertices.Add(vertexA);
                    vertices.Add(vertexB);
                }
            }

            // Delete all unreferenced vertices
            for (int i = m_Vertices.arraySize - 1; i >= 0; i--)
            {
                segment = m_Vertices.GetArrayElementAtIndex(i);
                if (segment.managedReferenceValue is SplineVertex vertex && !vertices.Contains(vertex))
                {
                    m_Vertices.DeleteArrayElementAtIndex(i);
                }
            }
        }
    }
}
