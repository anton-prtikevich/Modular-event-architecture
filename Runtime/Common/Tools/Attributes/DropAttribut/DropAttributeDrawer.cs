// ...existing code...
#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ModularEventArchitecture
{
    /// <summary>
    /// Класс для отрисовки кастомного поля с атрибутом [Drop], теперь показывает имена .asmdef в проекте.
    /// </summary>
    [CustomPropertyDrawer(typeof(DropAttribute))]
    public class DropAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Получаем все asmdef имена только в папке Assets
            var guids = AssetDatabase.FindAssets("t:AssemblyDefinitionAsset", new[] { "Assets" });
            var asmNamesList = guids
                .Select(g => Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(g)))
                .Distinct()
                .OrderBy(n => n)
                .ToList();

            // Попробуем получить список исключённых asmdef из FeatureGeneratorSettings
            var excluded = new System.Collections.Generic.HashSet<string>();
            var settingsGuids = AssetDatabase.FindAssets("FeatureGeneratorSettings");
            if (settingsGuids != null && settingsGuids.Length > 0)
            {
                var settingsPath = AssetDatabase.GUIDToAssetPath(settingsGuids[0]);
                var settingsObj = AssetDatabase.LoadAssetAtPath<UnityEngine.ScriptableObject>(settingsPath);
                if (settingsObj != null)
                {
                    var field = settingsObj.GetType().GetField("ExcludedAsmdefs");
                    if (field != null)
                    {
                        var value = field.GetValue(settingsObj) as System.Collections.IEnumerable;
                        if (value != null)
                        {
                            foreach (var v in value)
                            {
                                if (v != null)
                                    excluded.Add(v.ToString());
                            }
                        }
                    }
                }
            }

            // Если не нашли настроек или список пуст — используем разумный fallback
            if (excluded.Count == 0)
            {
                excluded.Add("ModularEventArchitecture");
                excluded.Add("UniRx");
            }

            var asmNames = asmNamesList.Where(n => !excluded.Contains(n)).ToArray();

            if (asmNames.Length > 0)
            {
                // Текущий индекс по значению property.stringValue
                int currentIndex = System.Array.IndexOf(asmNames, property.stringValue);
                if (currentIndex == -1) currentIndex = 0;

                int newIndex = EditorGUI.Popup(position, label.text, currentIndex, asmNames);

                if (newIndex != currentIndex)
                {
                    property.stringValue = asmNames[newIndex];
                    property.serializedObject.ApplyModifiedProperties();
                }
            }
            else
            {
                // Если asmdef не найдены — обычное текстовое поле
                EditorGUI.PropertyField(position, property, label);
            }

            EditorGUI.EndProperty();
        }
    }
}
#endif
// ...existing code...