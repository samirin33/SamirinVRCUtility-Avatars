using UnityEngine;
using VRC.SDKBase;
using Samirin33.NDMF.Base;

namespace Samirin33.NDMF.Module
{
    [AddComponentMenu("SamirinVRC/ModuleSetter")]
    public class ModuleSetter : SamirinMABase
    {
        public GameObject[] modulePrefabs;
        public override void OnBuildResolvingBeforeMA(GameObject avatarRootObject)
        {
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