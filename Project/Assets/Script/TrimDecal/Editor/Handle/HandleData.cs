using UnityEditor;
using UnityEngine;

namespace TrimDecal.Editor
{
    public class HandleData
    {
        public int controlID;
        public TrimDecal decal;
        public int shapeIndex;
        public int vertexIndex;
        public HandleContext context;
        public Vector3 position;
        public Vector3? positionPrev;
        public Vector3? positionNext;

        /////////////////////////////////////////////////////////////////

        public HandleData(TrimDecal decal)
        {
            this.decal = decal;
        }

        /////////////////////////////////////////////////////////////////

        public void GetNeighboringPositions()
        {
            TrimShape shape = decal[shapeIndex];
            position = shape[vertexIndex].position;

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
        }
    }
}