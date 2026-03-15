using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Samirin33.Editor;

namespace Samirin33.AvatarEditor.Animation.Editor
{
    static class VRCAvatarParamSetterSettingsProvider
    {
        const string SettingsPath = "Preferences/Samirin Editor Tools/VRCAvatarParamSetter";

        [SettingsProvider]
        public static SettingsProvider Create()
        {
            return new SettingsProvider(SettingsPath, SettingsScope.User)
            {
                label = "VRChat Avatar Param Setter",
                keywords = new HashSet<string>(new[] { "VRChat", "Avatar", "Animator", "Parameter" }),
                guiHandler = OnGUI
            };
        }

        static void OnGUI(string searchContext)
        {
            SamirinEditorStyleHelper.DrawWithBlueBackground(() =>
            {
                VRCAvatarParamSetterPreferences.ClearCache();

                EditorGUILayout.Space(4);
                EditorGUILayout.HelpBox(
                    "一括追加時に「追加しない」ようにするパラメータを選んでください。チェックを付けたパラメータは「不足パラメータを一括追加」で追加されません。",
                    MessageType.Info);
                EditorGUILayout.Space(4);

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("すべて除外", GUILayout.Height(22)))
                {
                    VRCAvatarParamSetterPreferences.SetAllExcluded(true);
                    VRCAvatarParamSetterPreferences.ClearCache();
                    RepaintPreferences();
                }
                if (GUILayout.Button("デフォルト設定にもどす", GUILayout.Height(22)))
                {
                    VRCAvatarParamSetterPreferences.ResetToDefault();
                    VRCAvatarParamSetterPreferences.ClearCache();
                    RepaintPreferences();
                }
                if (GUILayout.Button("すべての除外を解除", GUILayout.Height(22)))
                {
                    VRCAvatarParamSetterPreferences.SetAllExcluded(false);
                    VRCAvatarParamSetterPreferences.ClearCache();
                    RepaintPreferences();
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(6);

                bool addAtFront = VRCAvatarParamSetterPreferences.AddParametersAtFront;
                bool newAddAtFront = EditorGUILayout.ToggleLeft("追加したパラメータを先頭に移動する", addAtFront);
                if (newAddAtFront != addAtFront)
                    VRCAvatarParamSetterPreferences.AddParametersAtFront = newAddAtFront;
                EditorGUILayout.Space(4);

                foreach (var p in VRChatBuiltInParams.All)
                {
                    bool excluded = VRCAvatarParamSetterPreferences.IsExcluded(p.Name);
                    var label = new GUIContent($"{p.Name} ({p.Type})", string.IsNullOrEmpty(p.Description) ? null : p.Description);
                    bool newVal = EditorGUILayout.ToggleLeft(label, excluded);
                    if (newVal != excluded)
                        VRCAvatarParamSetterPreferences.SetExcluded(p.Name, newVal);
                }
            });
        }

        static void RepaintPreferences()
        {
            var window = EditorWindow.GetWindow(typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.PreferencesWindow"));
            window?.Repaint();
        }
    }
}
