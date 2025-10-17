using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace ModularEventArchitecture.Editor.ProjectMap.Map
{
    public class RefreshMapUseCase
    {
        public void Execute(
            Dictionary<string, Rect> assemblyRects,
            Dictionary<string, List<string>> dependencies,
            EditorWindow window,
            DefaultAsset selectedFolder)
        {
            assemblyRects.Clear();
            dependencies.Clear();

            string folderPath = "Assets";
            if (selectedFolder != null)
            {
                folderPath = AssetDatabase.GetAssetPath(selectedFolder);
                if (!AssetDatabase.IsValidFolder(folderPath))
                    folderPath = "Assets";
            }

            var asmdefs = AssetDatabase.FindAssets("t:AssemblyDefinitionAsset", new[] { folderPath })
                .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                .Where(path => !path.Contains("Tests") && !path.Contains("Editor"));

            int x = 10, y = 50;
            foreach (var asmdefPath in asmdefs)
            {
                var asmdef = AssetDatabase.LoadAssetAtPath<AssemblyDefinitionAsset>(asmdefPath);
                var data = JsonUtility.FromJson<ProjectMapWindow.AssemblyDefinitionData>(asmdef.text);

                var rect = new Rect(x, y, 200, 70);
                assemblyRects[data.name] = rect;
                y += 80;
                if (window != null && y > window.position.height - 100)
                {
                    y = 50;
                    x += 220;
                }

                dependencies[data.name] = new List<string>();
                if (data.references != null)
                {
                    foreach (var reference in data.references)
                    {
                        dependencies[data.name].Add(reference);
                    }
                }
            }
        }
    }
}
