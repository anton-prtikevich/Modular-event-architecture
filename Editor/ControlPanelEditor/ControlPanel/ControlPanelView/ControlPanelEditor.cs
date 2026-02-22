using System.IO;
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
        // инициализация папки для досок при открытии редактора
        EnsureBoardsFolderAssignedToConfig();
    }
    
    private static void EnsureBoardsFolderAssignedToConfig()
    {
        const string root = "Assets";
        const string configsFolderName = "ControlPanelConfigs";
        const string boardsFolderName = "Boards";

        // Создать Assets/ControlPanelConfigs/Boards, если не существует
        string configsPath = Path.Combine(root, configsFolderName).Replace("\\", "/");
        if (!AssetDatabase.IsValidFolder(configsPath))
        {
            AssetDatabase.CreateFolder(root, configsFolderName);
        }

        string boardsPath = Path.Combine(configsPath, boardsFolderName).Replace("\\", "/");
        if (!AssetDatabase.IsValidFolder(boardsPath))
        {
            AssetDatabase.CreateFolder(configsPath, boardsFolderName);
        }

        // Получаем DefaultAsset для папки
        var defaultAsset = AssetDatabase.LoadAssetAtPath<DefaultAsset>(boardsPath);

        // Найти существующий BoardsFolderConfig или создать новый в папке ControlPanelConfigs
        string[] cfgGuids = AssetDatabase.FindAssets("t:BoardsFolderConfig", new[] { configsPath });
        BoardsFolderConfig boardsConfig = null;
        if (cfgGuids != null && cfgGuids.Length > 0)
        {
            var cfgPath = AssetDatabase.GUIDToAssetPath(cfgGuids[0]);
            boardsConfig = AssetDatabase.LoadAssetAtPath<BoardsFolderConfig>(cfgPath);
        }
        else
        {
            // Создать новый ScriptableObject конфигурации в папке ControlPanelConfigs
            var newCfg = ScriptableObject.CreateInstance<BoardsFolderConfig>();
            string cfgAssetPath = Path.Combine(configsPath, "BoardsFolderConfig.asset").Replace("\\", "/");
            AssetDatabase.CreateAsset(newCfg, cfgAssetPath);
            AssetDatabase.SaveAssets();
            boardsConfig = AssetDatabase.LoadAssetAtPath<BoardsFolderConfig>(cfgAssetPath);
        }

        if (boardsConfig != null && boardsConfig.boardsFolder == null)
        {
            boardsConfig.boardsFolder = defaultAsset;
            EditorUtility.SetDirty(boardsConfig);
            AssetDatabase.SaveAssets();
        }
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
