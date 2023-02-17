﻿using Coimbra.Listeners;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Coimbra.Services.PlayerLoopEvents
{
    [DisallowMultipleComponent]
    [AddComponentMenu("")]
    [HelpURL("https://docs.unity3d.com/ScriptReference/MonoBehaviour.FixedUpdate.html")]
    [MovedFrom(true, "Coimbra")]
    internal sealed class FixedUpdateListener : PlayerLoopListenerBase
    {
        /// <inheritdoc />
        public override event EventHandler OnTrigger
        {
            add
            {
                base.OnTrigger += value;

                if (this.IsValid())
                {
                    enabled = HasListener;
                }
            }

            remove
            {
                base.OnTrigger -= value;

                if (this.IsValid())
                {
                    enabled = HasListener;
                }
            }
        }

        /// <inheritdoc/>
        protected override void OnPreInitializeActor()
        {
            enabled = HasListener;
        }

        /// <inheritdoc/>
        protected override void OnPostInitializeActor() { }

        private void FixedUpdate()
        {
            Trigger(Time.deltaTime);
        }
    }
}
