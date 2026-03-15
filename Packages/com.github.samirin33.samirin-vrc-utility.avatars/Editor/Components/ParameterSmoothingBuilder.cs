using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using nadena.dev.modular_avatar.core;
using Samirin33.NDMF.Base.Plugin;

namespace Samirin33.NDMF.Components.Editor
{
    public static class ParameterSmoothingBuilder
    {
        [InitializeOnLoadMethod]
        private static void RegisterBuilder()
        {
            SamirinMABaseSingleBuildRegistry.Register<ParameterSmoothing>(Build);
        }

        private const string EmptyMotionGUID = "4de039275b65be24c8f0a641d7a44924";
        private static string GeneratedFolder => "Assets/Generated/SamirinVRCUtility/ParameterSmoothing";

        public static void Build(GameObject avatarRootObject, params ParameterSmoothing[] parameterSmoothings)
        {
            if (parameterSmoothings == null || parameterSmoothings.Length == 0)
                return;

            var mergedInfos = MergeParameterSmoothingData(parameterSmoothings);
            if (mergedInfos.Count == 0)
                return;

            var controller = CreateControllerFromParameterSmoothingData(mergedInfos.ToArray(), out var paramNamesToRegister);
            if (controller == null)
                return;

            AddModularAvatarModule(avatarRootObject, controller, paramNamesToRegister);
        }

        private static List<ParameterSmoothing.ParameterSmoothingInfo> MergeParameterSmoothingData(
            ParameterSmoothing[] parameterSmoothings)
        {
            var processedParamNames = new HashSet<string>(StringComparer.Ordinal);
            var merged = new List<ParameterSmoothing.ParameterSmoothingInfo>();

            foreach (var component in parameterSmoothings)
            {
                if (component.parameterSmoothingData == null) continue;

                foreach (var info in component.parameterSmoothingData)
                {
                    if (string.IsNullOrEmpty(info.parameterName)) continue;
                    if (processedParamNames.Contains(info.parameterName)) continue;

                    processedParamNames.Add(info.parameterName);
                    merged.Add(info);
                }
            }

            return merged;
        }

        private static AnimatorController CreateControllerFromParameterSmoothingData(
            ParameterSmoothing.ParameterSmoothingInfo[] infos,
            out List<(string name, ParameterSyncType syncType)> paramNamesToRegister)
        {
            paramNamesToRegister = new List<(string, ParameterSyncType)>();

            if (!Directory.Exists(GeneratedFolder))
                Directory.CreateDirectory(GeneratedFolder);

            var controller = AnimatorController.CreateAnimatorControllerAtPath(
                $"{GeneratedFolder}/ParameterSmoothing_Generated.controller");
            if (controller == null)
                return null;

            // パラメータ追加: ConstOne, FPS/Value, 各パラメータ用 (param, param_Smoothed, param_FixedSmoothWeight)
            controller.AddParameter(new AnimatorControllerParameter
            {
                name = "ConstOne",
                type = AnimatorControllerParameterType.Float,
                defaultFloat = 1f
            });
            controller.AddParameter("FPS/Value", AnimatorControllerParameterType.Float);
            paramNamesToRegister.Add(("FPS/Value", ParameterSyncType.Float));

            foreach (var info in infos)
            {
                var paramName = info.parameterName;
                controller.AddParameter(paramName, AnimatorControllerParameterType.Float);
                controller.AddParameter($"{paramName}_Smoothed", AnimatorControllerParameterType.Float);
                controller.AddParameter($"{paramName}_FixedSmoothWeight", AnimatorControllerParameterType.Float);
                paramNamesToRegister.Add((paramName, ParameterSyncType.Float));
                paramNamesToRegister.Add(($"{paramName}_Smoothed", ParameterSyncType.Float));
                paramNamesToRegister.Add(($"{paramName}_FixedSmoothWeight", ParameterSyncType.Float));
            }

            controller.RemoveLayer(0);

            // ParameterSmoothing レイヤー (ParamSmooth.controller の構造)
            var layer = CreateParameterSmoothingLayer(infos, controller);
            controller.AddLayer(layer);

            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();

            var controllerPath = $"{GeneratedFolder}/ParameterSmoothing_Generated.controller";
            AssetDatabase.ImportAsset(controllerPath, ImportAssetOptions.ForceUpdate);
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();

            return controller;
        }

