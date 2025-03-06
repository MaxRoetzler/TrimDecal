using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.UIElements;

namespace TrimDecal.Editor
{
    [Overlay(typeof(SceneView), "TrimDecalOverlay", "Trim Decal", false)]
    public class TrimDecalOverlay : Overlay
    {
        private Label titleLabel;
        private FloatField valueField;
        private Toggle toggleField;
        private TrimDecal trimDecal;

        public override VisualElement CreatePanelContent()
        {
            var root = new VisualElement();
            titleLabel = new Label("Shape Properties");
            root.Add(titleLabel);

            toggleField = new Toggle("Enable Feature");
            toggleField.RegisterValueChangedCallback(evt =>
            {
                Debug.Log(evt.newValue);
            });
            root.Add(toggleField);

            return root;
        }

        public void SetTarget(TrimDecal target)
        {
            trimDecal = target;
        }
    }
}