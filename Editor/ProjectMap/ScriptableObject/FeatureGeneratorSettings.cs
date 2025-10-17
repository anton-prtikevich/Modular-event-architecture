using System.Collections.Generic;
using UnityEngine;

namespace ModularEventArchitecture.Editor.ProjectMap
{
    [CreateAssetMenu(fileName = "FeatureGeneratorSettings", menuName = "ModularEventArchitecture/Feature Generator Settings")]
    public class FeatureGeneratorSettings : ScriptableObject
    {
        public List<string> defaultAsmdefReferences = new List<string> { "ModularEventArchitecture" };
    }
}
