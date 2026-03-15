using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Samirin33.NDMF.Base;
using Samirin33.NDMF.Module;
using UnityEditor;

namespace Samirin33.NDMF.Components
{
    [AddComponentMenu("samirin33 VRC/ParameterSmoothing")]
    public class ParameterSmoothing : SamirinMABaseSingle
    {
        [System.Serializable]
        public class ParameterSmoothingInfo
        {
            public string parameterName;
            public float smoothWeight;
        }

        public ParameterSmoothingInfo[] parameterSmoothingData;

        const string FPSCounterGUID = "9b06db4aacbe94745a2bcd84f67103eb";

#if UNITY_EDITOR

        public override void OnBuildSingle(SamirinBuildPhase buildPhase, bool beforeModularAvatar, SamirinMABaseSingle[] _MAScripts, GameObject avatarRootObject, Action<GameObject, SamirinMABaseSingle[]> invokeBuilder, Action<GameObject, SamirinMABaseSingle[]> invokeReplaceBuilder)
        {
            if (buildPhase == SamirinBuildPhase.Resolving && beforeModularAvatar)
            {
                invokeBuilder(avatarRootObject, _MAScripts);

                var fpsCounterPath = AssetDatabase.GUIDToAssetPath(FPSCounterGUID);
                var fpsCounter = !string.IsNullOrEmpty(fpsCounterPath)
                    ? AssetDatabase.LoadAssetAtPath<GameObject>(fpsCounterPath)
                    : null;
                if (fpsCounter == null)
                {
                    Debug.LogError($"FPSCounter not found (GUID: {FPSCounterGUID}, path: {fpsCounterPath})");
                    return;
                }

                var moduleSetter = GetComponent<ModuleSetter>();
                if (moduleSetter == null) moduleSetter = this.gameObject.AddComponent<ModuleSetter>();
                moduleSetter.modulePrefabs = new GameObject[] { fpsCounter };
                Debug.Log("FPSCounter added to module setter");
            }
        }
        
#endif
    }
}