﻿using Coimbra.Editor;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Coimbra.Linting.Editor
{
    /// <summary>
    /// Define references that will be referenced automatically.
    /// </summary>
    [CreateAssetMenu(menuName = CoimbraUtility.GeneralMenuPath + DefaultAssetMenuPath + "Required References")]
    public sealed class RequiredReferencesAssemblyDefinitionRule : AssemblyDefinitionRuleBase
    {
        private const string GuidFormat = "GUID:{0}";

        [SerializeField]
        [Tooltip("The list os assembly definition names to require.")]
        private AssemblyDefinitionAsset[] _requiredReferences;

        /// <summary>
        /// The list of <see cref="AssemblyDefinitionAsset"/> references to require.
        /// </summary>
        public IReadOnlyList<AssemblyDefinitionAsset> RequiredReferences
        {
            get => _requiredReferences;
            set => _requiredReferences = value.ToArray();
        }

        /// <inheritdoc/>
        public override bool Apply(AssemblyDefinition assemblyDefinition)
        {
            using (ListPool.Pop(out List<string> list))
            using (HashSetPool.Pop(out HashSet<string> set))
            {
                list.AddRange(assemblyDefinition.References);
                set.UnionWith(assemblyDefinition.References);

                foreach (AssemblyDefinitionAsset asset in _requiredReferences)
                {
                    if (asset == null || !set.Add(asset.name))
                    {
                        continue;
                    }

                    string path = AssetDatabase.GetAssetPath(asset);
                    string guid = string.Format(GuidFormat, AssetDatabase.AssetPathToGUID(path));

                    if (!set.Add(guid))
                    {
                        continue;
                    }

                    list.Add(guid);
                    Debug.LogWarning($"{assemblyDefinition.Name} was missing required reference to {asset.name}!");
                }

                if (assemblyDefinition.References.Length == list.Count)
                {
                    return false;
                }

                assemblyDefinition.References = list.ToArray();

                return true;
            }
        }
    }
}
