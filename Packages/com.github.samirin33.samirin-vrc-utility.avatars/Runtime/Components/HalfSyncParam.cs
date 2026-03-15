using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using nadena.dev.ndmf;
using Samirin33.NDMF.Base;

namespace Samirin33.NDMF.Components
{
    [AddComponentMenu("samirin33 VRC/HalfSyncParam")]
    public class HalfSyncParam : SamirinMABaseSingle
    {
        private void Reset()
        {
            priority = 50;
        }

        [System.Serializable]
        public class syncParamSetting
        {
            public string paramName;
            public ParamType paramType;
            public BitType bitType;

            public float smoothWeight = 0.2f;

            /// <summary> true の場合、ビルド時に親 Animator 内のこのパラメータ参照をすべて _Smoothed に置換する。 </summary>
            public bool replaceWithSmoothedInAnimator = true;
        }

        public enum ParamType
        {
            Int,
            FloatZeroToPlusOne,
            FloatMinusOneToPlusOne,
        }

        public enum BitType
        {
            _1bit, //1bitで0-1の値を送信
            _2bit, //2bitで0-3の値を送信
            _3bit, //3bitで0-7の値を送信
            _4bit, //4bitで0-15の値を送信
            _5bit, //5bitで0-31の値を送信
            _6bit, //6bitで0-63の値を送信
            _7bit, //7bitで0-127の値を送信
        }

        public syncParamSetting[] syncParamSettings;

        public bool writeDefault = false;

        public override void OnBuildSingle(BuildPhase buildPhase, bool beforeModularAvatar, SamirinMABaseSingle[] _MAScripts, GameObject avatarRootObject, Action<GameObject, SamirinMABaseSingle[]> invokeBuilder, Action<GameObject, SamirinMABaseSingle[]> invokeReplaceBuilder)
        {
            if (buildPhase == BuildPhase.Resolving && beforeModularAvatar)
            {
                invokeBuilder(avatarRootObject, _MAScripts);

                var floatSettings = syncParamSettings.Where(s => IsFloatParamType(s.paramType)).ToArray();
                if (floatSettings.Length > 0)
                {
                    var paramSmoothing = this.gameObject.GetComponent<ParameterSmoothing>();
                    if (paramSmoothing == null)
                        paramSmoothing = this.gameObject.AddComponent<ParameterSmoothing>();

                    var newInfos = floatSettings
                        .Select(s => new ParameterSmoothing.ParameterSmoothingInfo
                        {
                            parameterName = string.IsNullOrEmpty(s.paramName) ? $"Param_{s.paramType}{s.bitType}" : s.paramName,
                            smoothWeight = s.smoothWeight
                        })
                        .ToArray();

                    var existing = paramSmoothing.parameterSmoothingData ?? Array.Empty<ParameterSmoothing.ParameterSmoothingInfo>();
                    var existingNames = new HashSet<string>(existing.Select(x => x.parameterName), StringComparer.Ordinal);
                    var merged = existing.ToList();
                    foreach (var info in newInfos)
                    {
                        if (existingNames.Add(info.parameterName))
                            merged.Add(info);
                    }
                    paramSmoothing.parameterSmoothingData = merged.ToArray();
                }
            }

            if (buildPhase == BuildPhase.Optimizing && beforeModularAvatar)
            {
                invokeReplaceBuilder?.Invoke(avatarRootObject, _MAScripts);
            }
        }

        private static bool IsFloatParamType(HalfSyncParam.ParamType paramType)
        {
            return paramType == HalfSyncParam.ParamType.FloatZeroToPlusOne
                || paramType == HalfSyncParam.ParamType.FloatMinusOneToPlusOne;
        }
    }
}