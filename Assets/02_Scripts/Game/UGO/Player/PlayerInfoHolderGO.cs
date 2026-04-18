using System;
using Game.ECS;
using UnityEngine;

namespace Game.UGO
{
    public static class PlayerInfoHolderGO
    {
        private static PlayerInfo _value;
        private static bool _initialized;

        public static event Action<PlayerInfo, PlayerInfo> OnChanged;

        public static PlayerInfo Value => _value;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void ResetStatics()
        {
            _value = new PlayerInfo { needLevelUp = 10 };
            _initialized = false;
            OnChanged = null;
        }

        public static void EnsureInitialized()
        {
            if (_initialized) return;
            _initialized = true;
            OnChanged?.Invoke(_value, _value);
        }

        public static void Reset()
        {
            _value = new PlayerInfo { needLevelUp = 10 };
            _initialized = false;
        }

        public static void AddExp(int amount)
        {
            if (amount == 0) return;

            var prev = _value;
            _value.exp += amount;

            while (_value.exp >= _value.needLevelUp)
            {
                _value.exp -= _value.needLevelUp;
                _value.level += 1;
                _value.needLevelUp *= 2;
            }

            OnChanged?.Invoke(prev, _value);
        }

        internal static void SetForce(PlayerInfo info)
        {
            var prev = _value;
            _value = info;
            OnChanged?.Invoke(prev, _value);
        }
    }
}