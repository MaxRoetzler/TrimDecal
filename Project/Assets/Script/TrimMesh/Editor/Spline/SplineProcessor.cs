using System.Collections.Generic;
using UnityEngine;

namespace TrimMesh.Editor
{
    public class SplineProcessor
    {
        /*
        private const float k_VertexMergeDistance = 0.01f;

        private TrimMesh m_TrimMesh;
        private SplineSerializer m_Serializer;

        private List<Spline> m_Splines;
        private Dictionary<SplineSegment, Spline> m_SegmentToSpline;
        private Dictionary<VertexView, HashSet<SplineSegment>> m_VertexToSegment;

        /////////////////////////////////////////////////////////////

        public SplineProcessor(TrimMesh trimMesh)
        {
            m_Serializer = new SplineSerializer(trimMesh);
            m_TrimMesh = trimMesh;

            m_SegmentToSpline = new();
            m_VertexToSegment = new();
            m_Splines = new();
        }

        /////////////////////////////////////////////////////////////

        #region Spline Methods

        public void CreateSplineOrSegment(Vector3 positionA, Vector3 positionB)
        {
        Update();

        // Fetch existing vertices, or create new ones
        FindOrCreateVertex(positionA, out VertexView vertexA);
        FindOrCreateVertex(positionB, out VertexView vertexB);

        int vertexASegmentCount = GetSegmentCount(vertexA);
        int vertexBSegmentCount = GetSegmentCount(vertexB);

            // Case 1
            if (vertexASegmentCount == 0 && vertexBSegmentCount == 0)
            {
                Debug.Log("Create Spline, A -> B");
                CreateSpline(vertexA, vertexB);
    }
            // Case 2
            else if (vertexASegmentCount == 1 && vertexBSegmentCount == 0)
            {
                Debug.Log("Append Spline, A -> B");
                FindSplineUsingVertex(vertexA, out Spline spline);
    AppendSpline(vertexA, vertexB, spline);
}
            // Case 3
            else if (vertexASegmentCount == 0 && vertexBSegmentCount == 1)
{
    Debug.Log("Append Spline, B -> A");
    FindSplineUsingVertex(vertexB, out Spline spline);
    AppendSpline(vertexB, vertexA, spline);
}
// Case 4
else if (vertexASegmentCount == 1 && vertexBSegmentCount == 1)
{
    FindSplineUsingVertex(vertexA, out Spline splineA);
    FindSplineUsingVertex(vertexB, out Spline splineB);

    // Same spline vertices, close spline
    if (splineA == splineB)
    {
        Debug.Log("Close Spline");
        CloseSpline(vertexA, vertexB, splineA);
    }
    // Different spline vertices, connect splines
    else
    {
        Debug.Log("Connect Splines");
        MergeSplines(vertexA, vertexB, splineA, splineB);
    }
}
// Case 5
else if (vertexASegmentCount >= 2 && vertexBSegmentCount == 0)
{
    Debug.Log("Create Spline, A -> B");
    CreateSpline(vertexA, vertexB);
}
// Case 6
else if (vertexASegmentCount == 0 && vertexBSegmentCount >= 2)
{
    Debug.Log("Append Spline, B -> A");
    CreateSpline(vertexB, vertexA);
}
// Case 7
else if (vertexASegmentCount == 1 && vertexBSegmentCount >= 2)
{
    Debug.Log("Append Spline, A -> B");
    FindSplineUsingVertex(vertexA, out Spline splineA);
    AppendSpline(vertexA, vertexB, splineA);
}
// Case 8
else if (vertexASegmentCount >= 2 && vertexBSegmentCount == 1)
{
    Debug.Log("Append Spline, B -> A");
    FindSplineUsingVertex(vertexB, out Spline splineB);
    AppendSpline(vertexB, vertexA, splineB);
}
// Case 9
else if (vertexASegmentCount >= 2 && vertexBSegmentCount >= 2)
{
    Debug.Log("Append Branch If No Duplicate");
}

SerializeIntermediateModel();
        }

        public void DeleteSpline(int index)
{
    Update();
    Spline spline = m_Splines[index];

    foreach (SplineSegment segment in spline.segments)
    {
        m_SegmentToSpline.Remove(segment);
        RemoveSegmentFromVertex(segment.vertexA, segment);
        RemoveSegmentFromVertex(segment.vertexB, segment);
    }

    m_Splines.RemoveAt(index);
    RemoveIsolatedVertices();
    SerializeIntermediateModel();
}
#endregion

/////////////////////////////////////////////////////////////

private void Update()
{
    m_Splines.Clear();
    m_SegmentToSpline.Clear();
    m_VertexToSegment.Clear();

    foreach (Spline spline in m_TrimMesh.splines)
    {
        foreach (SplineSegment segment in spline.segments)
        {
            AddSegmentToSpline(segment, spline);
            AddVertexToSegment(segment.vertexA, segment);
            AddVertexToSegment(segment.vertexB, segment);
        }
        m_Splines.Add(spline);
    }
}

/////////////////////////////////////////////////////////////

private void CreateSpline(VertexView vertexA, VertexView vertexB)
{
    Spline spline = new();
    m_Splines.Add(spline);

    SplineSegment segment = new(vertexA, vertexB);
    AddSegmentToSpline(segment, spline);
    AddVertexToSegment(vertexA, segment);
    AddVertexToSegment(vertexB, segment);
}

private void AppendSpline(VertexView vertexA, VertexView vertexB, Spline spline)
{
    SplineSegment segment = new(vertexA, vertexB);
    AddSegmentToSpline(segment, spline);
    AddVertexToSegment(vertexA, segment);
    AddVertexToSegment(vertexB, segment);
}

private void CloseSpline(VertexView vertexA, VertexView vertexB, Spline spline)
{
    SplineSegment segment = new(vertexA, vertexB);
    AddSegmentToSpline(segment, spline);
    AddVertexToSegment(vertexA, segment);
    AddVertexToSegment(vertexB, segment);
}

private void MergeSplines(VertexView vertexA, VertexView vertexB, Spline splineA, Spline splineB)
{
    SplineSegment segment = new(vertexA, vertexB);
    AddSegmentToSpline(segment, splineA);
    AddVertexToSegment(vertexA, segment);
    AddVertexToSegment(vertexB, segment);

    splineA.segments.AddRange(splineB.segments);
    splineA.segments.ForEach(x => m_SegmentToSpline[x] = splineA);

    m_Splines.Remove(splineB);
}

private bool FindSplineUsingVertex(VertexView vertex, out Spline spline)
{
    if (m_VertexToSegment.TryGetValue(vertex, out HashSet<SplineSegment> segments))
    {
        foreach (SplineSegment segment in segments)
        {
            if (m_SegmentToSpline.TryGetValue(segment, out Spline segmentSpline))
            {
                spline = segmentSpline;
                return true;
            }
        }
    }
    spline = default;
    return false;
}

/////////////////////////////////////////////////////////////

private bool FindOrCreateVertex(Vector3 position, out VertexView vertex)
{
    foreach (VertexView key in m_VertexToSegment.Keys)
    {
        if (Vector3.Distance(key.position, position) < k_VertexMergeDistance)
        {
            vertex = key;
            return true;
        }
    }
    vertex = new VertexView(position);
    return false;
}

private int GetSegmentCount(VertexView vertex)
{
    return m_VertexToSegment.TryGetValue(vertex, out var segments) ? segments.Count : 0;
}

private void RemoveIsolatedVertices()
{
    List<VertexView> isolatedVertices = new();

    foreach (KeyValuePair<VertexView, HashSet<SplineSegment>> item in m_VertexToSegment)
    {
        if (item.Value.Count == 0)
        {
            isolatedVertices.Add(item.Key);
        }
    }

    for (int i = 0; i < isolatedVertices.Count; i++)
    {
        VertexView vertex = isolatedVertices[i];
        m_VertexToSegment.Remove(vertex);
    }
}

/////////////////////////////////////////////////////////////

private void SerializeIntermediateModel()
{
    m_Serializer.Clear();

    foreach (VertexView vertex in m_VertexToSegment.Keys)
    {
        m_Serializer.AddVertex(vertex);
    }

    for (int i = 0; i < m_Splines.Count; i++)
    {
        Spline spline = m_Splines[i];
        m_Serializer.AddSpline(spline);

        foreach (SplineSegment segment in spline.segments)
        {
            m_Serializer.AddSegment(segment, i);
        }
    }

    m_Serializer.ApplyModifiedProperties();
}

/////////////////////////////////////////////////////////////

private void AddVertexToSegment(VertexView vertex, SplineSegment segment)
{
    if (m_VertexToSegment.ContainsKey(vertex))
    {
        m_VertexToSegment[vertex].Add(segment);
    }
    else
    {
        m_VertexToSegment[vertex] = new HashSet<SplineSegment> { segment };
    }
}

private void RemoveSegmentFromVertex(VertexView vertex, SplineSegment segment)
{
    if (m_VertexToSegment.TryGetValue(vertex, out HashSet<SplineSegment> segments))
    {
        segments.Remove(segment);

        if (segments.Count == 0)
        {
            m_VertexToSegment.Remove(vertex);
        }
    }
}

private void AddSegmentToSpline(SplineSegment segment, Spline spline)
{
    if (!m_SegmentToSpline.ContainsKey(segment))
    {
        m_SegmentToSpline[segment] = spline;
    }

    if (!spline.segments.Contains(segment))
    {
        spline.segments.Add(segment);
    }
}

private void RemoveSegmentFromSpline(SplineSegment segment, Spline spline)
{
    m_SegmentToSpline.Remove(segment);
    spline.segments.Remove(segment);
}
*/
    }
}