using UnityEditor;
using UnityEngine;
using Samirin33.Editor;
using Samirin33.AvatarEditer.Tools;

namespace Samirin33.AvatarEditor.Tools.Editor
{
    [CustomEditor(typeof(FPSLimiter))]
    [CanEditMultipleObjects]
    public class FPSLimiterEditor : UnityEditor.Editor
    {
        [MenuItem("GameObject/SamirinEditorTools/FPS Limiter", false, 10)]
        private static void CreateFPSLimiterGameObject(MenuCommand menuCommand)
        {
            var go = new GameObject("FPS Limiter");
            go.AddComponent<FPSLimiter>();

            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(go, "Create FPS Limiter");
            Selection.activeObject = go;
        }
        private SerializedProperty _targetFrameRateProp;

        private void OnEnable()
        {
            _targetFrameRateProp = serializedObject.FindProperty("targetFrameRate");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            SamirinEditorStyleHelper.DrawWithBlueBackground(() =>
            {
                EditorGUILayout.Space(4);

                // スライダー（10〜144、-1は無制限のためボタンで設定）
                int currentValue = _targetFrameRateProp.intValue;
                int sliderValue = currentValue < 1 ? -1 : (currentValue > 144 ? 144 : currentValue);

                EditorGUI.BeginChangeCheck();
                int newValue = EditorGUILayout.IntSlider("目標FPS", sliderValue, 1, 120);
                if (EditorGUI.EndChangeCheck())
                {
                    _targetFrameRateProp.intValue = newValue;
                }

                // -1（無制限）の場合は表示を調整
                if (currentValue == -1)
                {
                    SamirinEditorStyleHelper.DrawWithDefaultFont(() =>
                        EditorGUILayout.HelpBox("現在: 無制限（-1）", MessageType.Info));
                }

                EditorGUILayout.Space(8);

                // プリセットボタン
                EditorGUILayout.LabelField("プリセット", EditorStyles.boldLabel);
                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button("10 FPS"))
                {
                    _targetFrameRateProp.intValue = 10;
                }
                if (GUILayout.Button("30 FPS"))
                {
                    _targetFrameRateProp.intValue = 30;
                }
                if (GUILayout.Button("60 FPS"))
                {
                    _targetFrameRateProp.intValue = 60;
                }
                if (GUILayout.Button("120 FPS"))
                {
                    _targetFrameRateProp.intValue = 120;
                }
                if (GUILayout.Button("無制限"))
                {
                    _targetFrameRateProp.intValue = -1;
                }

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(4);
            });

            serializedObject.ApplyModifiedProperties();
        }
    }
}