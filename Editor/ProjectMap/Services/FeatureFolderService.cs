using UnityEditor;
using System.Collections.Generic;

namespace ModularEventArchitecture.Editor.ProjectMap.Services
{
    public class FeatureFolderService
    {
        public void CreateFeatureFolders(string featurePath, List<string> folderNames)
        {
            foreach (var folder in folderNames)
            {
                string fullPath = featurePath + "/" + folder;
                if (!AssetDatabase.IsValidFolder(fullPath))
                {
                    AssetDatabase.CreateFolder(featurePath, folder);
                }
            }
        }
    }
}
