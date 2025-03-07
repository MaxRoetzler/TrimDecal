using UnityEngine;

namespace TrimDecal
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class TrimDecal : MonoBehaviour
    {
        [SerializeField]
        private TrimProfile m_Profile = null;
        [SerializeField]
        private TrimShape[] m_Shapes = new TrimShape[0];

        private Mesh m_Mesh;

        /////////////////////////////////////////////////////////////////
        
        public int count
        {
            get => m_Shapes.Length;
        }

        public TrimShape this[int i]
        {
            get => m_Shapes[i];
        }

        /////////////////////////////////////////////////////////////////

        private void OnValidate()
        {
            if (m_Profile == null)
            {
                return;
            }

            if (m_Mesh == null)
            {
                m_Mesh = new();
                GetComponent<MeshFilter>().mesh = m_Mesh;
            }

            using (TrimMeshBuilder builder = new(m_Mesh))
            {
                foreach (TrimShape shape in m_Shapes)
                {
                    shape.Update();
                    builder.Add(shape, m_Profile);
                }

                builder.Validate();
            }
        }
    }
}