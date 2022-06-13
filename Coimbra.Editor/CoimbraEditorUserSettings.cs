﻿using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace Coimbra.Editor
{
    /// <summary>
    /// General <see cref="ScriptableSettingsType.EditorUserPreferences"/> settings.
    /// </summary>
    [InitializeOnLoad]
    [Preferences(CoimbraUtility.UserPreferencesPath, "Editor", true)]
    public sealed class CoimbraEditorUserSettings : ScriptableSettings
    {
#if UNITY_2021_3_OR_NEWER
        [Obsolete("Unity has now a built-in functionality for it in its Console window. This property will have no effect.")]
#endif
        [PublicAPI]
        [field: SerializeField]
        public bool ClearConsoleOnReload { get; set; }

        static CoimbraEditorUserSettings()
        {
#if !UNITY_2021_3_OR_NEWER
            AssemblyReloadEvents.beforeAssemblyReload -= HandleBeforeAssemblyReload;
            AssemblyReloadEvents.beforeAssemblyReload += HandleBeforeAssemblyReload;
#endif
        }

#if !UNITY_2021_3_OR_NEWER
        private static void HandleBeforeAssemblyReload()
        {
            ScriptableSettingsUtility.TryLoadOrCreate(out CoimbraEditorUserSettings settings, FindSingle);
            Debug.Assert(settings);

            if (settings.ClearConsoleOnReload)
            {
                CoimbraEditorUtility.ClearConsoleWindow();
            }
        }
#endif
    }
}