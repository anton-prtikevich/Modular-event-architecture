using UnityEngine;
using UnityEditor;
using System.Reflection;

namespace ModularEventArchitecture
{
    [CustomEditor(typeof(ScriptableObject), true)]
    public class ButtonAttributeEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            // Отрисовываем стандартный инспектор
            DrawDefaultInspector();

            // Получаем целевой объект
            var targetObject = target;

            // Получаем все методы объекта
            var methods = targetObject.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var method in methods)
            {
                // Проверяем наличие атрибута Button
                var buttonAttribute = method.GetCustomAttribute<ButtonAttribute>();
                if (buttonAttribute != null)
                {
                    bool shouldEnable = true;

                    // Проверяем режим кнопки
                    switch (buttonAttribute.mode)
                    {
                        case ButtonMode.EnabledInPlayMode:
                            shouldEnable = Application.isPlaying;
                            break;
                        case ButtonMode.DisabledInPlayMode:
                            shouldEnable = !Application.isPlaying;
                            break;
                    }

                    // Используем имя метода если имя кнопки не задано
                    string buttonName = string.IsNullOrEmpty(buttonAttribute.buttonName) 
                        ? ObjectNames.NicifyVariableName(method.Name) 
                        : buttonAttribute.buttonName;

                    EditorGUI.BeginDisabledGroup(!shouldEnable);
                    if (GUILayout.Button(buttonName))
                    {
                        // Вызываем метод
                        method.Invoke(targetObject, null);
                        
                        // Помечаем объект как измененный
                        EditorUtility.SetDirty(targetObject);
                        // Сохраняем изменения
                        AssetDatabase.SaveAssets();
                    }
                    EditorGUI.EndDisabledGroup();
                }
            }
        }
    }
}
