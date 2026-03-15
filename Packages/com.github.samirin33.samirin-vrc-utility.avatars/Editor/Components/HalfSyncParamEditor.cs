using UnityEditor;
using UnityEngine;
using Samirin33.Editor;
using Samirin33.NDMF.Base.Editor;

namespace Samirin33.NDMF.Components.Editor
{
    [CustomEditor(typeof(HalfSyncParam))]
    [CanEditMultipleObjects]
    public class HalfSyncParamEditor : SamirinMABaseEditor
    {
        private SerializedProperty _syncParamSettings;
        private SerializedProperty _writeDefault;

        private void OnEnable()
        {
            _syncParamSettings = serializedObject.FindProperty("syncParamSettings");
            _writeDefault = serializedObject.FindProperty("writeDefault");
        }

        public override void OnInspectorGUI()
        {
            DrawWithBlueBackground(() =>
            {
                serializedObject.Update();

                EditorGUILayout.LabelField("同期パラメータ設定", EditorStyles.boldLabel);
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                if (_syncParamSettings != null)
                {
                    for (int i = 0; i < _syncParamSettings.arraySize; i++)
                    {
                        var element = _syncParamSettings.GetArrayElementAtIndex(i);
                        var paramNameProp = element.FindPropertyRelative("paramName");
                        var paramTypeProp = element.FindPropertyRelative("paramType");
                        var bitTypeProp = element.FindPropertyRelative("bitType");

                        EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.ExpandWidth(true));

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField($"要素 {i + 1}", EditorStyles.boldLabel, GUILayout.Width(60));
                        EditorGUI.BeginDisabledGroup(i == 0);
                        if (GUILayout.Button("↑", GUILayout.Width(24)))
                        {
                            _syncParamSettings.MoveArrayElement(i, i - 1);
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.EndVertical();
                            break;
                        }
                        EditorGUI.EndDisabledGroup();
                        EditorGUI.BeginDisabledGroup(i == _syncParamSettings.arraySize - 1);
                        if (GUILayout.Button("↓", GUILayout.Width(24)))
                        {
                            _syncParamSettings.MoveArrayElement(i, i + 1);
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.EndVertical();
                            break;
                        }
                        EditorGUI.EndDisabledGroup();
                        if (GUILayout.Button("削除", GUILayout.Width(50)))
                        {
                            _syncParamSettings.DeleteArrayElementAtIndex(i);
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.EndVertical();
                            break;
                        }
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.PropertyField(paramNameProp, new GUIContent("パラメータ名"));
                        EditorGUILayout.PropertyField(paramTypeProp, new GUIContent("タイプ"));
                        EditorGUILayout.PropertyField(bitTypeProp, new GUIContent("ビット数"));

                        var paramType = (HalfSyncParam.ParamType)paramTypeProp.enumValueIndex;
                        var bitType = (HalfSyncParam.BitType)bitTypeProp.enumValueIndex;
                        var description = GetParamTypeDescription(paramType, bitType);
                        if (!string.IsNullOrEmpty(description))
                            EditorGUILayout.HelpBox(description, MessageType.None);

                        if (IsFloatParamType(paramType))
                        {
                            var paramName = paramNameProp.stringValue;
                            if (string.IsNullOrEmpty(paramName))
                                paramName = $"Param_{paramType}_{bitType}";
                            var snappedName = $"{paramName}_Snapped";
                            var smoothedName = $"{paramName}_Smoothed";

                            EditorGUILayout.HelpBox(
                                "分解能に合わせてスナップとスムージングされた数値を以下から取得できます。(Remoteでの結果はこれに一致します)",
                                MessageType.None);

                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField(snappedName, GUILayout.ExpandWidth(true));
                            if (GUILayout.Button("コピー", GUILayout.Width(50)))
                                EditorGUIUtility.systemCopyBuffer = snappedName;
                            EditorGUILayout.EndHorizontal();

                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField(smoothedName, GUILayout.ExpandWidth(true));
                            if (GUILayout.Button("コピー", GUILayout.Width(50)))
                                EditorGUIUtility.systemCopyBuffer = smoothedName;
                            EditorGUILayout.EndHorizontal();
                        }

                        EditorGUILayout.EndVertical();
                        EditorGUILayout.Space(3);
                    }

                    if (GUILayout.Button("+ 追加"))
                    {
                        _syncParamSettings.arraySize++;
                        var newElement = _syncParamSettings.GetArrayElementAtIndex(_syncParamSettings.arraySize - 1);
                        var bitTypeProp = newElement.FindPropertyRelative("bitType");
                        if (bitTypeProp != null)
                            bitTypeProp.enumValueIndex = (int)HalfSyncParam.BitType._4bit;
                    }
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(5);

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.PropertyField(_writeDefault, new GUIContent("生成されるステートのWrite Default"));
                EditorGUILayout.EndVertical();

                serializedObject.ApplyModifiedProperties();
            });
        }

        private static string GetParamTypeDescription(HalfSyncParam.ParamType paramType, HalfSyncParam.BitType bitType)
        {
            switch (paramType)
            {
                case HalfSyncParam.ParamType.Int:
                    return $"0~{GetMaxValue(bitType)}のIntを同期できます。";
                case HalfSyncParam.ParamType.FloatZeroToPlusOne:
                    return $"0～1のFloatを同期できます。(分解能: {1f / GetMaxValue(bitType)})";
                case HalfSyncParam.ParamType.FloatMinusOneToPlusOne:
                    return $"-1～1のFloatを同期できます。(分解能: {2f / GetMaxValue(bitType)})";
                default:
                    return "";
            }
        }

        private static bool IsFloatParamType(HalfSyncParam.ParamType paramType)
        {
            return paramType == HalfSyncParam.ParamType.FloatZeroToPlusOne
                || paramType == HalfSyncParam.ParamType.FloatMinusOneToPlusOne;
        }

        private static int GetMaxValue(HalfSyncParam.BitType bitType)
        {
            switch (bitType)
            {
                case HalfSyncParam.BitType._1bit:
                    return 1;
                case HalfSyncParam.BitType._2bit:
                    return 3;
                case HalfSyncParam.BitType._3bit:
                    return 7;
                case HalfSyncParam.BitType._4bit:
                    return 15;
                case HalfSyncParam.BitType._5bit:
                    return 31;
                case HalfSyncParam.BitType._6bit:
                    return 63;
                case HalfSyncParam.BitType._7bit:
                    return 127;
                default:
                    return 0;
            }
        }
    }
}