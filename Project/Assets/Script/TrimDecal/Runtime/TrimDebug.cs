using UnityEngine;

namespace Project
{
    public static class TrimDebug
    {
        public static void DrawMesh(Mesh mesh)
        {

        }

        public static void DrawShape(TrimShape shape)
        {
            for (int i = 0; i < shape.count; i++)
            {
                DrawPoint(shape, shape[i]);
            }
        }

        /////////////////////////////////////////////////////////////////

        private static void DrawPoint(TrimShape shape, TrimPoint point)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(point.position, point.tangentIn);
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(point.position, point.tangentOut);
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(point.position, point.bitangent);
            Gizmos.color = Color.green;
            Gizmos.DrawRay(point.position, point.bisector);
            Gizmos.color = Color.red;
            Gizmos.DrawRay(point.position, shape.normal);
        }
    }
}