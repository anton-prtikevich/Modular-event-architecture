using System.Collections.Generic;
using UnityEngine;

namespace ModularEventArchitecture.Editor.ProjectMap
{
    /// <summary>
    /// Настройки генератора фич для проектной карты.
    /// Используется для хранения настроек, таких как ссылки на asmdef по умолчанию.
    /// </summary>
    [CreateAssetMenu(fileName = "FeatureGeneratorSettings", menuName = "ModularEventArchitecture/Feature Generator Settings")]
    public class FeatureGeneratorSettings : ScriptableObject
    {
        public List<string> DefaultAsmdefReferences = new List<string> { "ModularEventArchitecture" };
        public List<string> ExcludedAsmdefs = new List<string>();
    }
}
