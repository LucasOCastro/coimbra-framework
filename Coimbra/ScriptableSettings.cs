﻿using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Coimbra
{
    /// <summary>
    /// Class that allows easy access to a <see cref="ScriptableObject"/>.
    /// <para>Inheriting from this class is not required but is recommended as it automates the process of adding the object into Preloaded Assets and (un)registering it in the shared lockup table.</para>
    /// </summary>
    public abstract class ScriptableSettings : ScriptableObject
    {
        private static readonly Dictionary<Type, ScriptableObject> Values = new Dictionary<Type, ScriptableObject>();

        /// <inheritdoc cref="Get"/>
        public static T Get<T>()
            where T : ScriptableObject
        {
            return Get(typeof(T)) as T;
        }

        /// <inheritdoc cref="GetOrFind"/>
        public static T GetOrFind<T>()
            where T : ScriptableObject
        {
            return GetOrFind(typeof(T)) as T;
        }

        /// <inheritdoc cref="Has"/>
        public static bool Has<T>()
        {
            return Has(typeof(T));
        }

        /// <inheritdoc cref="Set"/>
        public static void Set<T>(T value, bool ignoreWarning = false)
            where T : ScriptableObject
        {
            Set(typeof(T), value, ignoreWarning);
        }

        /// <inheritdoc cref="TryGet"/>
        public static bool TryGet<T>(out T result)
            where T : ScriptableObject
        {
            result = Get(typeof(T)) as T;

            return result != null;
        }

        /// <inheritdoc cref="TryGetOrFind"/>
        public static bool TryGetOrFind<T>(out T result)
            where T : ScriptableObject
        {
            result = GetOrFind(typeof(T)) as T;

            return result != null;
        }

        /// <summary>
        /// Gets the last set value for the specified type.
        /// </summary>
        /// <param name="type">The type of the settings.</param>
        /// <returns>The settings if set and still valid.</returns>
        protected static ScriptableObject Get(Type type)
        {
            Debug.Assert(typeof(ScriptableObject).IsAssignableFrom(type));

            return Values.TryGetValue(type, out ScriptableObject value) && value.IsValid() ? value : null;
        }

        /// <summary>
        /// Gets the last set value for the specified type, but also tries to find one through <see cref="Resources.FindObjectsOfTypeAll"/> if none is found.
        /// </summary>
        /// <param name="type">The type of the settings.</param>
        /// <returns>The settings if set and still valid or if a new one could be found.</returns>
        protected static ScriptableObject GetOrFind(Type type)
        {
            Debug.Assert(typeof(ScriptableObject).IsAssignableFrom(type));

            if (Values.TryGetValue(type, out ScriptableObject value) && value.IsValid())
            {
                return value;
            }

            Object[] rawValues = Resources.FindObjectsOfTypeAll(type);

            if (rawValues.Length == 0)
            {
                return null;
            }

            if (rawValues.Length > 1)
            {
                Debug.LogWarning($"It was expected a single loaded object of type {type}, but it was found {rawValues.Length}!");
            }

            ScriptableObject result = (ScriptableObject)rawValues[0];
            Values[type] = result;

            return result;
        }

        /// <summary>
        /// Checks if the value for the specified type has been set and is still valid.
        /// </summary>
        /// <param name="type">The type of the settings.</param>
        /// <returns>True if the settings is set and still valid.</returns>
        protected static bool Has(Type type)
        {
            Debug.Assert(typeof(ScriptableObject).IsAssignableFrom(type));

            return Values.TryGetValue(type, out ScriptableObject value) && value.IsValid();
        }

        /// <summary>
        /// Sets the value for the specified type. By default, it also logs a warning if trying to override a valid value.
        /// </summary>
        /// <param name="type">The type of the settings.</param>
        /// <param name="value">The new value for the specified type.</param>
        /// <param name="ignoreWarning">If true, no warning wil be logged if trying to override a valid value.</param>
        protected static void Set(Type type, ScriptableObject value, bool ignoreWarning = false)
        {
            Debug.Assert(typeof(ScriptableObject).IsAssignableFrom(type));

            value = value.GetValid();

            if (!ignoreWarning && TryGet(type, out ScriptableObject currentValue) && value != currentValue)
            {
                Debug.LogWarning($"Overriding value of {type} in {nameof(ScriptableSettings)}! Changing from {currentValue} to {value}.");
            }

            if (value != null)
            {
                Values[type] = value;
            }
            else
            {
                Values.Remove(type);
            }
        }

        /// <summary>
        /// Tries to get the last set value for the specified type.
        /// </summary>
        /// <param name="type">The type of the settings.</param>
        /// <param name="result">The settings if set and still valid.</param>
        /// <returns>True if the settings is set and still valid.</returns>
        protected static bool TryGet(Type type, out ScriptableObject result)
        {
            Debug.Assert(typeof(ScriptableObject).IsAssignableFrom(type));

            result = Get(type);

            return result != null;
        }

        /// <summary>
        /// Tries to get the last set value for the specified type, but also tries to find one through <see cref="Resources.FindObjectsOfTypeAll"/> if none is found.
        /// </summary>
        /// <param name="type">The type of the settings.</param>
        /// <param name="result">The settings if set and still valid or if a new one could be found.</param>
        /// <returns>The settings if set and still valid or if a new one could be found.</returns>
        protected static bool TryGetOrFind(Type type, out ScriptableObject result)
        {
            Debug.Assert(typeof(ScriptableObject).IsAssignableFrom(type));

            result = GetOrFind(type);

            return result != null;
        }

        protected virtual void Awake()
        {
#if UNITY_EDITOR
            using Disposable<List<Object>> pooledList = ManagedPool<List<Object>>.Shared.GetDisposable();
            pooledList.Value.Clear();
            pooledList.Value.AddRange(UnityEditor.PlayerSettings.GetPreloadedAssets());

            if (pooledList.Value.Contains(this))
            {
                pooledList.Value.Clear();

                return;
            }

            pooledList.Value.Add(this);
            UnityEditor.PlayerSettings.SetPreloadedAssets(pooledList.Value.ToArray());
            pooledList.Value.Clear();
#endif
        }

        protected virtual void OnEnable()
        {
            Type type = GetType();

            if (TryGet(type, out ScriptableObject current))
            {
                if (current != this)
                {
                    Debug.LogWarning($"Skipping setting settings {type} from {current} to {this}!");
                }
            }
            else
            {
                Set(type, this);
            }
        }

        protected virtual void OnDisable()
        {
            Type type = GetType();

            if (TryGet(type, out ScriptableObject current) && current == this)
            {
                Set(type, null);
            }
        }

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
        [UnityEditor.InitializeOnEnterPlayMode]
        private static void Initialize()
        {
            Values.Clear();
            UnityEditor.PlayerSettings.GetPreloadedAssets();
        }
#endif
    }
}