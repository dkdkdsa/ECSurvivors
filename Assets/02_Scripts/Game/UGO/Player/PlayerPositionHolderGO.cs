using Unity.Mathematics;
using UnityEngine;

namespace Game.UGO
{
    public static class PlayerPositionHolderGO
    {
        private static float3 _value;
        private static bool _hasValue;

        public static float3 Value => _value;
        public static bool HasValue => _hasValue;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void ResetStatics()
        {
            _value = default;
            _hasValue = false;
        }

        public static void Set(float3 value)
        {
            _value = value;
            _hasValue = true;
        }

        public static void Reset()
        {
            _hasValue = false;
            _value = default;
        }
    }
}