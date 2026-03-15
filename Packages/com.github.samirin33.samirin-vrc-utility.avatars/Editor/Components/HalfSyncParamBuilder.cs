using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using nadena.dev.modular_avatar.core;
using Samirin33.NDMF.Components;

namespace Samirin33.NDMF.Components.Editor
{
    public static class HalfSyncParamBuilder
    {
        private const string EmptyMotionGUID = "4de039275b65be24c8f0a641d7a44924";
        private static string GeneratedFolder => "Assets/Generated/SamirinVRCUtility/HalfSyncParam";

        private static readonly Dictionary<HalfSyncParam.BitType, int> BitCountMap = new Dictionary<HalfSyncParam.BitType, int>
        {
            { HalfSyncParam.BitType._1bit, 1 },
            { HalfSyncParam.BitType._2bit, 2 },
            { HalfSyncParam.BitType._3bit, 3 },
            { HalfSyncParam.BitType._4bit, 4 },
            { HalfSyncParam.BitType._5bit, 5 },
            { HalfSyncParam.BitType._6bit, 6 },
            { HalfSyncParam.BitType._7bit, 7 },
        };

        public static void Build(GameObject avatarRootObject, params HalfSyncParam[] halfSyncParams)
        {
            if (halfSyncParams == null || halfSyncParams.Length == 0)
                return;

            var (mergedSettings, writeDefault) = MergeSettingsFromModule(halfSyncParams);
            if (mergedSettings.Count == 0)
                return;

            var controller = CreateControllerFromScratch(mergedSettings.ToArray(), writeDefault, out var paramNamesToRegister);
            if (controller == null)
                return;

            AddModularAvatarModule(avatarRootObject, controller, paramNamesToRegister);
        }

        private static (List<HalfSyncParam.syncParamSetting> settings, bool writeDefault) MergeSettingsFromModule(
            HalfSyncParam[] halfSyncParams)
        {
            var processedParamNames = new HashSet<string>(StringComparer.Ordinal);
            var mergedSettings = new List<HalfSyncParam.syncParamSetting>();
            var writeDefault = halfSyncParams.Length > 0 && halfSyncParams[0].writeDefault;

            foreach (var component in halfSyncParams)
            {
                if (component.syncParamSettings == null) continue;

                foreach (var setting in component.syncParamSettings)
                {
                    if (!BitCountMap.ContainsKey(setting.bitType)) continue;

                    var paramName = string.IsNullOrEmpty(setting.paramName)
                        ? $"Param_{setting.paramType}_{setting.bitType}"
                        : setting.paramName;

                    if (processedParamNames.Contains(paramName))
                        continue;

                    processedParamNames.Add(paramName);
                    mergedSettings.Add(setting);
                }

                if (component.writeDefault)
                    writeDefault = true;
            }

            return (mergedSettings, writeDefault);
        }

