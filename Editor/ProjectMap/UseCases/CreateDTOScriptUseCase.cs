using System.Collections.Generic;
using UnityEditor;
using System.IO;

namespace ModularEventArchitecture.Editor.ProjectMap.UseCases
{
    public class CreateDTOScriptUseCase
    {
        public void Execute(string featureFolder, string dtoName)
        {
            if (string.IsNullOrWhiteSpace(dtoName) || string.IsNullOrEmpty(featureFolder))
                return;

            string domainPath = Path.Combine(featureFolder, "Domain").Replace("\\", "/");
            if (!Directory.Exists(domainPath))
            {
                var folderService = new Services.FeatureFolderService();
                folderService.CreateFeatureFolders(featureFolder, new List<string> { "Domain" });
            }

            string scriptPath = Path.Combine(domainPath, dtoName + ".cs").Replace("\\", "/");
            if (!File.Exists(scriptPath))
            {
                string scriptContent = $"// DTO: {dtoName}\npublic class {dtoName}\n{{\n    \n}}\n";
                File.WriteAllText(scriptPath, scriptContent);
                AssetDatabase.ImportAsset(scriptPath);
            }
            AssetDatabase.Refresh();
        }
    }
}
