using UnityEngine;

namespace TrimDecal
{
    [CreateAssetMenu(menuName = "Trim Profile", order = 320)]
    public class TrimProfile : ScriptableObject
    {
        [SerializeField]
        private TrimProfileVertex[] m_Vertices;

        /////////////////////////////////////////////////////////////////

        public int count
        {
            get => m_Vertices.Length;
        }

        public TrimProfileVertex this[int i]
        {
            get => m_Vertices[i];
        }
    }
}