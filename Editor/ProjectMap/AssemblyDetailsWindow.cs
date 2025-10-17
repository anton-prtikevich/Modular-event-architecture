// Editor/AssemblyDetailsWindow.cs
using System.Collections.Generic;
using ModularEventArchitecture.Editor.ProjectMap.Services;
using ModularEventArchitecture.Editor.ProjectMap.UseCases;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Окно редактора для отображения деталей сборки (assembly),
/// </summary>
public class AssemblyDetailsWindow : EditorWindow
{
    private string _newModuleName = "";
    private string _assemblyName;
    private Vector2 _scrollPosition;
    private string _featureFolder; // кэш папки
    private string _lastAssemblyName; // для отслеживания смены

    public static void ShowForAssembly(string assemblyName)
    {
        var window = CreateInstance<AssemblyDetailsWindow>();
        window._assemblyName = assemblyName;
        window.titleContent = new GUIContent($"Details: {assemblyName}");
        window.ShowUtility();

        window.Setup();
    }

    private void Setup()
    {
        // Папка с фичей ищется по имени сборки (первое совпадение папки в Assets/*/имя_сборки), кэшируется
        if (_featureFolder == null || _lastAssemblyName != _assemblyName)
        {
            _featureFolder = FindFeatureFolder(_assemblyName);
            _lastAssemblyName = _assemblyName;
        }
    }

    void OnGUI()
    {
        if (string.IsNullOrEmpty(_assemblyName)) return;

        // _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

        GUILayout.Label(_assemblyName, EditorStyles.largeLabel);

        // EditorGUILayout.EndScrollView();
        // --- Документация ---
        DrowDocumentationSection();
        EditorGUILayout.Space();


        // --- Use Cases ---
        EditorGUILayout.Space();
        GUILayout.Label("Use Cases:", EditorStyles.boldLabel);

        // --- Создание модуля ---
        EditorGUILayout.Space(10);
        DrawBlockCreateModule();
        EditorGUILayout.Space(10);
        DrawBlockCreateUseCase();
        EditorGUILayout.Space(10);
        DrawBlockCreateDTO();
        

}
    private void DrawBlockCreateDTO()
    {
        EditorGUILayout.BeginHorizontal("Box");
        _newModuleName = EditorGUILayout.TextField("Название DTO", _newModuleName);
        if (GUILayout.Button("Создать DTO", GUILayout.Width(150)))
        {
            var useCase = new CreateDTOScriptUseCase();
            useCase.Execute(_featureFolder, _newModuleName);
        }
        EditorGUILayout.EndHorizontal();
    }
    
    private void DrawBlockCreateModule()
    {
        EditorGUILayout.BeginHorizontal("Box");
        _newModuleName = EditorGUILayout.TextField("Название модуля", _newModuleName);
        if (GUILayout.Button("Создать Модуль", GUILayout.Width(150)))
        {
            var useCase = new CreateModuleScriptUseCase();
            useCase.Execute(_featureFolder, _newModuleName);
        }
        EditorGUILayout.EndHorizontal();
    }
    private void DrawBlockCreateUseCase()
    {
        EditorGUILayout.BeginHorizontal("Box");
        _newModuleName = EditorGUILayout.TextField("Название Use Case", _newModuleName);
        if (GUILayout.Button("Создать Use Case", GUILayout.Width(150)))
        {
            var useCase = new CreateUseCaseScriptUseCase();
            useCase.Execute(_featureFolder, _newModuleName);
        }
        EditorGUILayout.EndHorizontal();
    }


    private void DrowDocumentationSection()
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Документация:", EditorStyles.boldLabel);

        string docPath = _featureFolder != null ? System.IO.Path.Combine(_featureFolder, "README.md") : null;
        bool docExists = docPath != null && System.IO.File.Exists(docPath);

        if (!docExists)
        {
            if (GUILayout.Button("Создать файл документации"))
            {
                if (docPath != null)
                {
                    System.IO.File.WriteAllText(docPath, $"Документация для {_assemblyName}\n\n...");
                    AssetDatabase.Refresh();
                }
            }
        }
        else
        {
            if (GUILayout.Button("Открыть файл для редактирования"))
            {
                var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(ToAssetPath(docPath));
                if (asset != null)
                    AssetDatabase.OpenAsset(asset);
            }
        }
        EditorGUILayout.EndHorizontal();

        if (docExists)
        {
            string docText = System.IO.File.ReadAllText(docPath);
            var style = new GUIStyle(EditorStyles.textArea) { wordWrap = true };
            GUI.enabled = false;
            EditorGUILayout.TextArea(docText, style, GUILayout.MinHeight(100));
            GUI.enabled = true;
        }
    }


    // Поиск папки с фичей по имени сборки (ищет в Assets/*/имя_сборки)
    private string FindFeatureFolder(string assemblyName)
    {
        string[] folders = System.IO.Directory.GetDirectories("Assets", "*", System.IO.SearchOption.AllDirectories);
        foreach (var folder in folders)
        {
            string name = System.IO.Path.GetFileName(folder);
            if (name == assemblyName)
                return folder.Replace("\\", "/");
        }
        return null;
    }

    // Преобразует абсолютный путь к asset-пути Unity
    private string ToAssetPath(string absPath)
    {
        absPath = absPath.Replace("\\", "/");
        int idx = absPath.IndexOf("Assets/");
        return idx >= 0 ? absPath.Substring(idx) : absPath;
    }


    private object FindDTOsInAssembly(string assemblyName)
    {
        Debug.Log($"FindDTOsInAssembly: assemblyName={assemblyName}");
        // Здесь нужно реализовать логику поиска DTO в сборке
        return new List<object> { "DTO1", "DTO2", "DTO3" };
    }

    private IEnumerable<object> FindUseCasesInAssembly(string assemblyName)
    {
        Debug.Log($"FindUseCasesInAssembly: assemblyName={assemblyName}");
        // Здесь нужно реализовать логику поиска Use Cases в сборке
        return new List<object> { "UseCase1", "UseCase2", "UseCase3" };
    }

    private void DrawDTOItem(object dto)
    {
        Debug.Log($"DrawDTOItem: dto={dto}");
    }

    void DrawUseCaseItem(string useCaseName)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("• " + useCaseName, GUILayout.Width(300));
        if (GUILayout.Button("Open", GUILayout.Width(60)))
        {
            // Находим и открываем файл
            var guids = AssetDatabase.FindAssets(useCaseName);
            if (guids.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                var asset = AssetDatabase.LoadAssetAtPath<Object>(path);
                AssetDatabase.OpenAsset(asset);
            }
        }
        GUILayout.EndHorizontal();
    }
}