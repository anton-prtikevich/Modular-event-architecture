using UnityEditor;
using UnityEngine;
using ModularEventArchitecture.Editor.ProjectMap.Services;

namespace ModularEventArchitecture.Editor.ProjectMap.Map
{
    public class CreateFeatureWindow : EditorWindow
    {
        //ы ScriptableObject с настройками генерации
        [SerializeField] private FeatureGeneratorSettings settings;
        private string _featureName = "";
        private DefaultAsset selectedFolder;

        public static void ShowWindow()
        {
            var window = GetWindow<CreateFeatureWindow>(true, "Создать фичу", true);
            window.position = new Rect(Screen.width / 2, Screen.height / 2, 370, 150);
            window.ShowUtility();
        }

        private void OnGUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Настройки:", GUILayout.Width(70));
            settings = (FeatureGeneratorSettings)EditorGUILayout.ObjectField(settings, typeof(FeatureGeneratorSettings), false, GUILayout.Width(250));
            GUILayout.EndHorizontal();
            GUILayout.Label("Название фичи:", EditorStyles.boldLabel);
            _featureName = EditorGUILayout.TextField(_featureName);
            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Папка:", GUILayout.Width(40));
            selectedFolder = (DefaultAsset)EditorGUILayout.ObjectField(selectedFolder, typeof(DefaultAsset), false, GUILayout.Width(200));

            if (GUILayout.Button("Найти Features", GUILayout.Width(110)))
            {
                FindScriptsFolderService _findScriptsFolderUseCase = new FindScriptsFolderService();
                selectedFolder = _findScriptsFolderUseCase.FindFeaturesFolder();
                if (selectedFolder == null)
                {
                    //создать папку Features в Assets
                    string featuresPath = "Assets/Features";
                    if (!AssetDatabase.IsValidFolder(featuresPath))
                    {
                        AssetDatabase.CreateFolder("Assets", "Features");
                        selectedFolder = _findScriptsFolderUseCase.FindFeaturesFolder();
                    }
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Создать", GUILayout.Width(120)))
            {
                if (!string.IsNullOrWhiteSpace(_featureName))
                {
                    CreateFeature(_featureName);
                    Close();
                }
            }
            if (GUILayout.Button("Отмена", GUILayout.Width(120)))
            {
                Close();
            }
            GUILayout.EndHorizontal();
        }

        private void CreateFeature(string featureName)
        {
            FeatureFolderService _featureFolderService = new FeatureFolderService();
            string basePath = "Assets";
            if (selectedFolder != null)
            {
                basePath = AssetDatabase.GetAssetPath(selectedFolder);
            }
            string featurePath = $"{basePath}/{featureName}";
            if (!AssetDatabase.IsValidFolder(featurePath))
            {
                AssetDatabase.CreateFolder(basePath, featureName);
            }

            // Создаём подпапки через сервис
            _featureFolderService.CreateFeatureFolders(featurePath, new System.Collections.Generic.List<string> { "Domain", "Modules", "UseCases" });

            // Создаём .asmdef файл
            string asmdefPath = featurePath + "/" + featureName + ".asmdef";
            if (!System.IO.File.Exists(asmdefPath))
            {
                // Получаем зависимости из настроек или по умолчанию
                var refs = settings != null && settings.DefaultAsmdefReferences != null && settings.DefaultAsmdefReferences.Count > 0
                    ? settings.DefaultAsmdefReferences
                    : new System.Collections.Generic.List<string> { "ModularEventArchitecture.Editor" };
                string refsJson = string.Join(",\n    ", refs.ConvertAll(r => "\"" + r + "\""));
                string asmdefJson = $"{{\n  \"name\": \"{featureName}\",\n  \"references\": [\n    {refsJson}\n  ]\n}}";
                System.IO.File.WriteAllText(asmdefPath, asmdefJson);
                AssetDatabase.ImportAsset(asmdefPath);
            }

            // Создаём файл документации
            string docPath = featurePath + "/README.md";
            if (!System.IO.File.Exists(docPath))
            {
                System.IO.File.WriteAllText(docPath, $"Документация для {featureName}\n\n...");
                AssetDatabase.ImportAsset(docPath);
            }
                    

            AssetDatabase.Refresh();
        }
    }
}