        private static AnimatorControllerLayer CreateParameterSmoothingLayer(
            ParameterSmoothing.ParameterSmoothingInfo[] infos,
            AnimatorController controller)
        {
            var rootSm = new AnimatorStateMachine { name = "ParameterSmoothing" };
            var smoothingState = rootSm.AddState("Smoothing", new Vector3(280, 120, 0));
            smoothingState.writeDefaultValues = true;
            rootSm.defaultState = smoothingState;

            var rootBlendTree = CreateRootDirectBlendTree(infos, controller);
            smoothingState.motion = rootBlendTree;

            // State の Time Parameter を FPS/Value に設定
            var stateSo = new SerializedObject(smoothingState);
            var timeParamProp = stateSo.FindProperty("m_TimeParameterActive");
            if (timeParamProp != null) timeParamProp.boolValue = true;
            var timeParamNameProp = stateSo.FindProperty("m_TimeParameter");
            if (timeParamNameProp != null) timeParamNameProp.stringValue = "FPS/Value";
            stateSo.ApplyModifiedPropertiesWithoutUndo();

            return new AnimatorControllerLayer
            {
                name = "ParameterSmoothing",
                defaultWeight = 0f,
                stateMachine = rootSm
            };
        }

        /// <summary>
        /// ParamSmooth.controller のルートと同じ構造: Direct, 第一パラメータでブレンド。
        /// Child 0: WeightFix (空・後で指示), Child 1: 第一パラメータの Smoothing ツリー。
        /// 複数パラメータ時は 0, 0.5, 1 等で各パラメータの Smoothing ツリーを追加。
        /// </summary>
        private static BlendTree CreateRootDirectBlendTree(
            ParameterSmoothing.ParameterSmoothingInfo[] infos,
            AnimatorController controller)
        {
            var firstParamName = infos[0].parameterName;
            var directTree = new BlendTree
            {
                name = "Blend Tree",
                blendType = BlendTreeType.Direct,
                blendParameter = firstParamName,
                blendParameterY = firstParamName,
                useAutomaticThresholds = true,
                minThreshold = 0f,
                maxThreshold = 1f
            };
            AssetDatabase.AddObjectToAsset(directTree, controller);

            // WeightFix: 数式に従ってキーフレームを生成した Clip を配置（SampleRate 255, フレーム 1~255、全パラメータ分のカーブ）
            var weightFixClip = CreateWeightFixClip(infos, controller);
            directTree.AddChild(weightFixClip, 0f);

            for (int i = 0; i < infos.Length; i++)
            {
                var paramSmoothingTree = CreateParamSmoothingBlendTree(infos[i], controller);
                var threshold = infos.Length == 1 ? 1f : (i == 0 ? 0.5f : 1f);
                directTree.AddChild(paramSmoothingTree, threshold);
            }

            SetDirectBlendTreeChildrenParameter(directTree, "ConstOne");

            return directTree;
        }

        private const int WeightFixSampleRate = 255;
        private const int WeightFixFrameStart = 1;
        private const int WeightFixFrameEnd = 255;

        /// <summary>
        /// WeightFix 用クリップを生成。SampleRate 255、フレーム 1~255 にキーフレーム。
        /// 全パラメータ分の AnimatorFloat カーブを追加。
        /// value = (1 - Mathf.Pow((1/targetWeight), (1/(256 - frameIndex))))
        /// </summary>
        private static AnimationClip CreateWeightFixClip(
            ParameterSmoothing.ParameterSmoothingInfo[] infos,
            AnimatorController controller)
        {
            var clip = new AnimationClip();
            clip.frameRate = WeightFixSampleRate;

            var keyframeCount = WeightFixFrameEnd - WeightFixFrameStart + 1;

            foreach (var info in infos)
            {
                var paramName = info.parameterName;
                var fixedWeightParamName = $"{paramName}_FixedSmoothWeight";
                var inputWeight = info.smoothWeight;
                var targetWeight = inputWeight * inputWeight;

                var keyframes = new Keyframe[keyframeCount];
                for (int i = 0; i < keyframeCount; i++)
                {
                    int frameIndex = WeightFixFrameStart + i;
                    float time = frameIndex / (float)WeightFixSampleRate;
                    float value = 2 - Mathf.Pow(1f / targetWeight, 1f / frameIndex);
                    keyframes[i] = new Keyframe(time, value);
                }

                var curve = new AnimationCurve(keyframes);
                // curve は Unity によりキーがマージされることがあるため、実際のキー数でループする
                for (int i = 0; i < curve.keys.Length; i++)
                {
                    AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.Linear);
                    AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.Linear);
                }

                var binding = new EditorCurveBinding
                {
                    path = "",
                    type = typeof(Animator),
                    propertyName = fixedWeightParamName
                };
                AnimationUtility.SetEditorCurve(clip, binding, curve);
            }

