using UnityEngine;
using VRC.SDKBase;

namespace Samirin33.NDMF.Base
{
    public class SamirinMABase : MonoBehaviour, IEditorOnly
    {
        public virtual void OnBuildTransformingBeforeMA(GameObject avatarRootObject) { }
        public virtual void OnBuildTransformingAfterMA(GameObject avatarRootObject) { }
        public virtual void OnBuildResolvingBeforeMA(GameObject avatarRootObject) { }
        public virtual void OnBuildResolvingAfterMA(GameObject avatarRootObject) { }
    }
}