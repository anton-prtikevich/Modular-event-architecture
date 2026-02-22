using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

//класс для отображения ноды в редакторе
public class WindowNodeView : UnityEditor.Experimental.GraphView.Node
{ 
    public Action<WindowNodeView> OnNodeSelected;

    public Port inputPort;
    public Port outputPort;
    
    //ссылка на ноду в скриптовом объекте
    public NodeBase node;
    public WindowNodeModel windowNodeModel;
    public MainScreenView MainScreenView;

    private IMGUIContainer container2;
    private UnityEditor.Editor editor;

    //сериализованный объект
    private SerializedObject dataObject;
    private SerializedProperty dataProperty;
    private ScrollView contents;

    // Кэш рефлексии для кнопок — вычисляется один раз, а не каждый кадр
    private struct CachedButtonMethod
    {
        public MethodInfo method;
        public string buttonName;
        public ModularEventArchitecture.ButtonMode mode;
    }
    private List<CachedButtonMethod> _cachedButtonMethods;
    private Type _cachedDataType;
    private bool _isSelected;
    private IMGUIContainer _container1;

    public WindowNodeView(NodeBase node, MainScreenView mainScreenView) : base()
    {
        // Попытка безопасно загрузить UXML шаблон: сначала искать в Packages, затем в Assets
        try
        {
            string uxmlPath = FindUxmlPath("WindowNodeView.uxml", "WindowNodeView");
            if (!string.IsNullOrEmpty(uxmlPath))
            {
                var vta = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);
                if (vta != null)
                {
                    vta.CloneTree(this);
                }
            }
        }
        catch (Exception) { }
        windowNodeModel = node as WindowNodeModel;
        this.node = node;
        this.title = node.name;
        //установить ключ для сохранения данных
        this.viewDataKey = node.guid;

        MainScreenView = mainScreenView;

        CreatePorts();
        SetupBgColorElement();
        SetupRegisterCallback();

        //установить стиль
        SetupStyleNode(node);

        // var contents = this.Q<VisualElement>("contents");
        contents = this.Q<ScrollView>();
        

        var serializedObject = new SerializedObject(node);
        dataProperty = serializedObject.FindProperty("data");

