using UnityEngine;
using VRC.SDKBase;
using nadena.dev.ndmf;

namespace Samirin33.NDMF.Base
{
    public class SamirinMABase : MonoBehaviour, IEditorOnly
    {
        [System.NonSerialized]
        public int priority = 100;
        public virtual void OnBuild(BuildPhase buildPhase, bool beforeModularAvatar, GameObject avatarRootObject) { }
    }
}