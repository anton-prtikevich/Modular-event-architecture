using System.Collections.Generic;
using UnityEditor;
using System.IO;

namespace ModularEventArchitecture.Editor.ProjectMap.UseCases
{
    public class CreateUseCaseScriptUseCase
    {
        public void Execute(string featureFolder, string useCaseName)
        {
            if (string.IsNullOrWhiteSpace(useCaseName) || string.IsNullOrEmpty(featureFolder))
                return;

            string useCasesPath = Path.Combine(featureFolder, "UseCases").Replace("\\", "/");
            if (!Directory.Exists(useCasesPath))
            {
                var folderService = new Services.FeatureFolderService();
                folderService.CreateFeatureFolders(featureFolder, new List<string> { "UseCases" });
            }

            string scriptPath = Path.Combine(useCasesPath, useCaseName + ".cs").Replace("\\", "/");
            if (!File.Exists(scriptPath))
            {
                string scriptContent = $"using UnityEngine;\nusing ModularEventArchitecture;\n\npublic class {useCaseName} \n{{\n    \n}}\n";
                File.WriteAllText(scriptPath, scriptContent);
                AssetDatabase.ImportAsset(scriptPath);
            }
            AssetDatabase.Refresh();
        }
    }
}
