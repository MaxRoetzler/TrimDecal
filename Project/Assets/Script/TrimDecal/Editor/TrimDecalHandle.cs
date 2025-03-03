using UnityEngine.Rendering;
using UnityEngine;
using UnityEditor;

namespace TrimDecal.Editor
{
    public class TrimDecalHandle
    {
        private const float k_ShapeSelectDistance = 10.0f;
        private const float k_VertexMergeDistance = 0.05f;

        private Plane m_Plane;
        private Preview m_Preview;

        private HandleData m_Data;
        private TrimDecal m_Decal;
        private TrimSerializer m_Serializer;

        private int m_ShapeSelection;
        private int m_VertexSelection;

        private HandleBase m_Handle;
        private HandleBase[] m_Handles;

        /////////////////////////////////////////////////////////////////

        public TrimDecalHandle(TrimDecal decal, TrimSerializer serializer)
        {
            m_Decal = decal;
            m_Serializer = serializer;

            m_Data = new(decal);
            m_Handles = new HandleBase[]
            {
                new HandleVertexDelete(m_Data, serializer),
                new HandleVertexInsert(m_Data, serializer),
                new HandleVertexMove(m_Data, serializer),
            };
            m_Handle = m_Handles[2];

            m_Plane = new();
            m_Preview = new();

            m_ShapeSelection = -1;
            m_VertexSelection = -1;
        }

        /////////////////////////////////////////////////////////////////

        public void Draw()
        {
            if (m_Decal.count == 0)
            {
                return;
            }

            Event e = Event.current;
            int controlID = GUIUtility.GetControlID(FocusType.Passive);
            HandleUtility.AddDefaultControl(controlID);
            m_Data.controlID = controlID;

            if (e.type == EventType.Repaint)
            {
                // Draw Scene View handles
                DrawHandles(e);
                m_Handle.Preview(e);
            }

            // Skip layout update & viewport navigation
            if (e.type == EventType.Layout || e.alt || e.button != 0)
            {
                return;
            }

            // Update active handle
            if (m_Handle.isActive)
            {
                m_Handle.Perform(e);
                return;
            }

            if (e.type == EventType.MouseDown)
            {
                GetHandleContext(e);

                foreach (HandleBase handle in m_Handles)
                {
                    if (handle.CanEnter(e))
                    {
                        m_Handle.Exit(e);
                        m_Handle = handle;
                        m_Handle.Enter(e);
                        break;
                    }
                }
            }
        }

        private bool IsClosedMesh()
        {
            TrimShape shape = m_Decal[m_ShapeSelection];
            int lastIndex = shape.count - 1;

            if (m_VertexSelection == 0 && Vector3.Distance(m_Preview.position, shape[lastIndex].position) < k_VertexMergeDistance)
            {
                return true;
            }

            if (m_VertexSelection == lastIndex && Vector3.Distance(m_Preview.position, shape[0].position) < k_VertexMergeDistance)
            {
                return true;
            }
            return false;
        }

        private bool IsPointingAtInTangent()
        {
            TrimShape shape = m_Decal[m_ShapeSelection];
            TrimShapeVertex vertex = shape[m_VertexSelection];

            // Get tangent directions
            Vector3 toPosition = (m_Preview.position - (Vector3)vertex.position).normalized;
            float dotIn = Vector3.Dot(vertex.tangentIn, toPosition);
            float dotOut = Vector3.Dot(vertex.tangentOut, toPosition);

            return dotIn > dotOut;
        }

        private void GetPreviewPositions()
        {
            TrimShape shape = m_Decal[m_ShapeSelection];
            m_Preview.position = shape[m_VertexSelection].position;

            if (m_VertexSelection == 0)
            {
                m_Preview.positionIn = shape.isClosed ? shape[shape.count - 1].position : null;
                m_Preview.positionOut = shape[m_VertexSelection + 1].position;
            }
            else if (m_VertexSelection == shape.count - 1)
            {
                m_Preview.positionIn = shape[m_VertexSelection - 1].position;
                m_Preview.positionOut = shape.isClosed ? shape[0].position : null;
            }
            else
            {
                m_Preview.positionIn = shape[m_VertexSelection - 1].position;
                m_Preview.positionOut = shape[m_VertexSelection + 1].position;
            }

            /*
            PreviewAction = PreviewMoveAction;
            RealizeAction = RealizeMoveAction;
            */
        }

