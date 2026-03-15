using UnityEngine;
using Samirin33.NDMF.Base;

namespace Samirin33.NDMF.Components
{
    [AddComponentMenu("SamirinVRC/HalfSyncParam")]

    public class HalfSyncParam : SamirinMABase
    {
        [System.Serializable]
        public class syncParamSetting
        {
            public string paramName;
            public ParamType paramType;
            public BitType bitType;
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
    }
}