        private static AnimatorController CreateControllerFromScratch(HalfSyncParam.syncParamSetting[] settings,
            bool writeDefault, out List<(string name, ParameterSyncType syncType)> paramNamesToRegister)
        {
            paramNamesToRegister = new List<(string, ParameterSyncType)>();

            if (!Directory.Exists(GeneratedFolder))
                Directory.CreateDirectory(GeneratedFolder);

            var emptyMotion = LoadEmptyMotion();
            var paramDriverType = GetVRCAvatarParameterDriverType();
            if (paramDriverType == null)
            {
                Debug.LogError("[HalfSyncParam] VRCAvatarParameterDriver 型が見つかりません。VRChat SDK3 Avatars がインストールされているか確認してください。");
                return null;
            }

            var controller = AnimatorController.CreateAnimatorControllerAtPath($"{GeneratedFolder}/HalfSyncParam_Generated.controller");
            if (controller == null)
                return null;

            controller.AddParameter("IsLocal", AnimatorControllerParameterType.Bool);

            var layersToAdd = new List<(AnimatorControllerLayer layer, string paramName, string intParamName, int bitCount, int maxValue, bool isFloat)>();

            foreach (var setting in settings)
            {
                var bitCount = BitCountMap[setting.bitType];
                var paramName = string.IsNullOrEmpty(setting.paramName) ? $"Param_{setting.paramType}_{setting.bitType}" : setting.paramName;
                var maxValue = (1 << bitCount) - 1;
                var isFloat = IsFloatParamType(setting.paramType);
                var intParamName = isFloat ? $"{paramName}_Int" : paramName;

                if (isFloat)
                {
                    controller.AddParameter(paramName, AnimatorControllerParameterType.Float);
                    controller.AddParameter($"{paramName}_Snapped", AnimatorControllerParameterType.Float);
                    controller.AddParameter($"{paramName}_Smoothed", AnimatorControllerParameterType.Float);
                }
                controller.AddParameter(intParamName, AnimatorControllerParameterType.Int);
                for (int i = 0; i < bitCount; i++)
                {
                    var syncParamName = $"SUM/HalfParam/{intParamName}/{i}";
                    controller.AddParameter(syncParamName, AnimatorControllerParameterType.Bool);
                    paramNamesToRegister.Add((syncParamName, ParameterSyncType.Bool));
                }

                var layer = CreateLayerForParam(intParamName, bitCount, maxValue, emptyMotion, writeDefault);
                layersToAdd.Add((layer, paramName, intParamName, bitCount, maxValue, isFloat));
            }

            controller.RemoveLayer(0);

            if (settings.Any(s => IsFloatParamType(s.paramType)))
            {
                controller.AddParameter(new AnimatorControllerParameter
                {
                    name = "dummy",
                    type = AnimatorControllerParameterType.Bool,
                    defaultBool = true
                });

                var floatLayer = CreateFloatConvertLayer(settings, writeDefault);
                if (floatLayer != null)
                    controller.AddLayer(floatLayer);

                var smoothedLayer = CreateSmoothedLayer(settings, controller);
                if (smoothedLayer != null)
                    controller.AddLayer(smoothedLayer);
            }

            foreach (var (layer, _, _, _, _, _) in layersToAdd)
                controller.AddLayer(layer);

            foreach (var (_, _, intParamName, bitCount, maxValue, _) in layersToAdd)
                AddParamDriverBehaviours(controller, intParamName, bitCount, maxValue, paramDriverType);

            AddFloatConvertParamDrivers(controller, settings, paramDriverType);

            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();

            var controllerPath = $"{GeneratedFolder}/HalfSyncParam_Generated.controller";
            AssetDatabase.ImportAsset(controllerPath, ImportAssetOptions.ForceUpdate);
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();

            return controller;
        }

        private static AnimatorControllerLayer CreateLayerForParam(string paramName, int bitCount, int maxValue,
            AnimationClip emptyMotion, bool writeDefault)
        {
            var layerName = $"Convert_IntParam{paramName}_{bitCount}bit";
            var rootSm = new AnimatorStateMachine { name = layerName };

            var localSm = new AnimatorStateMachine { name = "Local" };
            var remoteSm = new AnimatorStateMachine { name = "Remote" };

            rootSm.AddStateMachine(localSm, new Vector3(300, 120, 0));
            rootSm.AddStateMachine(remoteSm, new Vector3(300, 160, 0));

            var localStates = new AnimatorState[maxValue + 1];
            var remoteStates = new AnimatorState[maxValue + 1];

            for (int value = 0; value <= maxValue; value++)
            {
                var localState = localSm.AddState($"Binary {value}", new Vector3(300, 120 + value * 40, 0));
                localState.motion = emptyMotion;
                localState.writeDefaultValues = writeDefault;
                localStates[value] = localState;

                var remoteState = remoteSm.AddState($"Binary {value}", new Vector3(300, 120 + value * 40, 0));
                remoteState.motion = emptyMotion;
                remoteState.writeDefaultValues = writeDefault;
                remoteStates[value] = remoteState;
            }

            localSm.defaultState = localStates[0];
            remoteSm.defaultState = remoteStates[0];
            rootSm.defaultState = localStates[0];

            for (int value = 0; value <= maxValue; value++)
            {
                var localTransition = rootSm.AddAnyStateTransition(localStates[value]);
                localTransition.hasExitTime = false;
                localTransition.exitTime = 0f;
                localTransition.duration = 0f;
                localTransition.canTransitionToSelf = false;
                localTransition.AddCondition(AnimatorConditionMode.If, 0, "IsLocal");
                if (value == 0)
                    localTransition.AddCondition(AnimatorConditionMode.Less, 1, paramName);  // paramName < 1
                else if (value == maxValue)
                    localTransition.AddCondition(AnimatorConditionMode.Greater, maxValue - 1, paramName);  // paramName > maxValue-1
                else
                    localTransition.AddCondition(AnimatorConditionMode.Equals, value, paramName);

                var remoteTransition = rootSm.AddAnyStateTransition(remoteStates[value]);
                remoteTransition.hasExitTime = false;
                remoteTransition.exitTime = 0f;
                remoteTransition.duration = 0f;
                remoteTransition.canTransitionToSelf = false;
                remoteTransition.AddCondition(AnimatorConditionMode.IfNot, 0, "IsLocal");
                for (int b = 0; b < bitCount; b++)
                {
                    var syncParamName = $"SUM/HalfParam/{paramName}/{b}";
                    var boolVal = ((value >> b) & 1) != 0;
                    remoteTransition.AddCondition(boolVal ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0, syncParamName);
                }
            }

            var layer = new AnimatorControllerLayer
            {
                name = layerName,
                defaultWeight = 1f,
                stateMachine = rootSm
            };

            return layer;
        }

