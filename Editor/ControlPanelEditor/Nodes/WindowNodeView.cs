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

    public WindowNodeView(NodeBase node, MainScreenView mainScreenView) : base(Path.Combine(GetScriptPath(), "WindowNodeView.uxml"))
    {
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
        string scriptGUID = AssetDatabase.FindAssets($"t:Script {nameof(WindowNodeView)}")[0];
        string scriptPath = AssetDatabase.GUIDToAssetPath(scriptGUID);
        string directoryPath = Path.GetDirectoryName(scriptPath);
        return directoryPath;
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