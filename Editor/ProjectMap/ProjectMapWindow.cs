// Editor/ProjectMapWindow.cs
using System;
using System.Collections.Generic;
using UnityEditor;
using ModularEventArchitecture.Editor.ProjectMap.Map;
using UnityEngine;

/// <summary>
/// Окно редактора для отображения карты проекта,
/// </summary>
public class ProjectMapWindow : EditorWindow 
{
    //---------------------------------------------------------------------
    
    /// Use Case для обновления карты проекта
    private RefreshMapUseCase _refreshMapUseCase = new RefreshMapUseCase();
    /// Use Case для поиска папки Scripts
    //---------------------------------------------------------------------
    private Dictionary<string, Rect> _assemblyRects = new Dictionary<string, Rect>();
    private Dictionary<string, List<string>> _dependencies = new Dictionary<string, List<string>>();
    private Vector2 _scrollPosition;
    private float _zoom = 1f;
    //!---------------------------------------------------------------------

    // Выбранная папка для поиска сборок
    private DefaultAsset selectedFolder;

    [MenuItem("Tools/Архитектура/Карта проекта")]
    public static void ShowWindow() => GetWindow<ProjectMapWindow>("Карта проекта");

    void OnGUI()
    {
        DrawToolbar();
        DrawMap();
    }

    void DrawToolbar()
    {
        GUILayout.BeginHorizontal(EditorStyles.toolbar);
        if (GUILayout.Button("Создать карту", EditorStyles.toolbarButton)) RefreshMap();
        if (GUILayout.Button("Создать фичу", EditorStyles.toolbarButton, GUILayout.Width(110)))
        {
            CreateFeatureWindow.ShowWindow();
        }
        GUILayout.Space(10);
        GUILayout.Label("Папка:", GUILayout.Width(40));
        selectedFolder = (DefaultAsset)EditorGUILayout.ObjectField(selectedFolder, typeof(DefaultAsset), false, GUILayout.Width(200));
        if (GUILayout.Button("Найти Scripts", EditorStyles.toolbarButton, GUILayout.Width(110)))
        {
            FindScriptsFolderService _findScriptsFolderUseCase = new FindScriptsFolderService();
            selectedFolder = _findScriptsFolderUseCase.FindScriptsFolder();
        }

        GUILayout.EndHorizontal();
    }

    private void RefreshMap()
    {
        _refreshMapUseCase.Execute(_assemblyRects, _dependencies, this, selectedFolder);
    }

    void DrawMap()
    {
        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
        GUILayout.Label("Архитектура проекта", EditorStyles.boldLabel);

        // Рисуем связи между сборками
        foreach (var assembly in _dependencies)
        {
            foreach (var dependency in assembly.Value)
            {
                if (_assemblyRects.ContainsKey(dependency))
                {
                    DrawConnection(_assemblyRects[assembly.Key], _assemblyRects[dependency]);
                }
            }
        }

        // Рисуем ноды сборок
        foreach (var assemblyRect in _assemblyRects)
        {
            DrawAssemblyNode(assemblyRect.Key, assemblyRect.Value);
        }

        EditorGUILayout.EndScrollView();
    }

    void DrawAssemblyNode(string assemblyName, Rect rect)
    {
        GUI.Box(rect, GUIContent.none, "flow node 0");

        // Иконка в зависимости от типа сборки
        var iconRect = new Rect(rect.x + 5, rect.y + 5, 20, 20);
        GUI.DrawTexture(iconRect, EditorGUIUtility.FindTexture("cs Script Icon"));

        // Название сборки
        var labelRect = new Rect(rect.x + 30, rect.y + 5, rect.width - 35, 20);
        GUI.Label(labelRect, assemblyName, EditorStyles.boldLabel);

        // Количество Use Cases
        // var useCases = FindUseCasesInAssembly(assemblyName);
        var countRect = new Rect(rect.x + 5, rect.y + 30, rect.width - 10, 16);
        // GUI.Label(countRect, $"{useCases.Length} Use Cases", EditorStyles.miniLabel);

        // Кнопка для деталей
        var buttonRect = new Rect(rect.x + 5, rect.y + 50, (rect.width - 20) / 2, 16);
        if (GUI.Button(buttonRect, "Детали", EditorStyles.miniButton))
        {
            ShowAssemblyDetails(assemblyName);
        }

        // Кнопка "Выделить папку"
        var selectFolderRect = new Rect(rect.x + 10 + (rect.width - 20) / 2, rect.y + 50, (rect.width - 20) / 2, 16);
        if (GUI.Button(selectFolderRect, "Папка", EditorStyles.miniButton))
        {
            string folder = FindAsmdefFolder(assemblyName);
            if (!string.IsNullOrEmpty(folder))
            {
                var asset = AssetDatabase.LoadAssetAtPath<DefaultAsset>(folder);
                if (asset != null)
                    Selection.activeObject = asset;
            }
        }
    }

    // Поиск папки, где лежит asmdef с нужным именем
    private string FindAsmdefFolder(string assemblyName)
    {
        var guids = AssetDatabase.FindAssets($"t:AssemblyDefinitionAsset {assemblyName}");
        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            if (System.IO.Path.GetFileNameWithoutExtension(path) == assemblyName)
                return System.IO.Path.GetDirectoryName(path).Replace("\\", "/");
        }
        return null;
    }

    private void ShowAssemblyDetails(string assemblyName)
    {
        AssemblyDetailsWindow.ShowForAssembly(assemblyName);
    }

    void DrawConnection(Rect from, Rect to)
    {
        Handles.BeginGUI();
        Handles.color = Color.white;
        Handles.DrawLine(
            new Vector2(from.x + from.width, from.y + from.height / 2),
            new Vector2(to.x, to.y + to.height / 2)
        );
        Handles.EndGUI();
    }

    [Serializable]
    public class AssemblyDefinitionData
    {
        public string name;
        public string[] references;
        public string[] includePlatforms;
    }
}