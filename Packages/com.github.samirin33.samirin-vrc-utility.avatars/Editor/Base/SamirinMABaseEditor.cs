using System;
using UnityEditor;
using UnityEngine;
using Samirin33.Editor;
using Samirin33.NDMF.Module;

namespace Samirin33.NDMF.Base.Editor
{
    [CustomEditor(typeof(SamirinMABase), true)]
    [CanEditMultipleObjects]
    public class SamirinMABaseEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            SamirinEditorStyleHelper.DrawWithBlueBackground(() => DrawDefaultInspector());
        }

        protected void DrawWithBlueBackground(Action drawContent)
        {
            SamirinEditorStyleHelper.DrawWithBlueBackground(drawContent);
        }

        protected void DrawHelpBoxWithDefaultFont(string message, MessageType type)
        {
            SamirinEditorStyleHelper.DrawHelpBoxWithDefaultFont(message, type);
        }

        protected void DrawWithDefaultFont(Action drawAction)
        {
            SamirinEditorStyleHelper.DrawWithDefaultFont(drawAction);
        }
    }
}