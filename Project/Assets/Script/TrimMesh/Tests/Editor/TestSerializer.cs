using NUnit.Framework;
using TrimMesh.Editor;
using UnityEngine;

namespace TrimMesh.Test
{
    /*
    public class TestSerializer
    {
        private TrimMesh m_TrimMesh;
        private Serializer m_Serializer;

        [SetUp]
        public void Setup()
        {
            GameObject gameObject = new();
            m_TrimMesh = gameObject.AddComponent<TrimMesh>();
            m_Serializer = new Serializer(m_TrimMesh);
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(m_TrimMesh.gameObject);
        }

        /////////////////////////////////////////////////////////////

        [Test]
        public void CreateNewSpline()
        {
            m_Serializer.CreateSpline(Vector3.zero, Vector3.right);
            m_Serializer.ApplyModifiedProperties();

            Assert.AreEqual(m_TrimMesh.vertices[0].position, Vector3.zero);
            Assert.AreEqual(m_TrimMesh.vertices[1].position, Vector3.right);
            Assert.AreEqual(m_TrimMesh.splines[0].segments[0].vertexA, m_TrimMesh.vertices[0]);
            Assert.AreEqual(m_TrimMesh.splines[0].segments[0].vertexB, m_TrimMesh.vertices[1]);
        }

        [Test]
        public void CreateSplineAppendingShape()
        {
            m_Serializer.CreateSpline(new Vector3(-1, 0, 0), new Vector3(0, 0, 0));
            m_Serializer.CreateSpline(new Vector3(0, 0, 0), new Vector3(0, 0, 1));
            m_Serializer.ApplyModifiedProperties();

            // Should not have created a new spline, and only one additional vertex
            Assert.AreEqual(m_TrimMesh.splines.Count, 1);
            Assert.AreEqual(3, m_TrimMesh.vertices.Length);

            // Check if middle vertex is shared
            Assert.AreEqual(m_TrimMesh.splines[0].segments[0].vertexB, m_TrimMesh.vertices[1]);
            Assert.AreEqual(m_TrimMesh.splines[0].segments[1].vertexA, m_TrimMesh.vertices[1]);
        }

        [Test]
        public void CreateSplineClosingShape()
        {
            // Create U shaped spline
            m_Serializer.CreateSpline(new Vector3(-1, 0, 0), new Vector3(0, 0, 0));
            m_Serializer.AppendSegment(0, 1, new Vector3(0, 0, 1));
            m_Serializer.AppendSegment(0, 2, new Vector3(-1, 0, 1));

            // Connect end points with new spline, should append segment without creating new Vertices
            m_Serializer.CreateSpline(new Vector3(-1, 0, 0), new Vector3(-1, 0, 1));
            m_Serializer.ApplyModifiedProperties();

            // Should not have created a new spline or vertices
            Assert.AreEqual(m_TrimMesh.splines.Length, 1);
            Assert.AreEqual(4, m_TrimMesh.vertices.Length);

            // Check if vertices are shared
            Assert.AreEqual(m_TrimMesh.splines[0].segments[3].vertexA, m_TrimMesh.splines[0].segments[2].vertexB);
            Assert.AreEqual(m_TrimMesh.splines[0].segments[3].vertexB, m_TrimMesh.splines[0].segments[0].vertexA);
        }
    }
    */
}