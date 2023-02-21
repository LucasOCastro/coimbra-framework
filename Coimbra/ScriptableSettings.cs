﻿#nullable enable

using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Scripting;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace Coimbra
{
    /// <summary>
    /// Class that allows easy access to a <see cref="ScriptableObject"/>.
    /// </summary>
    /// <remarks>
    /// This class is meant for globally-accessible read-only data containers. Those data containers can have different level os scopes:
    /// <list type="bullet">
    /// <item><see cref="ScriptableSettingsType.Custom"/>: those are dynamic objects generally either created at runtime or that the current instance changes at different stages of the application (i.e. a different asset according to selected difficulty).</item>
    /// <item><see cref="ScriptableSettingsType.EditorProjectSettings"/>: configured in the <b>Project Settings</b> window and saved at the <b>ProjectSettings</b> folder. Those settings are shared between all users of the current project and are never available outside the editor.</item>
    /// <item><see cref="ScriptableSettingsType.RuntimeProjectSettings"/>: configured in the <b>Project Settings</b> window and saved in the <b>Assets</b> folder (in a sub-folder of your choice). Those settings have the option to be included with the Preloaded Assets to be available since the application startup in the builds.</item>
    /// <item><see cref="ScriptableSettingsType.EditorUserPreferences"/>: configured in the <b>Preferences</b> window and saved using <b>EditorPrefs</b>. Those settings are local for the user machine, meaning that they are shader across all projects using the same Unity editor, and are never available outside the editor.</item>
    /// <item><see cref="ScriptableSettingsType.ProjectUserPreferences"/>: configured in the <b>Preferences</b> window and saved at the <b>UserSettings</b> folder. Those settings are local for the project, meaning that they won't affect other projects or other users, and are never available outside the editor.</item>
    /// </list>
    /// You can check all currently loaded <see cref="ScriptableSettings"/> by going to <b>Window/Coimbra Framework/Scriptable Settings</b>.
    /// </remarks>
    /// <seealso cref="PreferencesAttribute"/>
    /// <seealso cref="ProjectSettingsAttribute"/>
    /// <seealso cref="ScriptableSettingsType"/>
    /// <seealso cref="ScriptableSettingsTypeUtility"/>
    [RequireDerived]
    public abstract class ScriptableSettings : ScriptableObject
    {
#pragma warning disable CS0618
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete(nameof(ScriptableSettings) + "." + nameof(FindHandler) + " shouldn't be used anymore.")]
        public delegate ScriptableSettings FindHandler(Type type);

        // ReSharper disable once UnassignedReadonlyField
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete(nameof(ScriptableSettings) + "." + nameof(FindSingle) + " shouldn't be used anymore.")]
        public static readonly FindHandler? FindSingle;
#pragma warning restore CS0618

        internal static readonly Dictionary<Type, ScriptableSettings?> Instances = new();

        internal static readonly string[] FindAssetsFolders = new string[]
        {
            "Assets",
        };

        private static readonly Object?[] SaveTarget = new Object?[1];

        private bool _isCreating;

        [SerializeField]
        [FormerlySerializedAsBackingFieldOf("Preload")]
        [Tooltip("Should this setting be included in the preloaded assets?")]
        private bool _preload;

        [SerializeField]
        [HideInInspector]
        [FormerlySerializedAsBackingFieldOf("Type")]
        private ScriptableSettingsType _type;

        static ScriptableSettings()
        {
            Application.quitting -= HandleApplicationQuitting;
            Application.quitting += HandleApplicationQuitting;
        }

        protected ScriptableSettings()
        {
            _type = GetTypeData(GetType());
            _preload = !_type.IsEditorOnly();
        }

        /// <summary>
        /// Gets or sets a value indicating whether this setting should be included in the preloaded assets.
        /// </summary>
        [PublicAPI]
        public bool Preload
        {
            [DebuggerStepThrough]
            get => _preload;
            protected set
            {
                _preload = value;
                ValidatePreload(true);
            }
        }

        /// <summary>
        /// Gets the type of <see cref="ScriptableSettings"/> based on the presence of either <see cref="PreferencesAttribute"/> or <see cref="ProjectSettingsAttribute"/>.
        /// </summary>
        public ScriptableSettingsType Type => _type;

        /// <summary>
        /// Gets a value indicating whether the application is quitting.
        /// </summary>
        protected internal static bool IsQuitting { get; internal set; }

        /// <summary>
        /// Gets the last set value for the specified type, or a default created one if none is set.
        /// </summary>
        /// <param name="type">The type of the settings.</param>
        /// <returns>The settings if set and still valid or if a new one could be found.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ScriptableSettings Get(Type type)
        {
            Debug.Assert(typeof(ScriptableSettings).IsAssignableFrom(type));

            return GetOrCreate(type);
        }

        /// <inheritdoc cref="Get"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Get<T>()
            where T : ScriptableSettings
        {
            return (T)GetOrCreate(typeof(T));
        }

