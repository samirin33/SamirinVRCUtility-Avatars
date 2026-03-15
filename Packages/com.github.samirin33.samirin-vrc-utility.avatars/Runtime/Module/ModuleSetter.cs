using UnityEngine;
using VRC.SDKBase;
using nadena.dev.ndmf;
using Samirin33.NDMF.Base;

namespace Samirin33.NDMF.Module
{
    [AddComponentMenu("samirin33 VRC/ModuleSetter"), DisallowMultipleComponent]
    public class ModuleSetter : SamirinMABase
    {
        void Reset()
        {
            priority = 300;
        }
        public GameObject[] modulePrefabs;
        public override void OnBuild(BuildPhase buildPhase, bool beforeModularAvatar, GameObject avatarRootObject)
        {
            if (buildPhase != BuildPhase.Transforming || !beforeModularAvatar) return;

            var avatarObject = avatarRootObject;
            if (modulePrefabs == null) return;

            foreach (var prefab in modulePrefabs)
            {
                if (prefab == null) continue;
                if (!ContainsModuleInstance(avatarObject, prefab))
                {
                    var instance = Object.Instantiate(prefab, avatarObject.transform);
                    instance.name = prefab.name;
                }
            }
        }

        private static bool ContainsModuleInstance(GameObject avatarObject, GameObject prefab)
        {
            var prefabName = prefab.name;
            foreach (Transform child in avatarObject.transform)
            {
                if (child.name == prefabName || child.name.StartsWith(prefabName + " "))
                    return true;
            }
            return false;
        }
    }
}