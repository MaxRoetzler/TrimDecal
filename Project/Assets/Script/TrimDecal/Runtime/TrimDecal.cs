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

            using (TrimMeshContext context = new TrimMeshContext(m_Mesh))
            {
                foreach (TrimShape shape in m_Shapes)
                {
                    shape.Update();
                    context.Add(shape, m_Profile);
                }
            }
        }
    }
}