#pragma warning disable CS0618
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Obsolete(nameof(ScriptableSettings) + "." + nameof(GetOrFind) + " shouldn't be used anymore, use either " + nameof(Get) + " or " + nameof(IsSet) + " instead.")]
        public static ScriptableSettings GetOrFind(Type type, FindHandler? findHandler = null)
        {
            return Get(type);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Obsolete(nameof(ScriptableSettings) + "." + nameof(GetOrFind) + " shouldn't be used anymore, use either " + nameof(Get) + " or " + nameof(IsSet) + " instead.")]
        public static T GetOrFind<T>(FindHandler? findHandler = null)
            where T : ScriptableSettings
        {
            return Get<T>();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Obsolete(nameof(ScriptableSettings) + "." + nameof(GetType) + " shouldn't be used anymore, use " + nameof(GetTypeData) + " instead.")]
        public static ScriptableSettingsType GetType(Type type)
        {
            return GetTypeData(type);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Obsolete(nameof(ScriptableSettings) + "." + nameof(GetType) + " shouldn't be used anymore, use " + nameof(GetTypeData) + " instead.")]
        public static ScriptableSettingsType GetType<T>()
            where T : ScriptableSettings
        {
            return GetTypeData(typeof(T));
        }
#pragma warning restore CS0618

        /// <summary>
        /// Gets the <see cref="ScriptableSettingsType"/> for the <paramref name="type"/>.
        /// </summary>
        public static ScriptableSettingsType GetTypeData(Type type)
        {
            Debug.Assert(typeof(ScriptableSettings).IsAssignableFrom(type));

            ProjectSettingsAttribute projectSettingsAttribute = type.GetCustomAttribute<ProjectSettingsAttribute>();

            if (projectSettingsAttribute != null)
            {
                return projectSettingsAttribute.IsEditorOnly ? ScriptableSettingsType.EditorProjectSettings : ScriptableSettingsType.RuntimeProjectSettings;
            }

            PreferencesAttribute preferencesAttribute = type.GetCustomAttribute<PreferencesAttribute>();

            if (preferencesAttribute != null)
            {
                return preferencesAttribute.UseEditorPrefs ? ScriptableSettingsType.EditorUserPreferences : ScriptableSettingsType.ProjectUserPreferences;
            }

            return ScriptableSettingsType.Custom;
        }

        /// <summary>
        /// Gets the <see cref="ProjectSettingsAttribute"/> or <see cref="PreferencesAttribute"/> data for a <see cref="ScriptableSettings"/> type.
        /// </summary>
        /// <returns>The <see cref="ScriptableSettingsType"/> for the specified <paramref name="type"/>.</returns>
        public static ScriptableSettingsType GetTypeData(Type type, out string? windowPath, out string? filePath, out string[]? keywords)
        {
            Debug.Assert(typeof(ScriptableSettings).IsAssignableFrom(type));

            windowPath = null;
            filePath = null;
            keywords = null;

            ProjectSettingsAttribute projectSettingsAttribute = type.GetCustomAttribute<ProjectSettingsAttribute>();

            if (projectSettingsAttribute != null)
            {
                windowPath = $"{projectSettingsAttribute.WindowPath}/{projectSettingsAttribute.NameOverride ?? ApplicationUtility.GetDisplayName(type.Name)}";
                filePath = projectSettingsAttribute.IsEditorOnly ? $"{projectSettingsAttribute.FileDirectory}/{projectSettingsAttribute.FileNameOverride ?? $"{type.Name}.asset"}" : null;
                keywords = projectSettingsAttribute.Keywords;

                return projectSettingsAttribute.IsEditorOnly ? ScriptableSettingsType.EditorProjectSettings : ScriptableSettingsType.RuntimeProjectSettings;
            }

            PreferencesAttribute preferencesAttribute = type.GetCustomAttribute<PreferencesAttribute>();

            if (preferencesAttribute != null)
            {
                windowPath = preferencesAttribute.WindowPath != null ? $"{preferencesAttribute.WindowPath}/{preferencesAttribute.NameOverride ?? ApplicationUtility.GetDisplayName(type.Name)}" : null;
                filePath = preferencesAttribute.UseEditorPrefs ? null : $"{preferencesAttribute.FileDirectory}/{preferencesAttribute.FileNameOverride ?? $"{type.Name}.asset"}";
                keywords = preferencesAttribute.Keywords;

                return preferencesAttribute.UseEditorPrefs ? ScriptableSettingsType.EditorUserPreferences : ScriptableSettingsType.ProjectUserPreferences;
            }

            return ScriptableSettingsType.Custom;
        }

        /// <summary>
        /// Checks if the value is set for the specified type.
        /// </summary>
        /// <param name="type">The type of the settings.</param>
        /// <param name="result">The settings if set and still valid.</param>
        /// <returns>True if the settings is set and still valid.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsSet(Type type, [NotNullWhen(true)] out ScriptableSettings? result)
        {
            if (Instances.TryGetValue(type, out result))
            {
                if (result != null)
                {
                    return true;
                }

                Instances.Remove(type);
            }

            result = null;

            return false;
        }

        /// <inheritdoc cref="IsSet"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsSet<T>([NotNullWhen(true)] out T? result)
            where T : ScriptableSettings
        {
            if (IsSet(typeof(T), out ScriptableSettings? rawResult))
            {
                result = (T)rawResult;

                return true;
            }

            result = null;

            return false;
        }

        /// <summary>
        /// Sets the value for the specified type, if not set yet.
        /// </summary>
        /// <remarks>
        /// If another value is already set then it will emit a warning and don't do anything.
        /// <para></para>
        /// If overwriting is intended use <see cref="SetOrOverwrite"/>.
        /// </remarks>
        /// <param name="type">The type of the settings.</param>
        /// <param name="value">The new value for the specified type.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Set(Type type, ScriptableSettings? value)
        {
            Debug.Assert(typeof(ScriptableSettings).IsAssignableFrom(type));
            Set(false, type, value);
        }

        /// <inheritdoc cref="Set"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Set<T>(T value)
            where T : ScriptableSettings
        {
            Set(false, typeof(T), value);
        }

        /// <summary>
        /// Sets the value for the specified type, even if it was already set.
        /// </summary>
        /// <remarks>
        /// It will set the new value even if another value is already set.
        /// <para></para>
        /// If overwriting is intended use <see cref="SetOrOverwrite"/>.
        /// </remarks>
        /// <param name="type">The type of the settings.</param>
        /// <param name="value">The new value for the specified type.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetOrOverwrite(Type type, ScriptableSettings? value)
        {
            Debug.Assert(typeof(ScriptableSettings).IsAssignableFrom(type));
            Set(true, type, value);
        }

        /// <inheritdoc cref="SetOrOverwrite"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetOrOverwrite<T>(T? value)
            where T : ScriptableSettings
        {
            Set(true, typeof(T), value);
        }

