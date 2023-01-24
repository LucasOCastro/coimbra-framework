﻿using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Coimbra
{
    /// <summary>
    /// <see cref="Type"/> that can be viewed, modified and saved from the inspector.
    /// </summary>
    /// <typeparam name="T">Will require the type to be assignable to that.</typeparam>
    [Serializable]
    public struct SerializableType<T> : IEquatable<SerializableType<T>>, IEquatable<Type>, ISerializationCallbackReceiver
    {
        [SerializeField]
        private string _className;

        [SerializeField]
        private string _assemblyName;

        private Type _value;

        public SerializableType(Type type)
            : this()
        {
            Value = type;
        }

        /// <summary>
        /// Gets the assembly of the type.
        /// </summary>
        public string AssemblyName => _assemblyName;

        /// <summary>
        /// Gets the name of the type.
        /// </summary>
        public string ClassName => _className;

        /// <summary>
        /// Gets or sets the serialized type.
        /// </summary>
        public Type Value
        {
            get => _value ?? typeof(T);
            set
            {
                _value = value;

                if (_value != null && typeof(T).IsAssignableFrom(_value))
                {
                    _assemblyName = _value.Assembly.FullName;
                    _className = _value.FullName;
                }
                else
                {
                    _assemblyName = typeof(T).Assembly.FullName;
                    _className = typeof(T).FullName;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Type(SerializableType<T> type)
        {
            return type.Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator SerializableType<T>(Type type)
        {
            return new SerializableType<T>(type);
        }

        /// <inheritdoc/>
        public bool Equals(SerializableType<T> other)
        {
            return Value == other.Value;
        }

        /// <inheritdoc/>
        public bool Equals(Type other)
        {
            return Value == other;
        }

        public override string ToString()
        {
            return TypeString.Get(Value);
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize() { }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            try
            {
                if (string.IsNullOrEmpty(_assemblyName) || string.IsNullOrEmpty(_className))
                {
                    Value = null;
                }
                else
                {
                    Assembly assembly = Assembly.Load(_assemblyName);
                    Value = assembly != null ? assembly.GetType(_className) : null;
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}
