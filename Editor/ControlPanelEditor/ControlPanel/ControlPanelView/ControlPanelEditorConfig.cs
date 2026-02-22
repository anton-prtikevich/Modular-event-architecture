using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(fileName = "ControlPanelEditorConfig", menuName = "Control Panel Editor Config")]
public class ControlPanelEditorConfig : ScriptableObject
{
    [Header("UXML Templates")]
    public VisualTreeAsset designMainScreen;
    public VisualTreeAsset BoardButtonTemplate;

    [Header("Style Sheets")]
    public StyleSheet behaviourTreeEditorStyle;


    private static ControlPanelEditorConfig _instance;

    public static ControlPanelEditorConfig Instance
    {
        get
        {
            if (_instance == null)
            {
                string[] guids = AssetDatabase.FindAssets("t:ControlPanelEditorConfig");
                if (guids.Length > 0)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                    _instance = AssetDatabase.LoadAssetAtPath<ControlPanelEditorConfig>(path);
                }

                if (_instance == null)
                {
                    Debug.LogError(
                        "ControlPanelEditorConfig не найден! Создайте его через меню: " +
                        "Assets > Create > Editor > Control Panel Editor Config");
                }
            }
            return _instance;
        }
    }
}