        private static AnimatorControllerLayer CreateFloatConvertLayer(HalfSyncParam.syncParamSetting[] settings, bool writeDefault)
        {
            var floatSettings = settings.Where(s => IsFloatParamType(s.paramType)).ToArray();
            if (floatSettings.Length == 0) return null;

            var emptyMotion = LoadEmptyMotion();
            var rootSm = new AnimatorStateMachine { name = "FloatConvert" };

            var brunchState = rootSm.AddState("Brunch", new Vector3(300, 120, 0));
            brunchState.motion = emptyMotion;
            brunchState.writeDefaultValues = writeDefault;

            var localState = rootSm.AddState("Local", new Vector3(180, 180, 0));
            localState.motion = emptyMotion;
            localState.writeDefaultValues = writeDefault;

            var remoteState = rootSm.AddState("Remote", new Vector3(420, 180, 0));
            remoteState.motion = emptyMotion;
            remoteState.writeDefaultValues = writeDefault;

            rootSm.defaultState = brunchState;

            var toLocal = brunchState.AddTransition(localState);
            toLocal.hasExitTime = false;
            toLocal.duration = 0f;
            toLocal.canTransitionToSelf = false;
            toLocal.AddCondition(AnimatorConditionMode.If, 0, "IsLocal");

            var toRaw = brunchState.AddTransition(remoteState);
            toRaw.hasExitTime = false;
            toRaw.duration = 0f;
            toRaw.canTransitionToSelf = false;
            toRaw.AddCondition(AnimatorConditionMode.IfNot, 0, "IsLocal");

            var localSelfTransition = localState.AddTransition(localState);
            localSelfTransition.hasExitTime = false;
            localSelfTransition.duration = 0f;
            localSelfTransition.exitTime = 0f;
            localSelfTransition.canTransitionToSelf = true;
            localSelfTransition.AddCondition(AnimatorConditionMode.If, 0, "dummy");

            var remoteSelfTransition = remoteState.AddTransition(remoteState);
            remoteSelfTransition.hasExitTime = false;
            remoteSelfTransition.duration = 0f;
            remoteSelfTransition.exitTime = 0f;
            remoteSelfTransition.canTransitionToSelf = true;
            remoteSelfTransition.AddCondition(AnimatorConditionMode.If, 0, "dummy");

            return new AnimatorControllerLayer
            {
                name = "FloatConvert",
                defaultWeight = 1f,
                stateMachine = rootSm
            };
        }