            clip.name = "WeightFix";
            AssetDatabase.AddObjectToAsset(clip, controller);
            return clip;
        }

        /// <summary>
        /// 「Hoge Smoothing」相当: FixedSmoothWeight で Raw と Smoothed をブレンド。
        /// </summary>
        private static BlendTree CreateParamSmoothingBlendTree(
            ParameterSmoothing.ParameterSmoothingInfo info,
            AnimatorController controller)
        {
            var paramName = info.parameterName;
            var smoothedParamName = $"{paramName}_Smoothed";
            var fixedWeightParamName = $"{paramName}_FixedSmoothWeight";

            var clip0 = CreateEmbeddedParamClip(smoothedParamName, 0f, controller);
            var clip1 = CreateEmbeddedParamClip(smoothedParamName, 1f, controller);

            var rawTree = CreateRawBlendTree(paramName, smoothedParamName, clip0, clip1, controller);
            var smoothedTree = CreateSmoothedBlendTree(paramName, smoothedParamName, clip0, clip1, controller);

            var tree = new BlendTree
            {
                name = $"{paramName} Smoothing",
                blendType = BlendTreeType.Simple1D,
                blendParameter = fixedWeightParamName,
                useAutomaticThresholds = true,
                minThreshold = 0f,
                maxThreshold = 1f
            };
            AssetDatabase.AddObjectToAsset(tree, controller);
            tree.AddChild(rawTree, 0f);
            tree.AddChild(smoothedTree, 1f);
            return tree;
        }

        private static BlendTree CreateRawBlendTree(string paramName, string smoothedParamName,
            Motion clip0, Motion clip1, AnimatorController controller)
        {
            var tree = new BlendTree
            {
                name = "Raw",
                blendType = BlendTreeType.Simple1D,
                blendParameter = paramName,
                useAutomaticThresholds = true,
                minThreshold = 0f,
                maxThreshold = 1f
            };
            AssetDatabase.AddObjectToAsset(tree, controller);
            tree.AddChild(clip0, 0f);
            tree.AddChild(clip1, 1f);
            return tree;
        }

        private static BlendTree CreateSmoothedBlendTree(string paramName, string smoothedParamName,
            Motion clip0, Motion clip1, AnimatorController controller)
        {
            var tree = new BlendTree
            {
                name = "Smoothed",
                blendType = BlendTreeType.Simple1D,
                blendParameter = smoothedParamName,
                useAutomaticThresholds = true,
                minThreshold = 0f,
                maxThreshold = 1f
            };
            AssetDatabase.AddObjectToAsset(tree, controller);
            tree.AddChild(clip0, 0f);
            tree.AddChild(clip1, 1f);
            return tree;
        }

        private static void SetDirectBlendTreeChildrenParameter(BlendTree blendTree, string parameterName)
        {
            var so = new SerializedObject(blendTree);
            var childrenProp = so.FindProperty("m_Childs");
            if (childrenProp == null) return;

            for (int i = 0; i < childrenProp.arraySize; i++)
            {
                var childProp = childrenProp.GetArrayElementAtIndex(i);
                var directBlendProp = childProp.FindPropertyRelative("m_DirectBlendParameter");
                if (directBlendProp != null)
                    directBlendProp.stringValue = parameterName;

                var motionProp = childProp.FindPropertyRelative("m_Motion");
                if (motionProp != null && motionProp.objectReferenceValue is BlendTree childTree)
                    SetDirectBlendTreeChildrenParameter(childTree, parameterName);
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static AnimationClip CreateEmbeddedParamClip(string paramName, float value,
            AnimatorController controller)
        {
            var clip = CreateAnimatorParamClip(paramName, value);
            clip.name = $"{paramName} {value}";
            AssetDatabase.AddObjectToAsset(clip, controller);
            return clip;
        }

        private static AnimationClip CreateAnimatorParamClip(string paramName, float value)
        {
            var clip = new AnimationClip();
            var curve = AnimationCurve.Constant(0f, 0f, value);
            var binding = new EditorCurveBinding
            {
                path = "",
                type = typeof(Animator),
                propertyName = paramName
            };
            AnimationUtility.SetEditorCurve(clip, binding, curve);
            return clip;
        }

        private static void AddModularAvatarModule(GameObject avatarRootObject, AnimatorController controller,
            List<(string name, ParameterSyncType syncType)> paramNamesToRegister)
        {
            var mergeAnimator = avatarRootObject.AddComponent<ModularAvatarMergeAnimator>();

            mergeAnimator.animator = controller;
            mergeAnimator.layerPriority = 0;

            EditorUtility.SetDirty(avatarRootObject);
        }
    }
}
