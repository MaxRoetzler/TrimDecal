using System.Collections.Generic;
using System.Collections;
using Unity.Mathematics;
using System.Linq;

namespace TrimMesh
{
    public class SplineModel
    {
        private List<Spline> m_Splines;
        private List<SplineVertex> m_Vertices;
        private List<SplineSegment> m_Segments;

        public SplineModel()
        {
            m_Splines = new();
            m_Vertices = new();
            m_Segments = new();
        }

        /////////////////////////////////////////////////////////////

        public delegate void ModelChangedHandler(SplineModel model, SplineModification modification);
        public ModelChangedHandler onModelChanged;

        /////////////////////////////////////////////////////////////

        public List<Spline> splines
        {
            get => m_Splines;
        }

        public int splineCount
        {
            get => m_Splines.Count;
        }

        public List<SplineSegment> segments
        {
            get => m_Segments;
        }

        public int segmentCount
        {
            get => m_Segments.Count;
        }

        public List<SplineVertex> vertices
        {
            get => m_Vertices;
        }

        public int vertexCount
        {
            get => m_Vertices.Count;
        }

        /////////////////////////////////////////////////////////////

        public Spline CreateSpline(float3 positionA, float3 positionB)
        {
            SplineVertex vertexA = new(positionA);
            SplineVertex vertexB = new(positionB);
            m_Vertices.Add(vertexA);
            m_Vertices.Add(vertexB);

            return CreateSpline(vertexA, vertexB);
        }

        public Spline CreateSpline(float3 positionA, SplineVertex vertexB)
        {
            SplineVertex vertexA = new(positionA);
            m_Vertices.Add(vertexA);

            return CreateSpline(vertexA, vertexB);
        }

        public Spline CreateSpline(SplineVertex vertexA, float3 positionB)
        {
            SplineVertex vertexB = new(positionB);
            m_Vertices.Add(vertexB);

            return CreateSpline(vertexA, vertexB);
        }

        public Spline CreateSpline(SplineVertex vertexA, SplineVertex vertexB)
        {
            Spline spline = new();
            SplineSegment segment = new(vertexA, vertexB, spline);

            spline.segments.Add(segment);
            vertexA.segments.Add(segment);
            vertexB.segments.Add(segment);
            m_Segments.Add(segment);
            m_Splines.Add(spline);

            NotifyModelChanged(SplineModification.Structure);
            return spline;
        }

        public void MergeSplines(Spline splineA, SplineVertex vertexA, Spline splineB, SplineVertex vertexB)
        {
            SplineSegment newSegment = new(vertexA, vertexB, splineA);
            splineA.segments.Add(newSegment);
            vertexA.segments.Add(newSegment);
            vertexB.segments.Add(newSegment);
            m_Segments.Add(newSegment);

            // Append splineB to splineA
            foreach (SplineSegment segment in splineB.segments)
            {
                segment.spline = splineA;
                splineA.segments.Add(segment);
            }

            m_Splines.Remove(splineB);
            SortSplineSegments(splineA);
            NotifyModelChanged(SplineModification.Structure);
        }

        public void RemoveSpline(int index)
        {
            Spline spline = m_Splines[index];

            foreach (SplineSegment segment in spline.segments)
            {
                m_Vertices.Remove(segment.vertexA);
                m_Vertices.Remove(segment.vertexB);
                m_Segments.Remove(segment);
            }

            m_Splines.RemoveAt(index);
            NotifyModelChanged(SplineModification.Structure);
        }

        /////////////////////////////////////////////////////////////

        public SplineSegment AppendSegment(Spline spline, SplineVertex vertexA, float3 position)
        {
            SplineVertex vertexB = new(position);
            SplineSegment segment = new(vertexA, vertexB, spline);

            spline.segments.Add(segment);
            vertexA.segments.Add(segment);
            vertexB.segments.Add(segment);

            m_Vertices.Add(vertexB);
            m_Segments.Add(segment);

            NotifyModelChanged(SplineModification.Structure);
            return segment;
        }

        public SplineSegment AppendSegment(Spline spline, SplineVertex vertexA, SplineVertex vertexB)
        {
            SplineSegment segment = new(vertexA, vertexB, spline);

            spline.segments.Add(segment);
            vertexA.segments.Add(segment);
            vertexB.segments.Add(segment);

            m_Segments.Add(segment);

            NotifyModelChanged(SplineModification.Structure);
            return segment;
        }

        public void RemoveSegment(BitArray segmentMask)
        {
            HashSet<Spline> inspectSplines = new();
            HashSet<SplineVertex> inspectVertices = new();
            HashSet<SplineSegment> segmentsToRemove = new();

            for (int i = 0; i < m_Segments.Count; i++)
            {
                if (segmentMask[i])
                {
                    SplineSegment segment = m_Segments[i];

                    segmentsToRemove.Add(segment);
                    segment.vertexA.segments.Remove(segment);
                    segment.vertexB.segments.Remove(segment);

                    inspectSplines.Add(segment.spline);
                    inspectVertices.Add(segment.vertexA);
                    inspectVertices.Add(segment.vertexB);
                }
            }

            // Delete segments
            foreach (SplineSegment segment in segmentsToRemove)
            {
                segment.spline.segments.Remove(segment);
                m_Segments.Remove(segment);
            }

            // Delete isolated vertices
            foreach (SplineVertex vertex in inspectVertices)
            {
                if (vertex.segments.Count == 0)
                {
                    m_Vertices.Remove(vertex);
                }
            }

            // Sort and split splines
            foreach (Spline spline in inspectSplines)
            {
                SplitAndSortSpline(spline);
            }

            NotifyModelChanged(SplineModification.Structure);
        }

