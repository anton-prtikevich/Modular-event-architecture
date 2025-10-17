using UnityEditor;

namespace ModularEventArchitecture.Editor.ProjectMap.Map
{
    public class FindScriptsFolderService
    {
        public DefaultAsset FindScriptsFolder()
        {
            string[] folders = System.IO.Directory.GetDirectories("Assets", "*Scripts*", System.IO.SearchOption.AllDirectories);
            if (folders.Length > 0)
            {
                string scriptsFolder = folders[0].Replace("\\", "/");
                var asset = AssetDatabase.LoadAssetAtPath<DefaultAsset>(scriptsFolder);
                if (asset != null)
                {
                    return asset;
                }
            }
            return null;
        }
        public DefaultAsset FindFeaturesFolder()
        {
            string[] folders = System.IO.Directory.GetDirectories("Assets", "Features", System.IO.SearchOption.AllDirectories);
            if (folders.Length > 0)
            {
                string featureFolder = folders[0].Replace("\\", "/");
                var asset = AssetDatabase.LoadAssetAtPath<DefaultAsset>(featureFolder);
                if (asset != null)
                {
                    return asset;
                }
            }
            return null;
        }
    }
}
