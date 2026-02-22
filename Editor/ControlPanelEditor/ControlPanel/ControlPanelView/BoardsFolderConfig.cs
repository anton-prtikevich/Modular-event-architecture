using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "BoardsFolderConfig", menuName = "Control Panel/Boards Folder Config")]
public class BoardsFolderConfig : ScriptableObject
{
    [Tooltip("Папка для сохранения досок (BehavioureTree assets)")]
    public DefaultAsset boardsFolder;

    private static BoardsFolderConfig _instance;

    public static BoardsFolderConfig Instance
    {
        get
        {
            if (_instance == null)
            {
                string[] guids = AssetDatabase.FindAssets("t:BoardsFolderConfig");
                if (guids.Length > 0)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                    _instance = AssetDatabase.LoadAssetAtPath<BoardsFolderConfig>(path);
                }

                if (_instance == null)
                {
                    Debug.LogWarning("BoardsFolderConfig не найден. Создайте его через: Assets > Create > Control Panel > Boards Folder Config");
                }
            }
            return _instance;
        }
    }
}
