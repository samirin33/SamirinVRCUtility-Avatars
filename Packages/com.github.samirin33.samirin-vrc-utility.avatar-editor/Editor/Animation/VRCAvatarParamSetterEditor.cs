using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Samirin33.Editor;

namespace Samirin33.AvatarEditor.Animation.Editor
{
    public class VRCAvatarParamSetterEditor : EditorWindow
    {
        RuntimeAnimatorController _animatorController;
        static bool _showParamDescriptions;
        static Vector2 _paramDescriptionScroll;

        [MenuItem("samirin33 Editor Tools/VRChat Avatar Param Setter")]
        public static void Open()
        {
            var w = GetWindow<VRCAvatarParamSetterEditor>();
            w.titleContent = new GUIContent("VRChat Param Setter");
        }

        void OnGUI()
        {
            SamirinEditorStyleHelper.DrawWithBlueBackground(() =>
            {
                EditorGUILayout.Space(4);
                _animatorController = (RuntimeAnimatorController)EditorGUILayout.ObjectField(
                    "Animator Controller",
                    _animatorController,
                    typeof(RuntimeAnimatorController),
                    false);

                EditorGUILayout.Space(4);

                // ビルトインパラメータの説明は Animator が未設定でも表示する
                DrawParamDescriptionSection();

                if (_animatorController == null)
                {
                    EditorGUILayout.HelpBox("Animator Controller を指定してください。", MessageType.Info);
                    DrawPreferencesLink();
                    return;
                }

                var controller = _animatorController as AnimatorController;
                if (controller == null)
                {
                    EditorGUILayout.HelpBox("Animator Controller アセット（.controller）を指定してください。", MessageType.Warning);
                    DrawPreferencesLink();
                    return;
                }

                if (GUILayout.Button("不足している VRChat パラメータを一括追加"))
                {
                    AddMissingParameters(controller);
                }

                DrawPreferencesLink();
            });
        }

        void DrawParamDescriptionSection()
        {
            _showParamDescriptions = EditorGUILayout.BeginFoldoutHeaderGroup(_showParamDescriptions, "ビルトインパラメータの役割・詳細");
            if (_showParamDescriptions)
            {
                EditorGUILayout.Space(2);
                _paramDescriptionScroll = EditorGUILayout.BeginScrollView(_paramDescriptionScroll, GUILayout.ExpandHeight(true));

                var controller = _animatorController as AnimatorController;

                foreach (var p in VRChatBuiltInParams.All)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.MinHeight(0));
                    EditorGUILayout.LabelField($"{p.Name} ({p.Type})", EditorStyles.label);
                    if (!string.IsNullOrEmpty(p.Description))
                        EditorGUILayout.LabelField(p.Description, EditorStyles.label);

                    EditorGUILayout.Space(2);
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();

                    // パラメータ名コピー
                    if (GUILayout.Button("名前をコピー", GUILayout.Width(110)))
                    {
                        EditorGUIUtility.systemCopyBuffer = p.Name;
                    }

                    // Animator への追加ボタン
                    bool hasController = controller != null;
                    bool hasParam = false;
                    if (hasController)
                    {
                        var parameters = controller.parameters;
                        for (int i = 0; i < parameters.Length; i++)
                        {
                            if (parameters[i].name == p.Name)
                            {
                                hasParam = true;
                                break;
                            }
                        }
                    }

                    bool isExcluded = VRCAvatarParamSetterPreferences.IsExcluded(p.Name);

                    using (new EditorGUI.DisabledScope(!hasController || hasParam || isExcluded))
                    {
                        var label = hasParam ? "追加済み" : "Animatorへ追加";
                        if (GUILayout.Button(label, GUILayout.Width(120)))
                        {
                            Undo.RecordObject(controller, "Add VRChat parameter: " + p.Name);
                            controller.AddParameter(p.Name, p.Type);
                            EditorUtility.SetDirty(controller);
                            AssetDatabase.SaveAssetIfDirty(controller);
                        }
                    }

                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space(2);
                }
                EditorGUILayout.EndScrollView();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        static void DrawPreferencesLink()
        {
            EditorGUILayout.Space(4);
            EditorGUILayout.HelpBox(
                "除外するパラメータは Edit > Preferences > SamirinEditorTools/VRCAvatarParamSetter で設定できます。",
                MessageType.None);
        }

        static void AddMissingParameters(AnimatorController controller)
        {
            var existing = new HashSet<string>();
            foreach (var p in controller.parameters)
                existing.Add(p.name);

            var addedNames = new List<string>();
            foreach (var def in VRChatBuiltInParams.All)
            {
                if (existing.Contains(def.Name))
                    continue;
                if (VRCAvatarParamSetterPreferences.IsExcluded(def.Name))
                    continue;

                Undo.RecordObject(controller, "Add VRChat parameter: " + def.Name);
                controller.AddParameter(def.Name, def.Type);
                existing.Add(def.Name);
                addedNames.Add(def.Name);
            }

            if (addedNames.Count > 0 && VRCAvatarParamSetterPreferences.AddParametersAtFront)
            {
                Undo.RecordObject(controller, "Reorder parameters");
                var current = controller.parameters;
                var addedSet = new HashSet<string>(addedNames);
                var reordered = new List<AnimatorControllerParameter>(current.Length);
                foreach (var name in addedNames)
                {
                    for (int i = 0; i < current.Length; i++)
                    {
                        if (current[i].name == name)
                        {
                            reordered.Add(current[i]);
                            break;
                        }
                    }
                }
                foreach (var p in current)
                {
                    if (!addedSet.Contains(p.name))
                        reordered.Add(p);
                }
                controller.parameters = reordered.ToArray();
            }

            if (addedNames.Count > 0)
            {
                EditorUtility.SetDirty(controller);
                AssetDatabase.SaveAssetIfDirty(controller);
            }

            Debug.Log($"[VRCAvatarParamSetter] {controller.name}: {addedNames.Count} 個のパラメータを追加しました。");
        }
    }
}
