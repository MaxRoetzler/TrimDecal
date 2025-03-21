using UnityEngine;
using UnityEditor;
using System.Reflection;

namespace TrimMesh.Editor
{
    public static class RaycastUtility
    {
        private delegate bool RaycastHitHandler(Ray ray, Mesh mesh, Matrix4x4 matrix, out RaycastHit hit);
        private static readonly RaycastHitHandler IntersectRayMesh = (RaycastHitHandler)typeof(HandleUtility).GetMethod("IntersectRayMesh", BindingFlags.Static | BindingFlags.NonPublic).CreateDelegate(typeof(RaycastHitHandler));

        /////////////////////////////////////////////////////////////

        public static bool RaycastPlane(Vector2 position, Plane plane, out RaycastHit hit)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(position);
            hit = new RaycastHit();

            if (plane.Raycast(ray, out float distance))
            {
                hit.point = ray.GetPoint(distance);
                hit.normal = plane.normal;
                return true;
            }
            return false;
        }
    }
}