﻿using System.Diagnostics;
using UnityEditor;
using UnityEngine.Rendering;

namespace Coimbra.Editor
{
    /// <summary>
    /// Editor for <see cref="RenderObjectListener"/>.
    /// </summary>
    [CustomEditor(typeof(RenderObjectListener))]
    public sealed class RenderObjectListenerEditor : UnityEditor.Editor
    {
        /// <inheritdoc/>
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GraphicsSettings.currentRenderPipeline.IsValid())
            {
                DisplayRenderPipelineWarning();
            }
        }

        [Conditional("WITH_HDRP")]
        private void DisplayRenderPipelineWarning()
        {
            CoimbraEditorGUIUtility.DrawComponentWarningForRenderPipeline(target.GetType());
        }
    }
}