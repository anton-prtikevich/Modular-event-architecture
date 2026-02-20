using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class ControlPanelEditor : EditorWindow
{
    private MainScreenView _treeView;
    private ControlPanelView _сontrolPanelView;
    private VisualElement _rightPanel;
    private BehavioureTree _target;

    [MenuItem("Tools/Панель управления")]
    public static void OpenWindow()
    {
        ControlPanelEditor wnd = GetWindow<ControlPanelEditor>();
        wnd.titleContent = new GUIContent("ControlPanelEditor");
    }
    

    public void CreateGUI()
    {
        VisualElement root = rootVisualElement;

        var config = ControlPanelEditorConfig.Instance;
        if (config == null || config.designMainScreen == null)
        {
            Debug.LogError("ControlPanelEditorConfig или designMainScreen не назначены!");
            return;
        }
        config.designMainScreen.CloneTree(root);
        
        // получить ссылку на дерево         
        _treeView = root.Q<MainScreenView>();
        _сontrolPanelView = root.Q<ControlPanelView>();
        _rightPanel = root.Q<VisualElement>("Right-Panel");

        Button populateBUtton = root.Q<Button>("populateBUtton");
        populateBUtton.clicked -= populateBUttonClick;
        populateBUtton.clicked += populateBUttonClick;

        SetupControlPanel();
        
        // При старте доска не выбрана — скрыть правую панель
        SetRightPanelEnabled(false);
    }

    public void SetupControlPanel() => _сontrolPanelView.Setup(this);

    public void SelectTrget(BehavioureTree target)
    {
        _target = target;
        OnSelectionChange();
    }

    private void OnSelectionChange()
    {
        bool hasTarget = _target != null && _treeView != null;
        SetRightPanelEnabled(hasTarget);

        if (hasTarget)
        {
            _treeView.PopulateView(_target);
        }
    }

    private void SetRightPanelEnabled(bool enabled)
    {
        if (_rightPanel == null) return;
        _rightPanel.SetEnabled(enabled);
        _rightPanel.style.opacity = enabled ? 1f : 0.3f;
    }

    private void populateBUttonClick()
    {
        SetupControlPanel();
    }
}