        private static void AddFloatConvertParamDrivers(AnimatorController controller, HalfSyncParam.syncParamSetting[] settings,
            Type paramDriverType)
        {
            var floatSettings = settings.Where(s => IsFloatParamType(s.paramType)).ToArray();
            if (floatSettings.Length == 0 || paramDriverType == null) return;

            foreach (var layer in controller.layers)
            {
                if (layer.name != "FloatConvert") continue;

                foreach (var state in layer.stateMachine.states)
                {
                    var stateName = state.state.name;
                    if (stateName == "Local")
                    {
                        foreach (var setting in floatSettings)
                        {
                            var bitCount = BitCountMap[setting.bitType];
                            var maxValue = (1 << bitCount) - 1;
                            var resolution = 1f / maxValue;
                            var paramName = string.IsNullOrEmpty(setting.paramName) ? $"Param_{setting.paramType}_{setting.bitType}" : setting.paramName;
                            var intParamName = $"{paramName}_Int";
                            var (sourceMin, sourceMax, destMin, destMax) = GetFloatConvertRanges(setting.paramType, maxValue);

                            var behaviour = state.state.AddStateMachineBehaviour(paramDriverType);
                            if (behaviour != null)
                            {
                                AssetDatabase.AddObjectToAsset(behaviour, controller);
                                SetParamDriverCopy(behaviour, paramName, intParamName, sourceMin - resolution, sourceMax - resolution, 0, maxValue, clearFirst: true);
                                SetParamDriverCopy(behaviour, intParamName, $"{paramName}_Snapped", 0, maxValue, destMin, destMax, clearFirst: false);
                            }
                        }
                    }
                    else if (stateName == "Remote")
                    {
                        foreach (var setting in floatSettings)
                        {
                            var bitCount = BitCountMap[setting.bitType];
                            var maxValue = (1 << bitCount) - 1;
                            var resolution = 1f / maxValue;
                            var paramName = string.IsNullOrEmpty(setting.paramName) ? $"Param_{setting.paramType}_{setting.bitType}" : setting.paramName;
                            var intParamName = $"{paramName}_Int";
                            var (sourceMin, sourceMax, destMin, destMax) = GetFloatConvertRanges(setting.paramType, maxValue);

                            var behaviour = state.state.AddStateMachineBehaviour(paramDriverType);
                            if (behaviour != null)
                            {
                                AssetDatabase.AddObjectToAsset(behaviour, controller);
                                SetParamDriverCopy(behaviour, intParamName, $"{paramName}_Snapped", 0, maxValue, destMin, destMax, clearFirst: true);
                                SetParamDriverCopy(behaviour, $"{paramName}_Snapped", paramName, sourceMin, sourceMax, destMin, destMax, clearFirst: false);
                            }
                        }
                    }
                }
            }
        }

        private static bool IsFloatParamType(HalfSyncParam.ParamType paramType)
        {
            return paramType == HalfSyncParam.ParamType.FloatZeroToPlusOne
                || paramType == HalfSyncParam.ParamType.FloatMinusOneToPlusOne;
        }

        private static (float sourceMin, float sourceMax, float destMin, float destMax) GetFloatConvertRanges(
            HalfSyncParam.ParamType paramType, int maxValue)
        {
            switch (paramType)
            {
                case HalfSyncParam.ParamType.FloatZeroToPlusOne:
                    return (0f, 1f, 0f, 1f);
                case HalfSyncParam.ParamType.FloatMinusOneToPlusOne:
                    return (-1f, 1f, -1f, 1f);
                default:
                    return (0f, 1f, 0f, 1f);
            }
        }

