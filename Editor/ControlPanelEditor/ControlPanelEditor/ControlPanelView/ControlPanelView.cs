using System.Collections.Generic;
using MyEditor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

// namespace MyScripts.Architecture.Editor.ControlPanel.ControlPanelEditor
// {
    //класс для отображения ноды в редакторе
    public class ControlPanelView : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<ControlPanelView, VisualElement.UxmlTraits> { }
        private BehavioureTree _targetAsset;
        private ScrollView _scrollView;
        private Button _selectedButton;

        private ControlPanelEditor _controlPanelEditor;

        // Храним подписки для корректной отписки
        private List<(Button button, System.Action action)> _buttonSubscriptions = new List<(Button, System.Action)>();

        public ControlPanelView()
        {
            
        }

        internal void Setup(ControlPanelEditor controlPanelEditor)
        {
            UnsubscribeButtons();
            Clear();
            _controlPanelEditor = controlPanelEditor;
            
            // Создаем базовый контейнер
            var container = new VisualElement();
            
            
            // Панель ввода имени (скрыта по умолчанию)
            var nameInputPanel = new VisualElement();
            nameInputPanel.style.flexDirection = FlexDirection.Row;
            nameInputPanel.style.display = DisplayStyle.None;

            var nameField = new TextField();
            nameField.style.flexGrow = 1;
            nameField.value = "Новая доска";

            var confirmButton = new Button() { text = "✓" };
            confirmButton.style.width = 30;
            var cancelButton = new Button() { text = "✕" };
            cancelButton.style.width = 30;

            nameInputPanel.Add(nameField);
            nameInputPanel.Add(confirmButton);
            nameInputPanel.Add(cancelButton);

            // Действие создания доски
            System.Action createBoard = () =>
            {
                string boardName = nameField.value.Trim();
                if (string.IsNullOrEmpty(boardName)) return;

                var config = ControlPanelEditorConfig.Instance;
                string boardsFolderPath;

                if (config != null && config.boardsFolder != null)
                {
                    boardsFolderPath = AssetDatabase.GetAssetPath(config.boardsFolder);
                }
                else
                {
                    boardsFolderPath = EditorUtility.SaveFilePanelInProject(
                        "Создать доску", boardName, "asset",
                        "Назначьте boardsFolder в ControlPanelEditorConfig, чтобы не выбирать каждый раз");
                    if (string.IsNullOrEmpty(boardsFolderPath)) return;

                    var newAsset = ScriptableObject.CreateInstance<BehavioureTree>();
                    AssetDatabase.CreateAsset(newAsset, boardsFolderPath);
                    AssetDatabase.SaveAssets();
                    AddBoardButton(_scrollView, newAsset);
                    nameInputPanel.style.display = DisplayStyle.None;
                    return;
                }

                string path = AssetDatabase.GenerateUniqueAssetPath(boardsFolderPath + "/" + boardName + ".asset");

                var asset = ScriptableObject.CreateInstance<BehavioureTree>();
                AssetDatabase.CreateAsset(asset, path);
                AssetDatabase.SaveAssets();

                AddBoardButton(_scrollView, asset);
                nameInputPanel.style.display = DisplayStyle.None;
                nameField.value = "Новая доска";
            };

            confirmButton.clicked += () => createBoard();
            cancelButton.clicked += () =>
            {
                nameInputPanel.style.display = DisplayStyle.None;
                nameField.value = "Новая доска";
            };

            // Enter подтверждает, Escape отменяет
            nameField.RegisterCallback<KeyDownEvent>(evt =>
            {
                if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
                {
                    createBoard();
                    evt.StopPropagation();
                }
                else if (evt.keyCode == KeyCode.Escape)
                {
                    nameInputPanel.style.display = DisplayStyle.None;
                    nameField.value = "Новая доска";
                    evt.StopPropagation();
                }
            });

            // Кнопка "+ Создать новую доску" показывает панель ввода
            var createButton = new Button(() =>
            {
                nameInputPanel.style.display = DisplayStyle.Flex;
                nameField.Focus();
                nameField.SelectAll();
            })
            {
                text = "+ Создать новую доску"
            };
            
            // Создаем скролл для списка
            _scrollView = new ScrollView();
            
            // Добавляем элементы в контейнер
            container.Add(createButton);
            container.Add(nameInputPanel);
            container.Add(new HelpBox("Выберите доску из списка", HelpBoxMessageType.None));
            container.Add(_scrollView);
            DrawItemsList(_scrollView);
            
            Add(container);
        }

        private void UnsubscribeButtons()
        {
            foreach (var (button, action) in _buttonSubscriptions)
            {
                button.clicked -= action;
            }
            _buttonSubscriptions.Clear();
        }

        private void RefreshItemsList()
        {
            UnsubscribeButtons();
            _scrollView.Clear();
            DrawItemsList(_scrollView);
        }

        private void DrawAssetSelection()
        {
            Debug.Log("DrawAssetSelection");
            EditorGUILayout.HelpBox("Выберите ассет", MessageType.Info);
            
            // _targetAsset = EditorGUILayout.ObjectField("Asset", _targetAsset, typeof(BehavioureTree), false) as BehavioureTree;
            
            if (GUILayout.Button("Создать новый ассет"))
            {
                string path = EditorUtility.SaveFilePanelInProject("Create AvailableItems","AvailableItems","asset","Create a new AvailableItems asset");
                
                if (!string.IsNullOrEmpty(path))
                {
                    _targetAsset = EditorWindow.CreateInstance<BehavioureTree>();
                    AssetDatabase.CreateAsset(_targetAsset, path);
                    AssetDatabase.SaveAssets();
                }
            }
        }

        private void DrawItemsList(VisualElement container)
        {
            //получить список ассетов BehavioureTree        
            string[] guids = AssetDatabase.FindAssets("t:BehavioureTree");
        
            foreach (var item in guids)
            {
                BehavioureTree asset = AssetDatabase.LoadAssetAtPath<BehavioureTree>(AssetDatabase.GUIDToAssetPath(item));
                AddBoardButton(container, asset);
            }
        }

        private void AddBoardButton(VisualElement container, BehavioureTree asset)
        {
            var config = ControlPanelEditorConfig.Instance;
            if (config == null || config.BoardButtonTemplate == null)
            {
                Debug.LogError("ControlPanelEditorConfig: BoardButtonTemplate не назначен!");
                return;
            }

            // Клонируем шаблон кнопки
            var buttonElement = new VisualElement();
            config.BoardButtonTemplate.CloneTree(buttonElement);

            // Настраиваем кнопку выбора доски
            Button selectButton = buttonElement.Q<Button>("Select-board");
            selectButton.text = asset.name;

            System.Action selectAction = () =>
            {
                if (_selectedButton != null)
                {
                    _selectedButton.style.backgroundColor = new Color(0.345098f, 0.345098f, 0.345098f);
                }
                
                _controlPanelEditor.SelectTrget(asset);
                selectButton.style.backgroundColor = new Color(0.136837f, 0.3867925f, 0.1702243f);
                _selectedButton = selectButton;
            };
            selectButton.clicked += selectAction;
            _buttonSubscriptions.Add((selectButton, selectAction));

            // Настраиваем кнопку удаления доски
            Button deleteButton = buttonElement.Q<Button>("Delete-Doard");
            System.Action deleteAction = () =>
            {
                if (EditorUtility.DisplayDialog("Удалить доску", 
                    $"Удалить '{asset.name}'?", "Удалить", "Отмена"))
                {
                    string assetPath = AssetDatabase.GetAssetPath(asset);
                    AssetDatabase.DeleteAsset(assetPath);
                    AssetDatabase.SaveAssets();
                    RefreshItemsList();
                }
            };
            deleteButton.clicked += deleteAction;
            _buttonSubscriptions.Add((deleteButton, deleteAction));

            container.Add(buttonElement);
        }
    }
// }