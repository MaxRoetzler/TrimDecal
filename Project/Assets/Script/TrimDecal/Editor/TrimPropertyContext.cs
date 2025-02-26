using UnityEngine;
using UnityEditor;

namespace TrimDecal.Editor
{
    public class TrimPropertyContext
    {
        private SerializedProperty m_Shapes;
        private SerializedProperty m_Shape;
        private SerializedProperty m_Vertices;
        private SerializedProperty m_Vertex;
        private SerializedProperty m_Position;
        private SerializedProperty m_Normal;
        private SerializedProperty m_GridSize;
        private SerializedProperty m_IsClosed;
        private SerializedProperty m_IsFlipped;
        private SerializedObject m_SerializedObject;

        private bool m_IsDirty;

        /////////////////////////////////////////////////////////////

        public TrimPropertyContext(SerializedObject serializedObject)
        {
            m_SerializedObject = serializedObject;
            m_Shapes = serializedObject.FindProperty(nameof(m_Shapes));
        }

        /////////////////////////////////////////////////////////////

        public bool hasShapes
        {
            get => m_Shapes != null && m_Shapes.arraySize > 0;
        }

        public int shapeCount
        {
            get => m_Shapes.arraySize;
        }

        public bool isClosed
        {
            get => m_IsClosed.boolValue;
            set
            {
                m_IsClosed.boolValue = value;
                m_IsDirty = true;
            }
        }

        public bool isFlipped
        {
            get => m_IsFlipped.boolValue;
            set
            {
                m_IsFlipped.boolValue = value;
                m_IsDirty = true;
            }
        }

        public Vector3 normal
        {
            get => m_Normal.vector3Value;
            set
            {
                m_Normal.vector3Value = value;
                m_IsDirty = true;
            }
        }

        public float gridSize
        {
            get => m_GridSize.floatValue;
            set
            {
                m_GridSize.floatValue = value;
                m_IsDirty = true;
            }
        }

        public bool hasVertices
        {
            get => m_Vertices != null && m_Vertices.arraySize > 0;
        }

        public int vertexCount
        {
            get => m_Vertices.arraySize;
        }

        public Vector3 position
        {
            get => m_Position.vector3Value;
            set
            {
                m_Position.vector3Value = value;
                m_IsDirty = true;
            }
        }

        /////////////////////////////////////////////////////////////

        public void SelectShape(int index)
        {
            m_Shape = m_Shapes.GetArrayElementAtIndex(index);
            m_Normal = m_Shape.FindPropertyRelative(nameof(m_Normal));
            m_Vertices = m_Shape.FindPropertyRelative(nameof(m_Vertices));
            m_GridSize = m_Shape.FindPropertyRelative(nameof(m_GridSize));
            m_IsClosed = m_Shape.FindPropertyRelative(nameof(m_IsClosed));
            m_IsFlipped = m_Shape.FindPropertyRelative(nameof(m_IsFlipped));
        }

        /////////////////////////////////////////////////////////////

        public void SelectVertex(int index)
        {
            m_Vertex = m_Vertices.GetArrayElementAtIndex(index);
            m_Position = m_Vertex.FindPropertyRelative(nameof(m_Position));
        }

        public Vector3 GetPosition(int index)
        {
            return m_Vertices.GetArrayElementAtIndex(index).FindPropertyRelative(nameof(m_Position)).vector3Value;
        }

        public void RemoveVertex(int index)
        {
            m_Vertices.DeleteArrayElementAtIndex(index);
            m_IsDirty = true;
        }

        public void AddPosition(Vector3 position, int index)
        {
            m_Vertices.InsertArrayElementAtIndex(index + 1);
            m_IsDirty = true;

            SelectVertex(index + 1);
            this.position = position;
        }

        public void AddPosition(Vector3 position)
        {
            int count = m_Vertices.arraySize;
            m_Vertices.InsertArrayElementAtIndex(count);
            m_IsDirty = true;

            SelectVertex(count);
            this.position = position;
        }

        /////////////////////////////////////////////////////////////

        public void ApplyModifiedProperties()
        {
            if (m_IsDirty)
            {
                m_SerializedObject.ApplyModifiedProperties();
                m_IsDirty = false;
            }
        }
    }
}