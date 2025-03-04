using UnityEditor;
using UnityEngine;
using System.Reflection;

namespace TrimDecal.Editor
{
    public static class RaycastUtility
    {
        private delegate bool RaycastHitHandler(Ray ray, Mesh mesh, Matrix4x4 matrix, out RaycastHit hit);
        private static readonly RaycastHitHandler IntersectRayMesh = (RaycastHitHandler)typeof(HandleUtility).GetMethod("IntersectRayMesh", BindingFlags.Static | BindingFlags.NonPublic).CreateDelegate(typeof(RaycastHitHandler));

        /////////////////////////////////////////////////////////////////

        public static bool RaycastPlane(Plane plane, Vector2 position, out RaycastHit hit)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(position);
            hit = new RaycastHit();

            if (plane.Raycast(ray, out float distance))
            {
                hit.point = ray.GetPoint(distance);
                hit.normal = plane.normal;

                if (EditorSnapSettings.gridSnapEnabled)
                {
                    hit.point = SnapToGrid(hit.point);
                }
                return true;
            }
            return false;
        }

        public static bool RaycastWorld(Vector2 position, out RaycastHit hit)
        {
            hit = new RaycastHit();

            GameObject gameObject = HandleUtility.PickGameObject(position, false);
            if (!gameObject)
            {
                return RaycastPlane(new Plane(Vector3.up, 0f), position, out hit);
            }

            Ray ray = HandleUtility.GUIPointToWorldRay(position);
            MeshFilter[] meshFilters = gameObject.GetComponentsInChildren<MeshFilter>();
            float minDistance = float.MaxValue;

            foreach (MeshFilter meshFilter in meshFilters)
            {
                Mesh mesh = meshFilter.sharedMesh;
                if (!mesh)
                {
                    continue;
                }

                RaycastHit meshHit;
                if (IntersectRayMesh(ray, mesh, meshFilter.transform.localToWorldMatrix, out meshHit))
                {
                    if (meshHit.distance < minDistance)
                    {
                        hit = meshHit;
                        minDistance = hit.distance;
                    }
                }
            }

            if (minDistance == float.MaxValue)
            {
                Collider[] colliders = gameObject.GetComponentsInChildren<Collider>();
                foreach (Collider collider in colliders)
                {
                    RaycastHit colliderHit;
                    if (collider.Raycast(ray, out colliderHit, Mathf.Infinity))
                    {
                        if (colliderHit.distance < minDistance)
                        {
                            hit = colliderHit;
                            minDistance = hit.distance;
                        }
                    }
                }
            }

            if (minDistance == float.MaxValue)
            {
                hit.point = Vector3.Project(gameObject.transform.position - ray.origin, ray.direction) + ray.origin;
            }
            return true;
        }

        /////////////////////////////////////////////////////////////////

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
    }
}
