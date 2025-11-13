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
        public List<string> defaultAsmdefReferences = new List<string> { "ModularEventArchitecture" };
    }
}
