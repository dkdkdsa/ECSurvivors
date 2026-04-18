using Game.ECS;
using UnityEngine;
using UnityEngine.Events;

namespace Game.UGO
{
    [DefaultExecutionOrder(SystemOrderGO.BRIDGE)]
    public class PlayerInfoBridgeGO : MonoBehaviour
    {
        public UnityEvent<PlayerInfo, PlayerInfo> OnPlayerInfoChanged;

        private bool _hasValue;
        private PlayerInfo _last;

        private void OnEnable()
        {
            PlayerInfoHolderGO.OnChanged += HandleChanged;

            _last = PlayerInfoHolderGO.Value;
            _hasValue = true;
            OnPlayerInfoChanged?.Invoke(_last, _last);
        }

        private void OnDisable()
        {
            PlayerInfoHolderGO.OnChanged -= HandleChanged;
        }

        private void HandleChanged(PlayerInfo prev, PlayerInfo current)
        {
            if (!_hasValue)
            {
                _last = current;
                _hasValue = true;
                OnPlayerInfoChanged?.Invoke(current, current);
                return;
            }

            if (_last.level == current.level &&
                _last.exp == current.exp &&
                _last.needLevelUp == current.needLevelUp)
                return;

            var oldInfo = _last;
            _last = current;
            OnPlayerInfoChanged?.Invoke(oldInfo, current);
        }
    }
}