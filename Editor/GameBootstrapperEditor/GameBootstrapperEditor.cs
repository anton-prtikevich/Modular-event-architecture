#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

namespace ModularEventArchitecture
{
    [CustomEditor(typeof(GameBootstrapper))]
    public class GameBootstrapperEditor : UnityEditor.Editor
    {
        public VisualTreeAsset treeAsset;

        private Button addManagerButton;
        private VisualElement managersContainer;
        private bool isMenuOpen = false;
        private GameBootstrapper targetBootstrapper;

        private List<Type> availableManagers;

        protected virtual void OnEnable()
        {
            targetBootstrapper = target as GameBootstrapper;

            var managerTypes = new HashSet<Type>();

            // Поиск в Assembly-CSharp
            var assemblyCSharp = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(assembly => assembly.GetName().Name == "Assembly-CSharp");

            if (assemblyCSharp != null)
            {
                // var csharpManagers = assemblyCSharp.GetTypes()
                //     .Where(t => typeof(ManagerEntity).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);
                // foreach (var type in csharpManagers)
                // {
                //     managerTypes.Add(type);
                // }
            }

            // Поиск в текущей сборке ModularEventArchitecture
            // var currentAssembly = Assembly.GetAssembly(typeof(ManagerEntity));
            // if (currentAssembly != null)
            // {
            //     var assemblyManagers = currentAssembly.GetTypes()
            //         .Where(t => typeof(ManagerEntity).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);
            //     foreach (var type in assemblyManagers)
            //     {
            //         managerTypes.Add(type);
            //     }
            // }

            availableManagers = managerTypes.ToList();
        }

        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();

            if (treeAsset != null)
            {
                treeAsset.CloneTree(root);

                addManagerButton = root.Q<Button>("Button_AddManager");
                managersContainer = root.Q<VisualElement>("ManagersContainer");

                if (addManagerButton != null)
                {
                    addManagerButton.clicked += ToggleManagerMenu;
                }
            }
            else
            {
                Debug.LogWarning("GameBootstrapperEditor: treeAsset is null");
            }

            // Add default inspector properties
            InspectorElement.FillDefaultInspector(root, serializedObject, this);

            return root;
        }

        private void ToggleManagerMenu()
        {
            isMenuOpen = !isMenuOpen;
            
            if (isMenuOpen)
            {
                if (managersContainer != null)
                {
                    managersContainer.style.display = DisplayStyle.Flex;
                    CreateManagerButtons();
                }
            }
            else
            {
                if (managersContainer != null)
                {
                    managersContainer.style.display = DisplayStyle.None;
                    managersContainer.Clear();
                }
            }
        }

        private void CreateManagerButtons()
        {
            if (managersContainer == null) return;

            managersContainer.Clear();

            // var existingManagerTypes = targetBootstrapper.GetComponentsInChildren<ManagerEntity>()
            //     .Where(m => m != null)
            //     .Select(m => m.GetType())
            //     .ToList();

            // foreach (var managerType in availableManagers)
            // {
            //     // Пропускаем типы менеджеров, которые уже существуют
            //     if (existingManagerTypes.Any(t => t == managerType))
            //         continue;

            //     string managerName = managerType.Name;
            //     Button managerButton = new Button(() => AddManager(managerType)) { text = managerName };
            //     managersContainer.Add(managerButton);
            // }

            // // Если нет доступных менеджеров для добавления, показываем сообщение
            // if (managersContainer.childCount == 0)
            // {
            //     var label = new Label("Все менеджеры уже добавлены");
            //     managersContainer.Add(label);
            // }
        }

        private void AddManager(Type managerType)
        {
            // Создаем пустой объект
            var managerObject = new GameObject(managerType.Name);
            managerObject.transform.SetParent(targetBootstrapper.transform);
            
            // Добавляем компонент менеджера
            managerObject.AddComponent(managerType);
            
            // После добавления менеджера, закрываем меню
            ToggleManagerMenu();
            
            // Обновляем инспектор
            EditorUtility.SetDirty(target);
        }
    }
}
#endif
