using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TrimMesh.Editor
{
    public class Serializer
    {
        private bool m_IsDirty;
        private TrimMesh m_TrimMesh;
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
            m_TrimMesh = trimMesh;
            m_SerializedObject = new SerializedObject(trimMesh);
            m_Splines = m_SerializedObject.FindProperty(nameof(m_Splines));
            m_Vertices = m_SerializedObject.FindProperty(nameof(m_Vertices));
        }

        /////////////////////////////////////////////////////////////

        public void CreateSpline(Vector3 positionA, Vector3 positionB)
        {
            // TODO : Check if vertices already exist

            SplineVertex vertexA = CreateVertex(positionA);
            SplineVertex vertexB = CreateVertex(positionB);

            CreateSpline(vertexA, vertexB);
            m_IsDirty = true;
        }

        public void DeleteSpline(int splineIndex)
        {
            m_Splines.DeleteArrayElementAtIndex(splineIndex);
            DeleteIsolatedVertices();

            m_IsDirty = true;
        }

        public void AppendSegment(int splineIndex, SplineVertex referenceVertex, Vector3 position)
        {
            SerializedProperty spline, segment, segments;
            SplineVertex vertexA, vertexB;

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

        public void ConnectVertices(int vertexIndexA, int vertexIndexB)
        {
            int splineIndexA = -1;
            int splineIndexB = -1;
            SplineVertex vertexA = m_TrimMesh.vertices[vertexIndexA];
            SplineVertex vertexB = m_TrimMesh.vertices[vertexIndexB];
            SplineVertex splineVertexA, splineVertexB;
            SerializedProperty spline, segment, segments;

            bool vertexAHasConnection = false;
            bool vertexBHasConnection = false;

            // Find spline index of each vertex
            for (int i = 0; i < m_Splines.arraySize; i++)
            {
                spline = m_Splines.GetArrayElementAtIndex(i);
                segments = spline.FindPropertyRelative(k_NameOfSegments);

                for (int j = 0; j < segments.arraySize; j++)
                {
                    segment = segments.GetArrayElementAtIndex(j);
                    splineVertexA = segment.FindPropertyRelative(k_NameOfVertexA).managedReferenceValue as SplineVertex;
                    splineVertexB = segment.FindPropertyRelative(k_NameOfVertexB).managedReferenceValue as SplineVertex;

                    if (splineVertexA == vertexA || splineVertexB == vertexA)
                    {
                        splineIndexA = i;
                        vertexAHasConnection = j > 0 && j < segments.arraySize;
                    }

                    if (splineVertexA == vertexB || splineVertexB == vertexB)
                    {
                        splineIndexB = i;
                        vertexBHasConnection = j > 0 && j < segments.arraySize;
                    }
                }

                // Early out
                if (splineIndexA > -1 && splineIndexB > -1)
                {
                    break;
                }
            }

            // Case 0 : Create branch connections
            if (vertexAHasConnection && vertexBHasConnection)
            {
                int index = m_Splines.arraySize;
                m_Splines.InsertArrayElementAtIndex(index);
                spline = m_Splines.GetArrayElementAtIndex(index);
                segments = spline.FindPropertyRelative(k_NameOfSegments);
                segments.arraySize = 1;

                segment = segments.GetArrayElementAtIndex(0);
                segment.FindPropertyRelative(k_NameOfVertexA).managedReferenceValue = vertexA;
                segment.FindPropertyRelative(k_NameOfVertexB).managedReferenceValue = vertexB;
            }
            else
            {
                // Case 1 : Both vertices belong to the same spline
                spline = m_Splines.GetArrayElementAtIndex(splineIndexA);
                segments = spline.FindPropertyRelative(k_NameOfSegments);

                int index = segments.arraySize;
                segments.InsertArrayElementAtIndex(index);
                segment = segments.GetArrayElementAtIndex(index);

                segment.FindPropertyRelative(k_NameOfVertexA).managedReferenceValue = vertexA;
                segment.FindPropertyRelative(k_NameOfVertexB).managedReferenceValue = vertexB;

                if (splineIndexA != splineIndexB)
                {
                    SplineVertex copyVertexA, copyVertexB;
                    SerializedProperty splineB, segmentsB, segmentB;
                    splineB = m_Splines.GetArrayElementAtIndex(splineIndexB);
                    segmentsB = splineB.FindPropertyRelative(k_NameOfSegments);

                    for (int i = 0; i < segmentsB.arraySize; i++)
                    {
                        index = segments.arraySize;
                        segments.InsertArrayElementAtIndex(index);
                        segment = segments.GetArrayElementAtIndex(index);

                        segmentB = segmentsB.GetArrayElementAtIndex(i);
                        copyVertexA = segmentB.FindPropertyRelative(k_NameOfVertexA).managedReferenceValue as SplineVertex;
                        copyVertexB = segmentB.FindPropertyRelative(k_NameOfVertexB).managedReferenceValue as SplineVertex;

                        segment.FindPropertyRelative(k_NameOfVertexA).managedReferenceValue = copyVertexA;
                        segment.FindPropertyRelative(k_NameOfVertexB).managedReferenceValue = copyVertexB;
                    }

                    // Delete entire spline B
                    m_Splines.DeleteArrayElementAtIndex(splineIndexB);
                }
            }
            m_IsDirty = true;
        }

        /// <summary>
        /// Delete segments from the spline. Creates new splines for contiguous segments, deletes the original spline.
        /// </summary>
        /// <param name="splineIndex">The spline index.</param>
        /// <param name="segmentIndices">The segment indices to delete.</param>
        public void DeleteSegments(int splineIndex, HashSet<int> segmentIndices)
        {
            if (segmentIndices.Count == 0)
            {
                return;
            }

            SerializedProperty spline, segments;
            spline = m_Splines.GetArrayElementAtIndex(splineIndex);
            segments = spline.FindPropertyRelative(k_NameOfSegments);

            // Iterate over all segments, create new splines for remaining connected segments
            for (int i = segments.arraySize - 1; i >= 0; i--)
            {
                if (segmentIndices.Contains(i))
                {
                    segments.DeleteArrayElementAtIndex(i);
                }
            }

            DeleteIsolatedVertices();
            ResolveDisconnectedSegments();
            m_IsDirty = true;
        }

        /// <summary>
        /// Delete the vertices from all splines. Deletes any spline segment referencing one of the vertices.
        /// </summary>
        /// <param name="vertexIndices">The vertex indices to delete.</param>
        public void DeleteVertices(HashSet<int> vertexIndices)
        {
            SplineVertex vertexA, vertexB;
            SerializedProperty spline, segment, segments;

            // Collect vertex instances
            HashSet<SplineVertex> vertices = new();
            HashSet<int> cleanupSplines = new();
            foreach (int index in vertexIndices)
            {
                vertices.Add(m_TrimMesh.vertices[index]);
            }

            // Delete any segment referencing one of the vertices
            for (int i = m_Splines.arraySize - 1; i >= 0; i--)
            {
                spline = m_Splines.GetArrayElementAtIndex(i);
                segments = spline.FindPropertyRelative(k_NameOfSegments);

                for (int j = segments.arraySize - 1; j >= 0; j--)
                {
                    segment = segments.GetArrayElementAtIndex(j);
                    vertexA = segment.FindPropertyRelative(k_NameOfVertexA).managedReferenceValue as SplineVertex;
                    vertexB = segment.FindPropertyRelative(k_NameOfVertexB).managedReferenceValue as SplineVertex;

                    if (vertices.Contains(vertexA) || vertices.Contains(vertexB))
                    {
                        cleanupSplines.Add(i);
                        segments.DeleteArrayElementAtIndex(j);
                    }
                }
            }

            DeleteIsolatedVertices();
            ResolveDisconnectedSegments();
            m_IsDirty = true;
        }

        public void Update()
        {
            m_SerializedObject.Update();
        }

        public void ApplyModifiedProperties()
        {
            if (m_IsDirty)
            {
                m_SerializedObject.ApplyModifiedProperties();
                m_IsDirty = false;
            }
        }

        /////////////////////////////////////////////////////////////

        private void RemoveIsolatedVertices()
        {
            HashSet<SplineVertex> connectedVertices = new();

            for (int i = 0; i < m_Splines.arraySize; i++)
            {
                SerializedProperty spline = m_Splines.GetArrayElementAtIndex(i);
                SerializedProperty segments = spline.FindPropertyRelative(k_NameOfSegments);

                for (int j = 0; j < segments.arraySize; j++)
                {
                    SerializedProperty segment = segments.GetArrayElementAtIndex(j);
                    SplineVertex vertexA = segment.FindPropertyRelative(k_NameOfVertexA).managedReferenceValue as SplineVertex;
                    SplineVertex vertexB = segment.FindPropertyRelative(k_NameOfVertexB).managedReferenceValue as SplineVertex;

                    connectedVertices.Add(vertexA);
                    connectedVertices.Add(vertexB);
                }
            }

            for (int i = m_Vertices.arraySize - 1; i >= 0; i--)
            {
                SplineVertex vertex = m_Vertices.GetArrayElementAtIndex(i).managedReferenceValue as SplineVertex;

                if (!connectedVertices.Contains(vertex))
                {
                    m_Vertices.DeleteArrayElementAtIndex(i);
                }
            }
        }

        private void MergeNearbyVertices(Span<int> dirtyVertices, float threshold = 0.01f)
        {
            SplineVertex vertexA, vertexB;
            Dictionary<SplineVertex, SplineVertex> vertexRemap = new();

            // Detect vertices within threshold, map vertex B to vertex A
            for (int i = m_Vertices.arraySize - 1; i >= 0; i--)
            {
                vertexA = m_Vertices.GetArrayElementAtIndex(i).managedReferenceValue as SplineVertex;

                for (int j = 0; j < dirtyVertices.Length; j++)
                {
                    if (i == dirtyVertices[j])
                    {
                        continue;
                    }

                    vertexB = m_Vertices.GetArrayElementAtIndex(dirtyVertices[j]).managedReferenceValue as SplineVertex;

                    if (Vector3.Distance(vertexA.position, vertexB.position) < threshold)
                    {
                        vertexRemap.Add(vertexB, vertexA);
                        m_Vertices.DeleteArrayElementAtIndex(i); // This safe?!
                    }
                }
            }

            for (int i = 0; i < m_Splines.arraySize; i++)
            {
                SerializedProperty spline = m_Splines.GetArrayElementAtIndex(i);
                SerializedProperty segments = spline.FindPropertyRelative(k_NameOfSegments);

                for (int j = 0; j < segments.arraySize; j++)
                {
                    SerializedProperty segment = segments.GetArrayElementAtIndex(j);
                    SerializedProperty vertexAProperty = segment.FindPropertyRelative(k_NameOfVertexA);
                    SerializedProperty vertexBProperty = segment.FindPropertyRelative(k_NameOfVertexB);
                    vertexA = vertexAProperty.managedReferenceValue as SplineVertex;
                    vertexB = vertexBProperty.managedReferenceValue as SplineVertex;

                    // Remap merged vertex references
                    if (vertexRemap.ContainsKey(vertexA))
                    {
                        vertexAProperty.managedReferenceValue = vertexRemap[vertexA];
                    }
                    if (vertexRemap.ContainsKey(vertexB))
                    {
                        vertexBProperty.managedReferenceValue = vertexRemap[vertexB];
                    }
                }
            }
        }

        /////////////////////////////////////////////////////////////
        // LEGACY ///////////////////////////////////////////////////

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

        private void DeleteIsolatedVertices()
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

        public void ResolveDisconnectedSegments()
        {
            for (int i = 0; i < m_Splines.arraySize; i++)
            {
                SerializedProperty sourceSpline = m_Splines.GetArrayElementAtIndex(i);
                SerializedProperty sourceSegments = sourceSpline.FindPropertyRelative(k_NameOfSegments);

                if (sourceSegments.arraySize == 0)
                {
                    m_Splines.DeleteArrayElementAtIndex(i);
                    return;
                }

                SerializedProperty targetSpline = null;
                SerializedProperty targetSegments = null;
                int targetSegmentIndex = 0;

                // Create the first new spline
                int newSplineIndex = m_Splines.arraySize;
                m_Splines.InsertArrayElementAtIndex(newSplineIndex);
                targetSpline = m_Splines.GetArrayElementAtIndex(newSplineIndex);
                targetSegments = targetSpline.FindPropertyRelative(k_NameOfSegments);
                targetSegments.arraySize = 0;

                for (int j = 0; j < sourceSegments.arraySize; j++)
                {
                    SerializedProperty segment = sourceSegments.GetArrayElementAtIndex(j);
                    SplineVertex vertexA = segment.FindPropertyRelative(k_NameOfVertexA).managedReferenceValue as SplineVertex;
                    SplineVertex vertexB = segment.FindPropertyRelative(k_NameOfVertexB).managedReferenceValue as SplineVertex;

                    // If this is a disconnected segment, create a new spline
                    if (j > 0 && !AreSegmentsConnected(sourceSegments, j - 1, j))
                    {
                        newSplineIndex = m_Splines.arraySize;
                        m_Splines.InsertArrayElementAtIndex(newSplineIndex);
                        targetSpline = m_Splines.GetArrayElementAtIndex(newSplineIndex);
                        targetSegments = targetSpline.FindPropertyRelative(k_NameOfSegments);
                        targetSegments.arraySize = 0;
                        targetSegmentIndex = 0;
                    }

                    // Copy segment to new spline
                    targetSegments.InsertArrayElementAtIndex(targetSegmentIndex);
                    SerializedProperty targetSegment = targetSegments.GetArrayElementAtIndex(targetSegmentIndex);
                    targetSegment.FindPropertyRelative(k_NameOfVertexA).managedReferenceValue = vertexA;
                    targetSegment.FindPropertyRelative(k_NameOfVertexB).managedReferenceValue = vertexB;
                    targetSegmentIndex++;
                }

                // Delete the original spline
                m_Splines.DeleteArrayElementAtIndex(i);
            }
        }

        // Helper function to check if two segments are connected
        private bool AreSegmentsConnected(SerializedProperty segments, int indexA, int indexB)
        {
            SerializedProperty segmentA = segments.GetArrayElementAtIndex(indexA);
            SerializedProperty segmentB = segments.GetArrayElementAtIndex(indexB);

            SplineVertex a1 = segmentA.FindPropertyRelative(k_NameOfVertexA).managedReferenceValue as SplineVertex;
            SplineVertex a2 = segmentA.FindPropertyRelative(k_NameOfVertexB).managedReferenceValue as SplineVertex;
            SplineVertex b1 = segmentB.FindPropertyRelative(k_NameOfVertexA).managedReferenceValue as SplineVertex;
            SplineVertex b2 = segmentB.FindPropertyRelative(k_NameOfVertexB).managedReferenceValue as SplineVertex;

            return a1 == b1 || a1 == b2 || a2 == b1 || a2 == b2;
        }
    }
}
