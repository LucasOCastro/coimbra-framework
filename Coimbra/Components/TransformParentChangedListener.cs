﻿using UnityEngine;

namespace Coimbra
{
    /// <summary>
    /// Listen to <see cref="OnTransformParentChanged"/> callback.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu(CoimbraUtility.GeneralMenuPath + "Transform Parent Changed Listener")]
    [HelpURL("https://docs.unity3d.com/ScriptReference/MonoBehaviour.OnTransformParentChanged.html")]
    public sealed class TransformParentChangedListener : MonoBehaviour
    {
        public delegate void EventHandler(TransformParentChangedListener sender);

        /// <summary>
        /// Invoked inside <see cref="OnTransformParentChanged"/>.
        /// </summary>
        public event EventHandler OnTrigger;

        private void OnTransformParentChanged()
        {
            OnTrigger?.Invoke(this);
        }
    }
}