using UnityEngine;

namespace Samirin33.AvatarEditer.Tools
{
    public class FPSLimiter : MonoBehaviour
    {
        [Tooltip("目標FPS。0で垂直同期、-1で無制限")]
        [SerializeField] private int targetFrameRate = 90;

        private void Start()
        {
            ApplyFrameRate();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // 再生中にInspectorで値を変更した場合も即反映
            if (Application.isPlaying)
            {
                ApplyFrameRate();
            }
        }
#endif

        private void ApplyFrameRate()
        {
            Application.targetFrameRate = targetFrameRate;
        }

        /// <summary>
        /// 実行時にFPS上限を変更する場合用の公開メソッド
        /// </summary>
        public void SetTargetFrameRate(int fps)
        {
            targetFrameRate = fps;
            ApplyFrameRate();
        }
    }
}