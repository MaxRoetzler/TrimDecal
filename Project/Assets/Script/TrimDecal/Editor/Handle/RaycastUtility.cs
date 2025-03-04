using UnityEditor;
using UnityEngine;

namespace TrimDecal.Editor
{
    public static class RaycastUtility
    {
        private static Vector3 SnapToGrid(Vector3 position)
        {
            Vector3 grid = EditorSnapSettings.move;
            return new Vector3()
            {
                x = Mathf.Round(position.x / grid.x) * grid.x,
                y = Mathf.Round(position.y / grid.y) * grid.y,
                z = Mathf.Round(position.z / grid.z) * grid.z,
            };
        }

        public static bool RaycastPlane(Plane plane, Vector2 position, out Vector3 point)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(position);

            if (plane.Raycast(ray, out float distance))
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
    }
}
