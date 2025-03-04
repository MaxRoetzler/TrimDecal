using UnityEngine;

namespace TrimDecal.Editor
{
    public class HandleData
    {
        private const float k_VertexMergeDistance = 0.05f;

        public int controlID;
        public TrimDecal decal;
        public int shapeIndex;
        public int vertexIndex;
        public Plane plane;
        public Vector3 position;
        public Vector3? positionPrev;
        public Vector3? positionNext;

        /////////////////////////////////////////////////////////////////

        public HandleData(TrimDecal decal)
        {
            this.decal = decal;
            plane = new();
        }

        /////////////////////////////////////////////////////////////////

        public void Setup()
        {
            if (shapeIndex > -1)
            {
                TrimShape shape = decal[shapeIndex];
                position = shape[vertexIndex].position;
                plane.SetNormalAndPosition(shape.normal, position);

                if (vertexIndex == 0)
                {
                    positionPrev = shape.isClosed ? shape[shape.count - 1].position : null;
                    positionNext = shape[vertexIndex + 1].position;
                }
                else if (vertexIndex == shape.count - 1)
                {
                    positionPrev = shape[vertexIndex - 1].position;
                    positionNext = shape.isClosed ? shape[0].position : null;
                }
                else
                {
                    positionPrev = shape[vertexIndex - 1].position;
                    positionNext = shape[vertexIndex + 1].position;
                }
                return;
            }

            position = Vector3.zero;
            positionPrev = null;
            positionNext = null;
        }

        public bool IsClosedMesh()
        {
            TrimShape shape = decal[shapeIndex];
            int lastIndex = shape.count - 1;

            if (vertexIndex == 0 && Vector3.Distance(position, shape[lastIndex].position) < k_VertexMergeDistance)
            {
                return true;
            }

            if (vertexIndex == lastIndex && Vector3.Distance(position, shape[0].position) < k_VertexMergeDistance)
            {
                return true;
            }
            return false;
        }

        public bool IsPreviousPosition()
        {
            TrimShape shape = decal[shapeIndex];
            TrimShapeVertex vertex = shape[vertexIndex];

            // Get tangent directions
            Vector3 toPosition = (position - (Vector3)vertex.position).normalized;
            float dotIn = Vector3.Dot(vertex.tangentIn, toPosition);
            float dotOut = Vector3.Dot(vertex.tangentOut, toPosition);

            return dotIn > dotOut;
        }
    }
}