        if (dataProperty != null)
        {
            _container1 = new IMGUIContainer(() => 
            {
                // Обновляем данные только когда нода выделена
                if (_isSelected) serializedObject.Update();

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(dataProperty, true);
                
                if (_isSelected && EditorGUI.EndChangeCheck())
                {
                    if(container2 != null) container2.SetEnabled(dataProperty.objectReferenceValue != null);
                }
                
                if (_isSelected && serializedObject.hasModifiedProperties)
                {
                    serializedObject.ApplyModifiedProperties();
                    Cretecontainer2();
                }
            });
            _container1.cullingEnabled = true;
            
            contents.Add(_container1);

            if(dataProperty.objectReferenceValue != null) Cretecontainer2();

            SetupLabelColorElement(_container1);
        }
    }

    public override void OnSelected()
    {
        base.OnSelected();
        _isSelected = true;
        // Принудительно перерисовать контейнеры при выделении
        _container1?.MarkDirtyRepaint();
        container2?.MarkDirtyRepaint();
        OnNodeSelected?.Invoke(this);
    }

    public override void OnUnselected()
    {
        base.OnUnselected();
        _isSelected = false;
    }

    private void CacheButtonMethods(UnityEngine.Object target)
    {
        if (target == null) 
        {
            _cachedButtonMethods = null;
            _cachedDataType = null;
            return;
        }

        var targetType = target.GetType();
        // Пересчитываем только если тип данных изменился
        if (_cachedDataType == targetType && _cachedButtonMethods != null) return;

        _cachedDataType = targetType;
        _cachedButtonMethods = new List<CachedButtonMethod>();

        var methods = targetType.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        foreach (var method in methods)
        {
            var buttonAttribute = method.GetCustomAttribute<ModularEventArchitecture.ButtonAttribute>();
            if (buttonAttribute == null) continue;

            _cachedButtonMethods.Add(new CachedButtonMethod
            {
                method = method,
                buttonName = string.IsNullOrEmpty(buttonAttribute.buttonName) 
                    ? ObjectNames.NicifyVariableName(method.Name) 
                    : buttonAttribute.buttonName,
                mode = buttonAttribute.mode
            });
        }
    }

    private void Cretecontainer2()
    {
        //удалить contents2 из contents
        if(container2 != null)
        {
            contents.Remove(container2);
        }
        
        if(dataProperty.objectReferenceValue != null)
        {
            SerializedObject dataObject = new SerializedObject(dataProperty.objectReferenceValue);

            // Кешируем рефлексию один раз
            CacheButtonMethods(dataProperty.objectReferenceValue);

            // Контейнер 2 - для полей объекта data
            container2 = new IMGUIContainer(() => 
            {
                // Обновляем данные только когда нода выделена
                if (_isSelected) dataObject.Update();

                SerializedProperty iterator = dataObject.GetIterator();
                bool enterChildren = true;

                while (iterator.NextVisible(enterChildren))
                {
                    enterChildren = false;
                    if (iterator.name == "m_Script") continue;

                    EditorGUILayout.PropertyField(iterator, true);
                }

                if (_isSelected && dataObject.hasModifiedProperties)
                {
                    dataObject.ApplyModifiedProperties();
                }

                // Рисуем кнопки только когда нода выделена
                if (_isSelected && _cachedButtonMethods != null)
                {
                    var target = dataProperty.objectReferenceValue;
                    foreach (var cached in _cachedButtonMethods)
                    {
                        bool shouldEnable = true;
                        switch (cached.mode)
                        {
                            case ModularEventArchitecture.ButtonMode.EnabledInPlayMode:
                                shouldEnable = Application.isPlaying;
                                break;
                            case ModularEventArchitecture.ButtonMode.DisabledInPlayMode:
                                shouldEnable = !Application.isPlaying;
                                break;
                        }

                        EditorGUI.BeginDisabledGroup(!shouldEnable);

                        if (GUILayout.Button(cached.buttonName))
                        {
                            cached.method.Invoke(target, null);
                            EditorUtility.SetDirty(target);
                            AssetDatabase.SaveAssets();
                        }
                            
                        EditorGUI.EndDisabledGroup();
                    }
                }
            });
            container2.cullingEnabled = true;
        }

        // Начальное состояние container2
        if (container2 != null)
        {
            container2.SetEnabled(true);
            contents.Add(container2);
        }
    }

    //зарегестрировать обработчик двойного клика
    private void SetupRegisterCallback()
    {
        this.RegisterCallback<MouseDownEvent>(evt =>
        {
            if (evt.clickCount == 2) // Двойной клик
            {
                FrameSelected(); // Центрируем камеру на выбранной ноде
            }
        });
    }
    
    public void FrameSelected()
    {
        Rect nodeRect = GetPosition();
        
        Vector3 nodeCenter = new Vector3(nodeRect.x - 100, nodeRect.y - 150, 0 );
        
        MainScreenView.UpdateViewTransform(-nodeCenter,  Vector3.one);
    }

    private void CreatePorts()
    {
        inputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(bool));
        outputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(bool));

        if(outputPort != null)
        {
            outputPort.portName = "выход";
            // mainContainer.Add(outputPort);
        }
        if(inputPort != null)
        {
            inputPort.portName = "вход";
            // mainContainer.Add(inputPort);
        }
    }

    private void SetupBgColorElement()
    {
        var bgColorElement = this.Q<ColorField>("BgColor");
        
        using (var serializedObject = new SerializedObject(node))
        {
            serializedObject.Update();
            
            //задать цвет подложки main container
            bgColorElement.value = node.ColorNode;
            mainContainer.style.backgroundColor = node.ColorNode;

            bgColorElement.RegisterValueChangedCallback(evt => 
            {
                    node.ColorNode = evt.newValue;
                    mainContainer.style.backgroundColor = evt.newValue;
                using (var updatedObject = new SerializedObject(node))
                {
                }
            });
        }
    }

    private void SetupLabelColorElement(VisualElement IMGUIContainer)
    {
        var labelColorElement = this.Q<ColorField>("LabelColor");
        
        labelColorElement.value = Color.white;
    }

    private static string GetScriptPath()
    {
        // Безопасный поиск: ищем MonoScript с классом WindowNodeView.
        // Предпочитаем скрипты из Packages, затем из Assets. Если ничего не найдено — фолбек на старый поиск и "Assets".
        var guids = AssetDatabase.FindAssets("t:MonoScript");
        string assetsPath = null;
        string packagesPath = null;

        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var mono = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
            if (mono == null) continue;

            var cls = mono.GetClass();
            // Сравниваем по типу или по имени (безопасно при разных сборках)
            if (cls == typeof(WindowNodeView) || (cls != null && cls.FullName == typeof(WindowNodeView).FullName))
            {
                if (path.StartsWith("Packages/", StringComparison.OrdinalIgnoreCase))
                {
                    packagesPath = Path.GetDirectoryName(path);
                    break; // предпочитаем пакетную версию
                }
                if (path.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
                {
                    assetsPath = Path.GetDirectoryName(path);
                }
                else if (assetsPath == null)
                {
                    assetsPath = Path.GetDirectoryName(path);
                }
            }
        }

        var chosen = packagesPath ?? assetsPath;
        if (!string.IsNullOrEmpty(chosen)) return chosen;

        // Фолбек на старый поиск, но с проверкой на пустой результат
        var fallback = AssetDatabase.FindAssets($"t:Script {nameof(WindowNodeView)}");
        if (fallback != null && fallback.Length > 0)
        {
            var scriptPath = AssetDatabase.GUIDToAssetPath(fallback[0]);
            var dir = Path.GetDirectoryName(scriptPath);
            if (!string.IsNullOrEmpty(dir)) return dir;
        }

        // Крайний фолбек
        return "Assets";
    }

    private static string FindUxmlPath(string fileName, string assetNameWithoutExtension)
    {
        // Ищем по типу VisualTreeAsset и по имени ассета
        // Сначала пытаемся найти в Packages
        var pkgGuids = AssetDatabase.FindAssets($"t:VisualTreeAsset {assetNameWithoutExtension}", new[] { "Packages" });
        if (pkgGuids != null && pkgGuids.Length > 0)
        {
            var p = AssetDatabase.GUIDToAssetPath(pkgGuids[0]);
            if (!string.IsNullOrEmpty(p)) return p;
        }

        // Потом в Assets
        var assetGuids = AssetDatabase.FindAssets($"t:VisualTreeAsset {assetNameWithoutExtension}", new[] { "Assets" });
        if (assetGuids != null && assetGuids.Length > 0)
        {
            var p = AssetDatabase.GUIDToAssetPath(assetGuids[0]);
            if (!string.IsNullOrEmpty(p)) return p;
        }

        // Попробуем найти файл .uxml напрямую (редкий fallback)
        var uxmlGuids = AssetDatabase.FindAssets(fileName);
        if (uxmlGuids != null && uxmlGuids.Length > 0)
        {
            var p = AssetDatabase.GUIDToAssetPath(uxmlGuids[0]);
            if (!string.IsNullOrEmpty(p)) return p;
        }

        return null;
    }

    //тут меняется позиция ноду, так же отвечает за перетаскивание
    public override void SetPosition(Rect newPos)
    {
        base.SetPosition(newPos);

        node.Position = new Vector2(newPos.x, newPos.y);
    }

    private void SetupStyleNode(NodeBase node)
    {
        style.left = node.Position.x;
        style.top = node.Position.y;
    }    
}