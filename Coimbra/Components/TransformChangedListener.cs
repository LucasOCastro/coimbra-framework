﻿using UnityEngine;

namespace Coimbra
{
    /// <summary>
    /// Listen to <see cref="Transform"/>'s changes.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerLoopListenerBase))]
    [AddComponentMenu(CoimbraUtility.GeneralMenuPath + "Transform Changed Listener")]
    [HelpURL("https://docs.unity3d.com/ScriptReference/Transform-hasChanged.html")]
    public sealed class TransformChangedListener : MonoBehaviour
    {
        public delegate void EventHandler(TransformChangedListener sender);

        /// <summary>
        /// Invoked inside the given <see cref="PlayerLoopListener"/>.
        /// </summary>
        public event EventHandler OnTrigger
        {
            add
            {
                _eventHandler += value;

                if (_eventHandler == null || _isActive)
                {
                    return;
                }

                _isActive = true;
                PlayerLoopListener.OnTrigger += HandlePlayerLoop;
            }
            remove
            {
                _eventHandler -= value;

                if (_eventHandler != null || !_isActive)
                {
                    return;
                }

                _isActive = false;
                PlayerLoopListener.OnTrigger -= HandlePlayerLoop;
            }
        }

        private EventHandler _eventHandler;

        private bool _isActive;

        private Actor _actor;

        private PlayerLoopListenerBase _playerLoopListener;

        /// <summary>
        /// The actor this component belongs to.
        /// </summary>
        public Actor Actor => _actor != null ? _actor : _actor = gameObject.AsActor();

        /// <summary>
        /// The player loop listener this component depends on.
        /// </summary>
        public PlayerLoopListenerBase PlayerLoopListener => _playerLoopListener != null ? _playerLoopListener : _playerLoopListener = GetComponent<PlayerLoopListenerBase>();

        private void HandlePlayerLoop(PlayerLoopListenerBase sender, float deltaTime)
        {
            if (!Actor.Transform.hasChanged)
            {
                return;
            }

            Actor.Transform.hasChanged = false;
            _eventHandler?.Invoke(this);
        }
    }
}