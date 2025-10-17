#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System.Linq;
using System.Reflection;
using ModularEventArchitecture;

namespace ModularEventArchitecture
{
[CustomEditor(typeof(ModuleBase), true)]
    public class ModulesEditor : UnityEditor.Editor
    {
        //-------------------------------------------------------------------------------------
        private ModuleBase _targetModule;
        public VisualTreeAsset treeAsset;
        private VisualElement _buttonsContainer;
        private Button _buttonDeleteModule;

        //!-------------------------------------------------------------------------------------

        private void OnEnable()
        {
            _targetModule = target as ModuleBase;
        }
        
        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();

            treeAsset.CloneTree(root);

            _buttonsContainer = root.Q<VisualElement>("ButtonsContainer");
            _buttonDeleteModule = root.Q<Button>("ButtonDeletModule");

            _buttonDeleteModule.clicked += DeleteModule;

            // Добавить свойства инспектора по умолчанию
            InspectorElement.FillDefaultInspector(root, serializedObject, this);

            // Добавить кнопки для методов с атрибутом ButtonAttribute
            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var methods = target.GetType().GetMethods(flags).Where(m => m.GetParameters().Length == 0 && m.GetCustomAttributes(typeof(ButtonAttribute), true).Length > 0);

            foreach (var method in methods)
            {
                var buttonAttribute = (ButtonAttribute)method.GetCustomAttributes(typeof(ButtonAttribute), true)[0];

                // создать кнопку
                var button = new Button(() =>
                {
                    foreach (var targetObject in targets)
                    {
                        method.Invoke(targetObject, null);
                    }
                })
                {
                    // Use custom button name if provided, otherwise use method name
                    text = buttonAttribute.buttonName ?? ObjectNames.NicifyVariableName(method.Name)
                };

                // Set button enabled state
                bool isEnabled = buttonAttribute.mode == ButtonMode.AlwaysEnabled || 
                (EditorApplication.isPlaying ? buttonAttribute.mode == ButtonMode.EnabledInPlayMode : buttonAttribute.mode == ButtonMode.DisabledInPlayMode);
                
                button.SetEnabled(isEnabled);

                // Add some margin
                button.style.marginTop = 5;
                button.style.marginBottom = 5;

                _buttonsContainer.Add(button);
                // root.Add(button);
            }

            return root;
        }

        private void DeleteModule()
        {
            GameEntity _targetEntity = _targetModule.gameObject.GetComponent<GameEntity>();

            _targetEntity.RemoveModule(_targetModule);

            DestroyImmediate(_targetModule);

            EditorUtility.SetDirty(_targetEntity);
        }
    }
}
#endif
