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
    [CustomEditor(typeof(GameEntity), true)]
    public class GameEntityEditor : UnityEditor.Editor
    {
        //-------------------------------------------------------------------------------------
        public VisualTreeAsset treeAsset;

        private Button _ADDModule;
        private VisualElement _modulesContainer;
        private bool _isMenuOpen = false;
        private GameEntity _targetEntity;

        // private List<Type> availableModules; // больше не нужен, фильтрация будет происходить динамически
        //!-------------------------------------------------------------------------------------


        protected virtual void OnEnable()
        {
            try
            {
                _targetEntity = target as GameEntity;

                var moduleTypes = new HashSet<Type>();

                // Поиск в Assembly-CSharp
                var assemblyCSharp = AppDomain.CurrentDomain.GetAssemblies()
                    .FirstOrDefault(assembly => assembly.GetName().Name == "Assembly-CSharp");

                if (assemblyCSharp != null)
                {
                    var csharpModules = assemblyCSharp.GetTypes()
                        .Where(t => typeof(ModuleBase).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);
                    foreach (var type in csharpModules)
                    {
                        moduleTypes.Add(type);
                    }
                }

                // Поиск в текущей сборке ModularEventArchitecture
                var currentAssembly = Assembly.GetAssembly(typeof(ModuleBase));
                if (currentAssembly != null)
                {
                    var assemblyModules = currentAssembly.GetTypes()
                        .Where(t => typeof(ModuleBase).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);
                    foreach (var type in assemblyModules)
                    {
                        moduleTypes.Add(type);
                    }
                }

                // Удаляем фильтрацию из OnEnable, переносим в CreateModuleButtons
            }
            catch (Exception e)
            {
                Debug.LogError($"Error in GameEntityEditor.OnEnable: {e}");
                // availableModules = new List<Type>();
            }
        }

        public override VisualElement CreateInspectorGUI()
        {
            try
            {
                VisualElement root = new VisualElement();

                if (treeAsset != null)
                {
                    treeAsset.CloneTree(root);

                    _ADDModule = root.Q<Button>("Button_ADDMondule");
                    _modulesContainer = root.Q<VisualElement>("ModulesContainer");

                    if (_ADDModule != null)
                    {
                        _ADDModule.clicked += ToggleModuleMenu;
                    }
                }
                else
                {
                    Debug.LogWarning("GameEntityEditor: treeAsset is null");
                }

                // Add default inspector properties
                InspectorElement.FillDefaultInspector(root, serializedObject, this);

                return root;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error in GameEntityEditor.CreateInspectorGUI: {e}");
                return new VisualElement();
            }
        }

        private void ToggleModuleMenu()
        {
            _isMenuOpen = !_isMenuOpen;
            
            if (_isMenuOpen)
            {
                if (_modulesContainer != null)
                {
                    _modulesContainer.style.display = DisplayStyle.Flex;
                    CreateModuleButtons();
                }
            }
            else
            {
                if (_modulesContainer != null)
                {
                    _modulesContainer.style.display = DisplayStyle.None;
                    _modulesContainer.Clear();
                }
            }
        }

        private void CreateModuleButtons()
        {
            if (_modulesContainer == null) return;

            _modulesContainer.Clear();

            // Получаем все типы модулей из всех сборок
            var moduleTypes = new HashSet<Type>();
            var allAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in allAssemblies)
            {
                try
                {
                    var types = assembly.GetTypes()
                        .Where(t => typeof(ModuleBase).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);
                    foreach (var type in types)
                    {
                        moduleTypes.Add(type);
                    }
                }
                catch { /* некоторые сборки могут выбрасывать исключения при GetTypes */ }
            }

            // Получаем ScriptableObject TagsBuilder
            var tagsBuilder = UnityEditor.AssetDatabase.FindAssets("t:TagsBuilder")
                .Select(guid => UnityEditor.AssetDatabase.GUIDToAssetPath(guid))
                .Select(path => UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.ScriptableObject>(path))
                .OfType<ModularEventArchitecture.TagsBuilder>()
                .FirstOrDefault();

            // Получаем текущие теги сущности (массив строк)
            string[] entityTags = null;
            var tagField = _targetEntity.GetType().GetField("EntityTag");
            if (tagField != null)
            {
                var tagValue = tagField.GetValue(_targetEntity);
                if (tagValue is Array tagArray)
                {
                    entityTags = new string[tagArray.Length];
                    for (int i = 0; i < tagArray.Length; i++)
                    {
                        entityTags[i] = tagArray.GetValue(i)?.ToString();
                    }
                }
                else if (tagValue != null)
                {
                    entityTags = new string[] { tagValue.ToString() };
                }
            }

            // Фильтруем модули по TagsBuilder.ModuleTagPairs
            var compatibleModules = moduleTypes.Where(moduleType =>
            {
                if (tagsBuilder == null || tagsBuilder.ModuleTagPairs == null)
                    return true; // если нет ScriptableObject — показываем все

                var pair = tagsBuilder.ModuleTagPairs.FirstOrDefault(p => p.ModuleReference == moduleType.Name);
                if (pair == null || string.IsNullOrEmpty(pair.CompatibleTag))
                    return true;

                // Сравниваем каждый тег
                if (entityTags == null || entityTags.Length == 0)
                    return false;

                foreach (var tag in entityTags)
                {
                    if (pair.CompatibleTag == tag)
                        return true;
                }
                return false;
            });

            foreach (var moduleType in compatibleModules)
            {
                if (_targetEntity.GetComponent(moduleType) != null) continue;

                string moduleName = moduleType.Name;

                Button moduleButton = new Button(() => AddModule(moduleType)) { text = moduleName };

                _modulesContainer.Add(moduleButton);
            }
        }

        private void AddModule(Type moduleType)
        {
            var newModule = _targetEntity.gameObject.AddComponent(moduleType) as ModuleBase;

            // newModule.Setup(_targetEntity);

            _targetEntity.AddModule(newModule);

            // После добавления модуля, закрываем меню
            ToggleModuleMenu();
            
            // Обновляем инспектор
            EditorUtility.SetDirty(target);
        }
    }
}
#endif