        /////////////////////////////////////////////////////////////////

        private void DrawHandles(Event e)
        {
            int controlID = GUIUtility.GetControlID(FocusType.Passive);
            HandleUtility.AddDefaultControl(controlID);
            Handles.zTest = CompareFunction.Always;

            for (int i = 0; i < m_Decal.count; i++)
            {
                if (m_Decal[i].count < 2)
                {
                    continue;
                }

                TrimShape shape = m_Decal[i];
                int vertexCount = shape.count;
                bool isShapeSelected = i == m_Data.shapeIndex;
                int closedCount = vertexCount + (shape.isClosed ? 1 : 0);
                Vector3[] positions = new Vector3[closedCount];

                for (int j = 0; j < closedCount; j++)
                {
                    positions[j] = shape[j % vertexCount].position;

                    if (isShapeSelected && j < vertexCount)
                    {
                        Handles.color = GetSelectionColor(j == m_Data.vertexIndex);
                        Handles.DotHandleCap(controlID, positions[j], Quaternion.identity, 0.02f, EventType.Repaint);
                    }
                }

                Handles.color = GetSelectionColor(isShapeSelected);
                Handles.DrawAAPolyLine(positions);
            }
        }

        private void GetHandleContext(Event e)
        {
            int controlID = GUIUtility.GetControlID(FocusType.Passive);
            HandleUtility.AddDefaultControl(controlID);

            if (e.type == EventType.MouseDown)
            {
                float handleSize = 0.0f;
                float mouseDistance = 0.0f;
                float closestShapeDistance = float.MaxValue;
                int closestShapeIndex = -1;

                for (int i = 0; i < m_Decal.count; i++)
                {
                    if (m_Decal[i].count < 2)
                    {
                        continue;
                    }

                    TrimShape shape = m_Decal[i];
                    int vertexCount = shape.count;
                    int closedCount = vertexCount + (shape.isClosed ? 1 : 0);

                    for (int j = 0; j < vertexCount; j++)
                    {
                        Vector3 position = shape[j].position;
                        handleSize = HandleUtility.GetHandleSize(position) * 0.05f;
                        mouseDistance = HandleUtility.DistanceToCircle(position, handleSize * 2.0f);

                        // Check for vertex click
                        if (mouseDistance < handleSize * 2.0f)
                        {
                            m_Data.shapeIndex = i;
                            m_Data.vertexIndex = j;
                            m_Data.context = HandleContext.Vertex;
                            return;
                        }

                        // Track closest clicked segment
                        Vector3 positionA = shape[(j + 0) % vertexCount].position;
                        Vector3 positionB = shape[(j + 1) % vertexCount].position;
                        mouseDistance = HandleUtility.DistanceToLine(positionA, positionB);

                        if (mouseDistance < closestShapeDistance)
                        {
                            closestShapeDistance = mouseDistance;
                            closestShapeIndex = i;
                        }
                    }

                    if (closestShapeDistance < k_ShapeSelectDistance)
                    {
                        m_Data.shapeIndex = i;
                        m_Data.vertexIndex = -1;
                        m_Data.context = HandleContext.Shape;
                        return;
                    }
                }

                m_Data.shapeIndex = -1;
                m_Data.vertexIndex = -1;
                m_Data.context = HandleContext.None;
            }
        }

        /////////////////////////////////////////////////////////////////

        private Vector3 SnapToGrid(Vector3 position)
        {
            Vector3 grid = EditorSnapSettings.move;
            return new Vector3()
            {
                x = Mathf.Round(position.x / grid.x) * grid.x,
                y = Mathf.Round(position.y / grid.y) * grid.y,
                z = Mathf.Round(position.z / grid.z) * grid.z,
            };
        }

        private bool RaycastPlane(Vector2 position, out Vector3 point)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(position);

            if (m_Plane.Raycast(ray, out float distance))
            {
                point = ray.GetPoint(distance);

                if (EditorSnapSettings.gridSnapEnabled)
                {
                    point = SnapToGrid(point);

                }
                return true;
            }
            point = default;
            return false;
        }

        private Color GetSelectionColor(bool state)
        {
            return state ? Color.yellow : Color.gray;
        }

        /////////////////////////////////////////////////////////////////

        private struct Preview
        {
            public bool isValid;
            public int vertexIndex;
            public Vector3 position;
            public Vector3? positionIn;
            public Vector3? positionOut;
        }
    }
}
