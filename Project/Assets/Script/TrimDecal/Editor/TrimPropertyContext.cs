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

        public void SetShapeClosed(int shapeIndex, bool state)
        {
            m_Shape = m_Shapes.GetArrayElementAtIndex(shapeIndex);
            m_IsClosed = m_Shape.FindPropertyRelative(nameof(m_IsClosed));
            m_IsClosed.boolValue = state;

            m_IsDirty = true;
        }

        public void RemoveShape(int shapeIndex)
        {
            m_Shapes.DeleteArrayElementAtIndex(shapeIndex);

            m_IsDirty = true;
        }

        /////////////////////////////////////////////////////////////

        public void SetVertexPosition(int shapeIndex, int vertexIndex, Vector3 position)
        {
            m_Shape = m_Shapes.GetArrayElementAtIndex(shapeIndex);
            m_Vertices = m_Shape.FindPropertyRelative(nameof(m_Vertices));
            m_Vertex = m_Vertices.GetArrayElementAtIndex(vertexIndex);
            m_Vertex.FindPropertyRelative(nameof(m_Position)).vector3Value = position;

            m_IsDirty = true;
        }

        public void RemoveVertex(int shapeIndex, int vertexIndex)
        {
            m_Shape = m_Shapes.GetArrayElementAtIndex(shapeIndex);
            m_Vertices = m_Shape.FindPropertyRelative(nameof(m_Vertices));
            m_Vertices.DeleteArrayElementAtIndex(vertexIndex);

            m_IsDirty = true;
        }

        public void InsertVertex(int shapeIndex, int vertexIndex, Vector3 position)
        {
            m_Shape = m_Shapes.GetArrayElementAtIndex(shapeIndex);
            m_Vertices = m_Shape.FindPropertyRelative(nameof(m_Vertices));
            m_Vertices.InsertArrayElementAtIndex(vertexIndex);
            m_Vertex = m_Vertices.GetArrayElementAtIndex(vertexIndex);
            m_Vertex.FindPropertyRelative(nameof(m_Position)).vector3Value = position;

            m_IsDirty = true;
        }

        /*
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
        */

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