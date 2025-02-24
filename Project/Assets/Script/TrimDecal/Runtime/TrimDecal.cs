using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace Project
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class TrimDecal : MonoBehaviour
    {
        [SerializeField]
        private List<TrimShape> m_Shapes = new();
        [SerializeField]
        private float2[] m_Profile = new float2[]
        {
            new(1f, 0f),
            new(0f, 0f),
            new(0f, 1f),
        };
        [SerializeField]
        private bool m_DebugView = false;
        private Mesh m_Mesh;

        /////////////////////////////////////////////////////////////////

        private void OnValidate()
        {
            if (m_Mesh == null)
            {
                m_Mesh = new Mesh();
            }

            m_Shapes[0].Update();
            TrimMesh.Build(m_Shapes[0], m_Profile, ref m_Mesh);
            GetComponent<MeshFilter>().mesh = m_Mesh;
        }

        private void OnDrawGizmos()
        {
            if (m_DebugView)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                m_Shapes.ForEach(x => TrimDebug.DrawShape(x));

                Handles.matrix = transform.localToWorldMatrix;
                TrimDebug.DrawMesh(m_Mesh);
            }
        }
    }
}