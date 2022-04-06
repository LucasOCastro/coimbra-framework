﻿using UnityEngine;

namespace Coimbra
{
    /// <summary>
    /// Default implementation for <see cref="ICoroutineService"/>.
    /// </summary>
    [AddComponentMenu("")]
    public sealed class CoroutineSystem : ServiceActorBase<ICoroutineService>, ICoroutineService
    {
        private CoroutineSystem() { }

        /// <summary>
        /// Create a new <see cref="ICoroutineService"/>.
        /// </summary>
        public static ICoroutineService Create()
        {
            return new GameObject(nameof(CoroutineSystem)).AddComponent<CoroutineSystem>();
        }

        /// <inheritdoc/>
        protected override void OnObjectDespawn()
        {
            StopAllCoroutines();
            base.OnObjectDespawn();
        }

        /// <inheritdoc/>
        protected override void OnObjectInitialize()
        {
            DontDestroyOnLoad(CachedGameObject);
            base.OnObjectInitialize();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void HandleSubsystemRegistration()
        {
            ServiceLocator.Shared.SetCreateCallback(Create, false);
        }
    }
}
