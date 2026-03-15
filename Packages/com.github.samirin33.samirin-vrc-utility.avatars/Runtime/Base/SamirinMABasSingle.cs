using System;
using UnityEngine;
using VRC.SDKBase;
using nadena.dev.ndmf;

namespace Samirin33.NDMF.Base
{
    public class SamirinMABaseSingle : SamirinMABase
    {
        public virtual void OnBuildSingle(BuildPhase buildPhase, bool beforeModularAvatar, SamirinMABaseSingle[] _MAScripts, GameObject avatarRootObject, Action<GameObject, SamirinMABaseSingle[]> invokeBuilder, Action<GameObject, SamirinMABaseSingle[]> invokeReplaceBuilder) { }
    }
}