#pragma warning disable CS0618
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Obsolete(nameof(ScriptableSettings) + "." + nameof(TryGetOrFind) + " shouldn't be used anymore, use either " + nameof(Get) + " or " + nameof(IsSet) + " instead.")]
        public static bool TryGetOrFind(Type type, out ScriptableSettings value, FindHandler? findHandler = null)
        {
            value = Get(type);

            return true;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Obsolete(nameof(ScriptableSettings) + "." + nameof(TryGetOrFind) + " shouldn't be used anymore, use either " + nameof(Get) + " or " + nameof(IsSet) + " instead.")]
        public static bool TryGetOrFind<T>([NotNullWhen(true)] out T value, FindHandler? findHandler = null)
            where T : ScriptableSettings
        {
            value = Get<T>();

            return true;
        }
#pragma warning restore CS0618

        /// <summary>
        /// Compare if this instance content equals to another instance.
        /// <para></para>
        /// By default it uses <see cref="JsonUtility.ToJson(object, bool)"/> with pretty print disabled to compare the instances, but can be overriden to have a more performant comparison according to the project needs.
        /// </summary>
        /// <param name="other">The instance to compare with.</param>
        /// <returns>True if their content are equal.</returns>
        public virtual bool CompareContent(ScriptableSettings other)
        {
            return JsonUtility.ToJson(this, false) == JsonUtility.ToJson(other, false);
        }

        /// <summary>
        /// Loads this instance from the supported storage method, if any.
        /// </summary>
        public void Load()
        {
            ScriptableSettingsType type = _type;

            switch (type)
            {
                case ScriptableSettingsType.Custom:
                {
                    OnLoad();

                    break;
                }
#if UNITY_EDITOR
                case ScriptableSettingsType.RuntimeProjectSettings:
                {
                    string path = UnityEditor.AssetDatabase.GetAssetPath(this);

                    if (!string.IsNullOrWhiteSpace(path) && UnityEditor.AssetDatabase.LoadAssetAtPath(path, GetType()) is { } o && GetType().IsInstanceOfType(o))
                    {
                        string? data = UnityEditor.EditorJsonUtility.ToJson(o, false);
                        UnityEditor.EditorJsonUtility.FromJsonOverwrite(data, this);
                        _type = GetTypeData(GetType());
                    }

                    break;
                }

                case ScriptableSettingsType.EditorProjectSettings:
                case ScriptableSettingsType.ProjectUserPreferences:
                {
                    GetTypeData(GetType(), out _, out string? filePath, out _);

                    Object[] objects = UnityEditorInternal.InternalEditorUtility.LoadSerializedFileAndForget(filePath);
                    string? data = null;

                    foreach (Object o in objects)
                    {
                        if (data == null && GetType().IsInstanceOfType(o))
                        {
                            data = UnityEditor.EditorJsonUtility.ToJson(o, false);
                            UnityEditor.EditorJsonUtility.FromJsonOverwrite(data, this);
                            _type = GetTypeData(GetType());
                        }

                        DestroyImmediate(o);
                    }

                    break;
                }

                case ScriptableSettingsType.EditorUserPreferences:
                {
                    string key = ApplicationUtility.GetPrefsKey(GetType());

                    if (UnityEditor.EditorPrefs.HasKey(key))
                    {
                        string data = UnityEditor.EditorPrefs.GetString(key);
                        UnityEditor.EditorJsonUtility.FromJsonOverwrite(data, this);
                        _type = GetTypeData(GetType());
                    }

                    break;
                }
#endif
            }

            OnLoaded();
        }

        /// <summary>
        /// Saves this instance to the supported storage method, if any.
        /// </summary>
        /// <seealso cref="OnSave"/>
        public void Save()
        {
            switch (_type)
            {
                case ScriptableSettingsType.Custom:
                {
                    OnSave();

                    return;
                }
#if UNITY_EDITOR
                default:
                {
                    GetTypeData(GetType(), out _, out string? filePath, out _);

                    if (filePath == null)
                    {
                        if (_type.IsProjectSettings())
                        {
                            UnityEditor.EditorUtility.SetDirty(this);
                            UnityEditor.AssetDatabase.SaveAssetIfDirty(this);

                            break;
                        }

                        string value = UnityEditor.EditorJsonUtility.ToJson(this, true);
                        UnityEditor.EditorPrefs.SetString(ApplicationUtility.GetPrefsKey(GetType()), value);

                        break;
                    }

                    if (!string.IsNullOrWhiteSpace(UnityEditor.AssetDatabase.GetAssetPath(this)))
                    {
                        if (_type.IsProjectSettings())
                        {
                            UnityEditor.EditorUtility.SetDirty(this);
                            UnityEditor.AssetDatabase.SaveAssetIfDirty(this);

                            break;
                        }
                    }

                    string? directoryName = Path.GetDirectoryName(filePath);

                    if (!string.IsNullOrWhiteSpace(directoryName) && !Directory.Exists(directoryName))
                    {
                        Directory.CreateDirectory(directoryName);
                    }

                    SaveTarget[0] = this;
                    UnityEditorInternal.InternalEditorUtility.SaveToSerializedFileAndForget(SaveTarget, filePath, true);

                    SaveTarget[0] = null;

                    break;
                }
#endif
            }
        }

        internal void ValidatePreload(bool logMissingPreloadedAssets)
        {
            switch (_type)
            {
                case ScriptableSettingsType.RuntimeProjectSettings:
                {
                    _preload = true;

                    break;
                }

                case ScriptableSettingsType.EditorProjectSettings:
                case ScriptableSettingsType.EditorUserPreferences:
                case ScriptableSettingsType.ProjectUserPreferences:
                {
                    _preload = false;

                    break;
                }
            }

#if UNITY_EDITOR
            GUI.changed = true;
            UnityEditor.EditorUtility.SetDirty(this);

            if (_preload)
            {
                using (ListPool.Pop(out List<Object> pooledList))
                {
                    pooledList.AddRange(UnityEditor.PlayerSettings.GetPreloadedAssets());

                    if (pooledList.Contains(this))
                    {
                        return;
                    }

                    if (logMissingPreloadedAssets)
                    {
                        Debug.LogWarning($"Fixing \"{this}\" not being added to the preloaded assets.", this);
                    }

                    pooledList.Add(this);
                    UnityEditor.PlayerSettings.SetPreloadedAssets(pooledList.ToArray());
                }
            }
            else
            {
                using (ListPool.Pop(out List<Object> pooledList))
                {
                    pooledList.AddRange(UnityEditor.PlayerSettings.GetPreloadedAssets());

                    if (!pooledList.Remove(this))
                    {
                        pooledList.Clear();

                        return;
                    }

                    while (pooledList.Remove(this))
                    {
                        // remove all occurrences
                    }

                    UnityEditor.PlayerSettings.SetPreloadedAssets(pooledList.ToArray());
                }
            }
#endif
        }

        /// <summary>
        /// Will get called only once for a given object after the <see cref="ScriptableObject.CreateInstance(System.Type)"/>.
        /// <para></para>
        /// This will not get for previously created assets that are only being loaded from the disk.
        /// <para></para>
        /// Can be called inside edit-mode when inside the editor.
        /// </summary>
        protected virtual void OnCreate() { }

        /// <summary>
        /// Will get called whenever <see cref="Load"/> is called.
        /// <para></para>
        /// Only gets called if instance is <see cref="ScriptableSettingsType.Custom"/>.
        /// </summary>
        protected virtual void OnLoad() { }

        /// <summary>
        /// Will get called right after <see cref="OnLoad"/> or <see cref="OnCreate"/>.
        /// <para></para>
        /// Can be called multiple times without going through <see cref="OnUnload"/>.
        /// <para></para>
        /// Can be called inside edit-mode when inside the editor.
        /// </summary>
        protected virtual void OnLoaded() { }

        /// <summary>
        /// Will get called whenever <see cref="Reset"/> is called.
        /// <para></para>
        /// Can be called inside edit-mode when inside the editor.
        /// </summary>
        protected virtual void OnReset() { }

        /// <summary>
        /// Will get called when <see cref="Save"/> is called.
        /// <para></para>
        /// Only gets called if instance is <see cref="ScriptableSettingsType.Custom"/>.
        /// </summary>
        protected virtual void OnSave() { }

        /// <summary>
        /// Will get called whenever the object is unloaded from the memory.
        /// <para></para>
        /// Can be called inside edit-mode when inside the editor.
        /// </summary>
        protected virtual void OnUnload() { }

        /// <summary>
        /// Use this instead of the standard <see cref="OnValidate"/> callback.
        /// </summary>
        protected virtual void OnValidating() { }

        /// <summary>
        /// Non-virtual by design, use <see cref="OnCreate"/> instead.
        /// </summary>
        protected void Awake()
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                name = GetType().Name;
            }

            _isCreating = true;
        }

        /// <summary>
        /// Non-virtual by design, use <see cref="OnUnload"/> instead.
        /// </summary>
        protected void OnDisable()
        {
            OnUnload();
        }

        /// <summary>
        /// Non-virtual by design, use <see cref="OnLoaded"/> instead.
        /// </summary>
        protected void OnEnable()
        {
            if (_isCreating)
            {
                OnCreate();
            }

            OnLoaded();
        }

        /// <summary>
        /// Non-virtual by design, use <see cref="OnValidating"/> instead.
        /// </summary>
        protected void OnValidate()
        {
            ValidatePreload(true);
            OnValidating();
        }

        /// <summary>
        /// Non-virtual by design, use <see cref="OnReset"/> instead.
        /// </summary>
        protected void Reset()
        {
            ValidatePreload(false);
            OnReset();
        }

        private static void HandleApplicationQuitting()
        {
            IsQuitting = true;
        }

        private static ScriptableSettings GetOrCreate(Type type)
        {
            if (Instances.TryGetValue(type, out ScriptableSettings? value))
            {
                if (value != null)
                {
                    return value;
                }
            }

            switch (GetTypeData(type, out _, out string? filePath, out _))
            {
                case ScriptableSettingsType.Custom:
                {
                    if (TryFindAsset(type, out value))
                    {
                        value.Load();
                    }
                    else
                    {
                        value = (ScriptableSettings)CreateInstance(type);
                    }

                    Instances[type] = value;

                    return value;
                }

#if UNITY_EDITOR
                default:
                {
                    if (TryFindAsset(type, out value) && filePath != null)
                    {
                        string assetPath = UnityEditor.AssetDatabase.GetAssetPath(value);

                        if (!string.IsNullOrWhiteSpace(assetPath))
                        {
                            ScriptableSettings copy = Instantiate(value);
                            Debug.LogWarning($"Moving {value} from {assetPath} to {filePath}!", copy);
                            DestroyImmediate(value, true);
                            UnityEditor.AssetDatabase.DeleteAsset(assetPath);
                            copy.Save();

                            value = copy;
                        }
                    }

                    break;
                }
#endif
            }

            if (value == null)
            {
                value = (ScriptableSettings)CreateInstance(type);
            }
            else
            {
                value.Load();
            }

            Instances[type] = value;

            return value;
        }

        private static void Set(bool forceSet, Type type, ScriptableSettings? value)
        {
            value = value.GetValid();

            if (Instances.TryGetValue(type, out ScriptableSettings? current))
            {
                if (current != null && current != value)
                {
                    if (forceSet)
                    {
                        if (!ApplicationUtility.IsReloadingScripts && !IsQuitting && !GetTypeData(type).IsEditorOnly())
                        {
                            Debug.Log($"Overriding {type} in {nameof(ScriptableSettings)} from \"{current}\"!", current);
                            Debug.Log($"Overriding {type} in {nameof(ScriptableSettings)} to \"{value}\"!", value);
                        }
                    }
                    else
                    {
                        if (GetTypeData(type).IsEditorOnly())
                        {
                            return;
                        }

                        Debug.LogWarning($"{type} in {nameof(ScriptableSettings)} is already set to \"{current}\"!", current);
                        Debug.LogWarning($"{type} in {nameof(ScriptableSettings)} can't be overriden to \"{value}\".", value);

                        return;
                    }
                }
            }

            if (value != null)
            {
                Instances[type] = value;
            }
            else
            {
                Instances.Remove(type);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool TryFindAsset(Type type, [NotNullWhen(true)] out ScriptableSettings? value)
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static void assertSingle(Type type, IReadOnlyList<Object> instances)
            {
                int count = instances.Count;

                for (int i = 1; i < count; i++)
                {
                    string path = UnityEditor.AssetDatabase.GetAssetPath(instances[0]);
                    Debug.LogError($"It was expected a single object of type {type}, but {instances[i]} is from the same type as {path}! Set the instance manually before trying to access it to ensure the correct one is used.", instances[i]);
                }
            }
#if UNITY_EDITOR
            string[] assets = UnityEditor.AssetDatabase.FindAssets($"t:{type.Name}", FindAssetsFolders);

            using (ListPool.Pop(out List<Object> list))
            {
                list.EnsureCapacity(assets.Length);

                foreach (string asset in assets)
                {
                    Object o = UnityEditor.AssetDatabase.LoadMainAssetAtPath(UnityEditor.AssetDatabase.GUIDToAssetPath(asset));

                    if (type.IsInstanceOfType(o))
                    {
                        list.Add(o);
                    }
                }

                if (list.Count > 0)
                {
                    value = (ScriptableSettings)list[0];
                    assertSingle(type, list);

                    return true;
                }
            }
#endif
            Object[] array = Resources.FindObjectsOfTypeAll(type);

            if (array.Length > 0)
            {
                value = (ScriptableSettings)array[0];
                assertSingle(type, array);

                return true;
            }

            value = null;

            return false;
        }
    }
}
