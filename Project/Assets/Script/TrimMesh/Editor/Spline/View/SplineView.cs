using UnityEngine.Rendering;
using UnityEditor;
using UnityEngine;

namespace TrimMesh.Editor
{

    public class SplineView
    {
        private Matrix4x4 m_Matrix;
        private SplineModel m_Model;
        private MouseCursor m_Cursor;
        private SplineSelection m_Selection;

        /////////////////////////////////////////////////////////////

        public SplineView(SplineModel model, Transform transform)
        {
            m_Model = model;
            m_Selection = new(model);
            m_Model.onModelChanged += m_Selection.AllocateBitmasks;

            m_Cursor = MouseCursor.Arrow;
            m_Matrix = transform.localToWorldMatrix;
        }

        /////////////////////////////////////////////////////////////

        public SplineSelection selection
        {
            get => m_Selection;
        }

        /////////////////////////////////////////////////////////////

        public void SceneGUI(Event e)
        {
            Handles.matrix = m_Matrix;
            Handles.zTest = CompareFunction.Always;

            SetMouseCursor(e);
            m_Selection.Update(e);
            DrawSpline(e);
        }

        /////////////////////////////////////////////////////////////

        private void DrawSpline(Event e)
        {
            if (e.type == EventType.Repaint)
            {
                for (int i = 0; i < m_Model.vertexCount; i++)
                {
                    SplineVertex vertex = m_Model.vertices[i];
                    float handleSize = HandleUtility.GetHandleSize(vertex.position) * 0.04f;
                    bool isHover = m_Selection.nearestVertex == i;

                    Handles.color = GetSelectionColor(m_Selection.vertexMask[i], isHover);
                    Handles.DotHandleCap(0, vertex.position, Quaternion.identity, handleSize, EventType.Repaint);
                    Handles.Label(vertex.position, $"{i:D2}");
                }

                for (int i = 0; i < m_Model.segmentCount; i++)
                {
                    SplineSegment segment = m_Model.segments[i];
                    Vector3 positionA = segment.vertexA.position;
                    Vector3 positionB = segment.vertexB.position;

                    Handles.color = GetSelectionColor(m_Selection.segmentMask[i], false);
                    Handles.DrawLine(positionA, positionB);
                }
            }
        }

        private Color GetSelectionColor(bool isSelected, bool isHover)
        {
            return isSelected ? SplineConstant.colorSelected : isHover ? SplineConstant.colorHover : SplineConstant.colorDefault;
        }

        private void SetMouseCursor(Event e)
        {
            m_Cursor = e.control ? MouseCursor.ArrowMinus : e.shift ? MouseCursor.ArrowPlus : MouseCursor.Arrow;
            EditorGUIUtility.AddCursorRect(SceneView.lastActiveSceneView.camera.pixelRect, m_Cursor);
        }
    }
}