using System.Collections.Generic;
using UnityEditor;
using System.IO;

namespace ModularEventArchitecture.Editor.ProjectMap.UseCases
{
    public class CreateModuleScriptUseCase
    {
        public void Execute(string featureFolder, string moduleName)
        {
            if (string.IsNullOrWhiteSpace(moduleName) || string.IsNullOrEmpty(featureFolder))
                return;

            string modulesPath = Path.Combine(featureFolder, "Modules").Replace("\\", "/");
            if (!Directory.Exists(modulesPath))
            {
                var folderService = new Services.FeatureFolderService();
                folderService.CreateFeatureFolders(featureFolder, new List<string> { "Modules" });
            }

            string scriptPath = Path.Combine(modulesPath, moduleName + ".cs").Replace("\\", "/");
            if (!File.Exists(scriptPath))
            {
                string scriptContent = $"using UnityEngine;\nusing ModularEventArchitecture;\n\npublic class {moduleName} : ModuleBase\n{{\n    public override void Initialize()\n    {{\n    }}\n}}\n";
                File.WriteAllText(scriptPath, scriptContent);
                AssetDatabase.ImportAsset(scriptPath);
            }
            AssetDatabase.Refresh();
        }
    }
}
