using UnityEditor;
using UnityEngine;
using Samirin33.NDMF.Base.Editor;

namespace Samirin33.NDMF.Components.Editor
{
    [CustomEditor(typeof(ParameterSmoothing))]
    [CanEditMultipleObjects]
    public class ParameterSmoothingEditor : SamirinMABaseEditor
    {
        private SerializedProperty _parameterSmoothingData;

        private void OnEnable()
        {
            _parameterSmoothingData = serializedObject.FindProperty("parameterSmoothingData");
        }

        public override void OnInspectorGUI()
        {
            DrawWithBlueBackground(() =>
            {
                serializedObject.Update();

                EditorGUILayout.HelpBox(
                    "FloatパラメーターにAAPスムージングをかけることができます！",
                    MessageType.Info);

                EditorGUILayout.LabelField("パラメータスムージング設定");
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                if (_parameterSmoothingData != null)
                {
                    for (int i = 0; i < _parameterSmoothingData.arraySize; i++)
                    {
                        var element = _parameterSmoothingData.GetArrayElementAtIndex(i);
                        var parameterNameProp = element.FindPropertyRelative("parameterName");
                        var smoothWeightProp = element.FindPropertyRelative("smoothWeight");

                        EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.ExpandWidth(true));

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(parameterNameProp.stringValue);
                        GUILayout.FlexibleSpace();
                        EditorGUI.BeginDisabledGroup(i == 0);
                        if (GUILayout.Button("↑", GUILayout.Width(24)))
                        {
                            _parameterSmoothingData.MoveArrayElement(i, i - 1);
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.EndVertical();
                            break;
                        }
                        EditorGUI.EndDisabledGroup();
                        EditorGUI.BeginDisabledGroup(i == _parameterSmoothingData.arraySize - 1);
                        if (GUILayout.Button("↓", GUILayout.Width(24)))
                        {
                            _parameterSmoothingData.MoveArrayElement(i, i + 1);
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.EndVertical();
                            break;
                        }
                        EditorGUI.EndDisabledGroup();
                        if (GUILayout.Button("削除", GUILayout.Width(50)))
                        {
                            _parameterSmoothingData.DeleteArrayElementAtIndex(i);
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.EndVertical();
                            break;
                        }
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.PropertyField(parameterNameProp, new GUIContent("パラメータ名"));
                        EditorGUILayout.Slider(smoothWeightProp, 0.0001f, 0.9999f, new GUIContent("スムージング重み(高いほどゆっくり)"));

                        var paramName = parameterNameProp.stringValue;
                        if (!string.IsNullOrEmpty(paramName))
                        {
                            var smoothedName = $"{paramName}_Smoothed";
                            var fixedWeightName = $"{paramName}_FixedSmoothWeight";

                            EditorGUILayout.HelpBox(
                                "以下のパラメータが出力されます。",
                                MessageType.None);

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
                        _parameterSmoothingData.arraySize++;
                    }
                }
                EditorGUILayout.EndVertical();

                serializedObject.ApplyModifiedProperties();
            });
        }
    }
}