        /////////////////////////////////////////////////////////////

        public void RemoveVertex(BitArray vertexMask)
        {
            HashSet<Spline> splinesToValidate = new();
            HashSet<SplineVertex> verticesToRemove = new();
            HashSet<SplineSegment> segmentsToRemove = new();

            for (int i = 0; i < m_Vertices.Count; i++)
            {
                if (vertexMask[i])
                {
                    SplineVertex vertex = m_Vertices[i];
                    verticesToRemove.Add(vertex);

                    foreach (SplineSegment segment in vertex.segments)
                    {
                        SplineVertex connectedVertex = vertex != segment.vertexA ? segment.vertexA : segment.vertexB;
                        connectedVertex.segments.Remove(segment);
                        segmentsToRemove.Add(segment);
                        splinesToValidate.Add(segment.spline);

                        if (connectedVertex.segments.Count == 0)
                        {
                            verticesToRemove.Add(connectedVertex);
                        }
                    }
                }
            }

            foreach (SplineSegment segment in segmentsToRemove)
            {
                segment.spline.segments.Remove(segment);
                m_Segments.Remove(segment);
            }

            foreach (SplineVertex vertex in verticesToRemove)
            {
                m_Vertices.Remove(vertex);
            }

            foreach (Spline spline in splinesToValidate)
            {
                SplitAndSortSpline(spline);
            }

            NotifyModelChanged(SplineModification.Structure);
        }

        public void DissolveVertex(BitArray vertexMask)
        {

        }


        public void TranslateVertex(BitArray vertexMask, float3 offset)
        {
            // Apply offset to all selected vertices
            for (int i = 0; i < m_Vertices.Count; i++)
            {
                if (vertexMask[i])
                {
                    m_Vertices[i].position += offset;
                }
            }

            NotifyModelChanged(SplineModification.Offset);
        }

        public Spline GetSplineFromVertex(SplineVertex vertex)
        {
            foreach (SplineSegment segment in vertex.segments)
            {
                return m_Splines[m_Splines.IndexOf(segment.spline)];
            }
            return default;
        }

        /////////////////////////////////////////////////////////////

        public void NotifyModelChanged(SplineModification modification)
        {
            onModelChanged?.Invoke(this, modification);
        }

        public void Clear()
        {
            m_Vertices.Clear();
            m_Segments.Clear();
            m_Splines.Clear();
        }

        /////////////////////////////////////////////////////////////

        private void SortSplineSegments(Spline spline)
        {
            if (spline.segmentCount == 0)
            {
                m_Splines.Remove(spline);
                return;
            }

            List<SplineSegment> orderedSegments = new();
            HashSet<SplineSegment> visited = new();

            SplineSegment startSegment = FindSplineStart(spline.segments[0]);
            Queue<SplineSegment> queue = new();
            queue.Enqueue(startSegment);

            while (queue.Count > 0)
            {
                SplineSegment current = queue.Dequeue();
                if (visited.Add(current))
                {
                    orderedSegments.Add(current);

                    SplineVertex nextVertex = GetNextVertex(current, orderedSegments);
                    if (nextVertex != null)
                    {
                        foreach (SplineSegment neighbor in nextVertex.segments)
                        {
                            if (!visited.Contains(neighbor))
                            {
                                queue.Enqueue(neighbor);
                            }
                        }
                    }
                }
            }
            spline.segments.Clear();
            spline.segments.AddRange(orderedSegments);
        }

        private void SplitAndSortSpline(Spline spline)
        {
            if (spline.segmentCount == 0)
            {
                m_Splines.Remove(spline);
                return;
            }

            List<Spline> newSplines = new();
            HashSet<SplineSegment> visited = new();

            foreach (SplineSegment segment in spline.segments)
            {
                if (!visited.Contains(segment))
                {
                    Spline newSpline = new();
                    List<SplineSegment> orderedSegments = new();

                    SplineSegment startSegment = FindSplineStart(segment);
                    Queue<SplineSegment> queue = new();
                    queue.Enqueue(startSegment);

                    while (queue.Count > 0)
                    {
                        SplineSegment current = queue.Dequeue();
                        if (visited.Add(current))
                        {
                            orderedSegments.Add(current);
                            current.spline = newSpline;

                            SplineVertex nextVertex = GetNextVertex(current, orderedSegments);
                            if (nextVertex != null)
                            {
                                foreach (SplineSegment neighbor in nextVertex.segments)
                                {
                                    if (!visited.Contains(neighbor))
                                    {
                                        queue.Enqueue(neighbor);
                                    }
                                }
                            }
                        }
                    }
                    newSpline.segments.AddRange(orderedSegments);
                    newSplines.Add(newSpline);
                }
            }
            m_Splines.Remove(spline);
            m_Splines.AddRange(newSplines);
        }

        private SplineSegment FindSplineStart(SplineSegment segment)
        {
            if (segment.vertexA.segments.Count == 1)
            {
                return segment;
            }
            if (segment.vertexB.segments.Count == 1)
            {
                return segment;
            }
            return segment;
        }

        private SplineVertex GetNextVertex(SplineSegment current, List<SplineSegment> orderedSegments)
        {
            SplineVertex vertexA = current.vertexA;
            SplineVertex vertexB = current.vertexB;

            foreach (SplineSegment segment in vertexA.segments)
            {
                if (!orderedSegments.Contains(segment))
                    return vertexA;
            }

            foreach (SplineSegment segment in vertexB.segments)
            {
                if (!orderedSegments.Contains(segment))
                    return vertexB;
            }

            return null;
        }

    }
}