        private static void SetParamDriverCopy(StateMachineBehaviour behaviour, string sourceName, string destName, float sourceMin, float sourceMax, float destMin, float destMax, bool clearFirst = true)
        {
            var so = new SerializedObject(behaviour);
            var parametersProp = so.FindProperty("parameters");
            if (parametersProp == null) return;

            if (clearFirst)
                parametersProp.ClearArray();
            parametersProp.InsertArrayElementAtIndex(parametersProp.arraySize);
            var entry = parametersProp.GetArrayElementAtIndex(parametersProp.arraySize - 1);

            entry.FindPropertyRelative("name").stringValue = destName;
            entry.FindPropertyRelative("source").stringValue = sourceName;
            var typeProp = entry.FindPropertyRelative("type");
            if (typeProp != null)
            {
                if (typeProp.propertyType == SerializedPropertyType.Enum)
                    typeProp.enumValueIndex = 3;
                else
                    typeProp.intValue = 3;
            }
            var convertProp = entry.FindPropertyRelative("convertRange");
            if (convertProp != null) convertProp.intValue = 1;
            var sm = entry.FindPropertyRelative("sourceMin");
            if (sm != null) sm.floatValue = sourceMin;
            var sM = entry.FindPropertyRelative("sourceMax");
            if (sM != null) sM.floatValue = sourceMax;
            var dm = entry.FindPropertyRelative("destMin");
            if (dm != null) dm.floatValue = destMin;
            var dM = entry.FindPropertyRelative("destMax");
            if (dM != null) dM.floatValue = destMax;

            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static AnimatorControllerLayer CreateSmoothedLayer(HalfSyncParam.syncParamSetting[] settings,
            AnimatorController controller)
        {
            var floatSettings = settings.Where(s => s.paramType == HalfSyncParam.ParamType.FloatZeroToPlusOne || s.paramType == HalfSyncParam.ParamType.FloatMinusOneToPlusOne).ToArray();
            if (floatSettings.Length == 0) return null;

            controller.AddParameter("SUM/HalfParam/SmoothWeight", AnimatorControllerParameterType.Float);
            controller.AddParameter(new AnimatorControllerParameter
            {
                name = "ConstOne",
                type = AnimatorControllerParameterType.Float,
                defaultFloat = 1f
            });
            controller.AddParameter("FPS/Value", AnimatorControllerParameterType.Float);

            var rootSm = new AnimatorStateMachine { name = "Smoothed" };
            var smoothingState = rootSm.AddState("Smoothing", new Vector3(300, 120, 0));
            smoothingState.writeDefaultValues = true;
            rootSm.defaultState = smoothingState;

            var directBlendTree = CreateDirectBlendTree(floatSettings, controller);
            smoothingState.motion = directBlendTree;

            return new AnimatorControllerLayer
            {
                name = "Smoothed",
                defaultWeight = 1f,
                stateMachine = rootSm
            };
        }

        private static BlendTree CreateDirectBlendTree(HalfSyncParam.syncParamSetting[] floatSettings,
            AnimatorController controller)
        {
            var firstParamName = string.IsNullOrEmpty(floatSettings[0].paramName)
                ? $"Param_{floatSettings[0].paramType}_{floatSettings[0].bitType}"
                : floatSettings[0].paramName;

            var directTree = new BlendTree
            {
                name = "Direct",
                blendType = BlendTreeType.Direct,
                blendParameter = firstParamName,
                blendParameterY = firstParamName,
                useAutomaticThresholds = true,
                minThreshold = 0f,
                maxThreshold = 1f
            };
            AssetDatabase.AddObjectToAsset(directTree, controller);

            var fpsWeightFix = CreateFPSWeightFixBlendTree(controller);
            directTree.AddChild(fpsWeightFix, 0f);

            for (int i = 0; i < floatSettings.Length; i++)
            {
                var paramName = string.IsNullOrEmpty(floatSettings[i].paramName)
                    ? $"Param_{floatSettings[i].paramType}_{floatSettings[i].bitType}"
                    : floatSettings[i].paramName;
                var (_, _, destMin, destMax) = GetFloatConvertRanges(floatSettings[i].paramType, 0);
                var smoothedTree = CreateParamSmoothedBlendTree(paramName, controller, destMin, destMax);
                var threshold = floatSettings.Length == 1 ? 1f : (i == 0 ? 0.5f : 1f);
                directTree.AddChild(smoothedTree, threshold);
            }

            SetDirectBlendTreeChildrenParameter(directTree, "ConstOne");

            return directTree;
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

        private static BlendTree CreateFPSWeightFixBlendTree(AnimatorController controller)
        {
            var weight0 = CreateEmbeddedWeightClip(0.5f, "Weight 0", controller);
            var weight1 = CreateEmbeddedWeightClip(0.9f, "Weight 1", controller);

            var tree = new BlendTree
            {
                name = "FPSWeightFix",
                blendType = BlendTreeType.Simple1D,
                blendParameter = "FPS/Value",
                useAutomaticThresholds = true,
                minThreshold = 0f,
                maxThreshold = 1f
            };
            AssetDatabase.AddObjectToAsset(tree, controller);
            tree.AddChild(weight0, 0f);
            tree.AddChild(weight1, 1f);
            return tree;
        }

        private static BlendTree CreateParamSmoothedBlendTree(string paramName, AnimatorController controller, float rangeMin, float rangeMax)
        {
            var snappedParamName = $"{paramName}_Snapped";
            var smoothedParamName = $"{paramName}_Smoothed";

            var clip0 = CreateEmbeddedParamClip(smoothedParamName, rangeMin, controller);
            var clip1 = CreateEmbeddedParamClip(smoothedParamName, rangeMax, controller);

            var rawTree = CreateRawBlendTree(snappedParamName, smoothedParamName, clip0, clip1, controller, rangeMin, rangeMax);
            var smoothTree = CreateSmoothBlendTree(paramName, smoothedParamName, clip0, clip1, controller, rangeMin, rangeMax);

            var tree = new BlendTree
            {
                name = smoothedParamName,
                blendType = BlendTreeType.Simple1D,
                blendParameter = "SUM/HalfParam/SmoothWeight",
                useAutomaticThresholds = true,
                minThreshold = 0f,
                maxThreshold = 1f
            };
            AssetDatabase.AddObjectToAsset(tree, controller);
            tree.AddChild(rawTree, 0f);
            tree.AddChild(smoothTree, 1f);
            return tree;
        }

        private static BlendTree CreateRawBlendTree(string snappedParamName, string smoothedParamName,
            AnimationClip clip0, AnimationClip clip1, AnimatorController controller, float rangeMin, float rangeMax)
        {
            var tree = new BlendTree
            {
                name = "Raw",
                blendType = BlendTreeType.Simple1D,
                blendParameter = snappedParamName,
                useAutomaticThresholds = true,
                minThreshold = rangeMin,
                maxThreshold = rangeMax
            };
            AssetDatabase.AddObjectToAsset(tree, controller);
            tree.AddChild(clip0, rangeMin);
            tree.AddChild(clip1, rangeMax);
            return tree;
        }

        private static BlendTree CreateSmoothBlendTree(string paramName, string smoothedParamName,
            AnimationClip clip0, AnimationClip clip1, AnimatorController controller, float rangeMin, float rangeMax)
        {
            var tree = new BlendTree
            {
                name = "Smooth",
                blendType = BlendTreeType.Simple1D,
                blendParameter = smoothedParamName,
                useAutomaticThresholds = true,
                minThreshold = rangeMin,
                maxThreshold = rangeMax
            };
            AssetDatabase.AddObjectToAsset(tree, controller);
            tree.AddChild(clip0, rangeMin);
            tree.AddChild(clip1, rangeMax);
            return tree;
        }

        private static AnimationClip CreateEmbeddedParamClip(string smoothedParamName, float value,
            AnimatorController controller)
        {
            var clip = CreateAnimatorParamClip(smoothedParamName, value);
            clip.name = $"{smoothedParamName} {value}";
            AssetDatabase.AddObjectToAsset(clip, controller);
            return clip;
        }

        private static AnimationClip CreateEmbeddedWeightClip(float value, string clipName, AnimatorController controller)
        {
            var clip = CreateAnimatorParamClip("SUM/HalfParam/SmoothWeight", value);
            clip.name = clipName;
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

        private static void AddParamDriverBehaviours(AnimatorController controller, string paramName, int bitCount, int maxValue,
            Type paramDriverType)
        {
            if (paramDriverType == null || !typeof(StateMachineBehaviour).IsAssignableFrom(paramDriverType)) return;

            var layerName = $"Convert_IntParam{paramName}_{bitCount}bit";
            foreach (var layer in controller.layers)
            {
                if (layer.name != layerName) continue;

                foreach (var childSm in layer.stateMachine.stateMachines)
                {
                    var isLocal = childSm.stateMachine.name == "Local";
                    var states = childSm.stateMachine.states;

                    for (int i = 0; i < states.Length; i++)
                    {
                        var value = i;
                        var state = states[i].state;

                        var behaviour = state.AddStateMachineBehaviour(paramDriverType);
                        if (behaviour != null)
                        {
                            AssetDatabase.AddObjectToAsset(behaviour, controller);

                            var boolValues = new bool[bitCount];
                            for (int b = 0; b < bitCount; b++)
                                boolValues[b] = ((value >> b) & 1) != 0;

                            SetParamDriverParameters(behaviour, paramName, value, boolValues, isLocal);
                        }
                    }
                }
            }
        }

        private static void SetParamDriverParameters(StateMachineBehaviour behaviour, string paramName, int intValue, bool[] boolValues, bool isLocal)
        {
            var so = new SerializedObject(behaviour);
            var parametersProp = so.FindProperty("parameters");
            if (parametersProp == null) return;

            parametersProp.ClearArray();

            if (isLocal)
            {
                for (int i = 0; i < boolValues.Length; i++)
                {
                    var syncParamName = $"SUM/HalfParam/{paramName}/{i}";
                    parametersProp.InsertArrayElementAtIndex(i);
                    SetParamDriverEntry(parametersProp.GetArrayElementAtIndex(i), syncParamName, boolValues[i] ? 1 : 0);
                }
            }
            else
            {
                parametersProp.InsertArrayElementAtIndex(0);
                SetParamDriverEntry(parametersProp.GetArrayElementAtIndex(0), paramName, intValue);
            }

            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetParamDriverEntry(SerializedProperty entry, string paramName, int value)
        {
            entry.FindPropertyRelative("name").stringValue = paramName;

            var valueProp = entry.FindPropertyRelative("value");
            if (valueProp != null)
            {
                if (valueProp.propertyType == SerializedPropertyType.Float)
                    valueProp.floatValue = value;
                else
                    valueProp.intValue = value;
            }

            var typeProp = entry.FindPropertyRelative("type");
            if (typeProp != null)
            {
                if (typeProp.propertyType == SerializedPropertyType.Enum)
                    typeProp.enumValueIndex = 0; // 0 = Set
                else
                    typeProp.intValue = 0;
            }

            var sourceProp = entry.FindPropertyRelative("source");
            if (sourceProp != null) sourceProp.stringValue = "";
            var valueMinProp = entry.FindPropertyRelative("valueMin");
            if (valueMinProp != null) valueMinProp.intValue = 0;
            var valueMaxProp = entry.FindPropertyRelative("valueMax");
            if (valueMaxProp != null) valueMaxProp.intValue = 0;
        }

        private static AnimationClip LoadEmptyMotion()
        {
            var path = AssetDatabase.GUIDToAssetPath(EmptyMotionGUID);
            if (!string.IsNullOrEmpty(path))
            {
                var loadedClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
                if (loadedClip != null) return loadedClip;
            }

            if (!Directory.Exists(GeneratedFolder))
                Directory.CreateDirectory(GeneratedFolder);

            var clipPath = $"{GeneratedFolder}/HalfSyncParam_Empty.anim";
            var emptyClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
            if (emptyClip == null)
            {
                emptyClip = new AnimationClip();
                AssetDatabase.CreateAsset(emptyClip, clipPath);
                AssetDatabase.SaveAssets();
            }
            return emptyClip;
        }

        /// <summary>
        /// VRCAvatarParameterDriver (VRC Avatar Parameter Driver) の Type を取得。
        /// </summary>
        private static Type GetVRCAvatarParameterDriverType()
        {
            var t = Type.GetType("VRC.SDK3.Avatars.Module.VRCAvatarParameterDriver, VRCSDK3A");
            if (t != null && typeof(StateMachineBehaviour).IsAssignableFrom(t))
                return t;

            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    t = asm.GetType("VRC.SDK3.Avatars.Module.VRCAvatarParameterDriver");
                    if (t != null && typeof(StateMachineBehaviour).IsAssignableFrom(t))
                        return t;
                }
                catch { /* 無視 */ }
            }

            return null;
        }

        private static void AddModularAvatarModule(GameObject componentObject, AnimatorController controller,
            List<(string name, ParameterSyncType syncType)> paramNamesToRegister)
        {
            var mergeAnimator = componentObject.GetComponent<ModularAvatarMergeAnimator>();
            if (mergeAnimator == null)
                mergeAnimator = componentObject.AddComponent<ModularAvatarMergeAnimator>();

            mergeAnimator.animator = controller;
            mergeAnimator.layerPriority = 0;

            var maParameters = componentObject.GetComponent<ModularAvatarParameters>();
            if (maParameters == null)
                maParameters = componentObject.AddComponent<ModularAvatarParameters>();

            foreach (var (paramName, syncType) in paramNamesToRegister)
            {
                if (maParameters.parameters.Exists(p => p.nameOrPrefix == paramName))
                    continue;

                maParameters.parameters.Add(new ParameterConfig
                {
                    nameOrPrefix = paramName,
                    remapTo = "",
                    internalParameter = false,
                    isPrefix = false,
                    syncType = syncType,
                    localOnly = false,
                    defaultValue = 0f,
                    saved = false,
                    hasExplicitDefaultValue = true
                });
            }

            EditorUtility.SetDirty(componentObject);
        }
    }
}
