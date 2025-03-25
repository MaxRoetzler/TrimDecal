using UnityEngine;

namespace TrimMesh.Editor
{
    public static class SplineConstant
    {
        public const float lineDotGap = 1.0f;
        public const float vertexSelectionDistance = 0.12f;
        public const float segmentSelectionDistance = 20.0f;
        public const float marqueeSelectionThreshold = 1.0f;

        public static readonly Color colorHover = new(0.9f, 0.9f, 0.9f, 1.0f);
        public static readonly Color colorDefault = new(0.5f, 0.5f, 0.5f, 1.0f);
        public static readonly Color colorSelected = new(0.0f, 1.0f, 1.0f, 1.0f);
    }
}