using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;

namespace Project
{
    /*
    [CustomEditor(typeof(TrimDecal))]
    public class TrimDecalEditor : Editor
    {
        [SerializeField]
        private VisualTreeAsset m_UxmlDocument;

        private SerializedProperty m_Shape;
        private SerializedProperty m_Shapes;

        private ListView m_ShapeView;
        private Toggle m_IsClosedView;
        private Toggle m_IsFlippedView;
        private FloatField m_GridSizeView;
        private GroupBox m_SettingsView;
        private Button m_EditView;

        /////////////////////////////////////////////////////////////////

        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new();
            m_UxmlDocument.CloneTree(root);

            FetchProperties();
            QueryElements(root);
            RegisterEvents();

            return root;
        }

        /////////////////////////////////////////////////////////////////

        private void BindSelection(SerializedProperty property)
        {
            SerializedProperty gridSize = property.FindPropertyRelative("m_GridSize");
            SerializedProperty isClosed = property.FindPropertyRelative("m_IsClosed");
            SerializedProperty isFlipped = property.FindPropertyRelative("m_IsFlipped");

            m_GridSizeView.BindProperty(gridSize);
            m_IsClosedView.BindProperty(isClosed);
            m_IsFlippedView.BindProperty(isFlipped);
        }

        private void UnbindSelection()
        {
            m_GridSizeView.Unbind();
            m_IsClosedView.Unbind();
            m_IsFlippedView.Unbind();
        }

        /////////////////////////////////////////////////////////////////

        private void DuringSceneView(SceneView obj)
        {
            if (m_Shape == null)
            {
                return;
            }

            using (new Handles.DrawingScope())
            {
                Handles.Label(Vector3.zero, "Origin");
            }
        }

        private void OnShapeEdit()
        {
            
        }

        /////////////////////////////////////////////////////////////////

        private void OnShapeSelected(IEnumerable<object> items)
        {
            m_Shape = null;
            using (IEnumerator<object> enumer = items.GetEnumerator())
            {
                if (enumer.MoveNext()) m_Shape = enumer.Current as SerializedProperty;
            }

            if (m_Shape != null)
            {
                m_SettingsView.style.display = DisplayStyle.Flex;
                BindSelection(m_Shape);
                Repaint();
                return;
            }
            m_SettingsView.style.display = DisplayStyle.None;
            UnbindSelection();
        }

        private void OnShapeAdded(IEnumerable<int> obj)
        {

        }

        private void OnShapeRemoved(IEnumerable<int> obj)
        {

        }

        private void BindShapeItem(VisualElement element, int index)
        {
            Label label = element as Label;
            label.text = m_Shapes.GetArrayElementAtIndex(index).displayName;
        }

        private VisualElement MakeShapeItem()
        {
            return new Label();
        }

        /////////////////////////////////////////////////////////////////

        private void FetchProperties()
        {
            m_Shapes = serializedObject.FindProperty(nameof(m_Shapes));
        }

        private void QueryElements(VisualElement root)
        {
            m_ShapeView = root.Q<ListView>("shapes");
            m_ShapeView.makeItem = MakeShapeItem;
            m_ShapeView.bindItem = BindShapeItem;
            m_ShapeView.bindingPath = nameof(m_Shapes);
            m_ShapeView.selectionType = SelectionType.Single;

            m_SettingsView = root.Q<GroupBox>("settings");
            m_SettingsView.style.display = DisplayStyle.None;

            m_IsClosedView = root.Q<Toggle>("isClosed");
            m_IsFlippedView = root.Q<Toggle>("isFlipped");
            m_GridSizeView = root.Q<FloatField>("gridSize");
            m_EditView = root.Q<Button>("edit");
        }

        private void RegisterEvents()
        {
            m_EditView.clicked += OnShapeEdit;
            m_ShapeView.itemsAdded += OnShapeAdded;
            m_ShapeView.itemsRemoved += OnShapeRemoved;
            m_ShapeView.selectionChanged += OnShapeSelected;

            SceneView.duringSceneGui += DuringSceneView;
        }

        private void UnregisterEvents()
        {
            if (m_ShapeView != null)
            {
                m_EditView.clicked -= OnShapeEdit;
                m_ShapeView.itemsAdded -= OnShapeAdded;
                m_ShapeView.itemsRemoved -= OnShapeRemoved;
                m_ShapeView.selectionChanged -= OnShapeSelected;
            }

            SceneView.duringSceneGui -= DuringSceneView;
        }

        /////////////////////////////////////////////////////////////////

        private void OnEnable()
        {
            
        }

        private void OnDisable()
        {
            UnregisterEvents();
        }
    }
    */
}