using UnityEngine;
using UnityEngine.Events;

namespace Game.UGO
{

    [DefaultExecutionOrder(SystemOrderGO.BRIDGE)]
    public class PlayerDeadBridgeGO : MonoBehaviour
    {
        public UnityEvent OnPlayerDead;

        private bool _hasSeenPlayer;
        private bool _triggered;

        private void Update()
        {
            if (!PlayerSystemGO.HasInstance) return;

            bool hasPlayer = PlayerPositionHolderGO.HasValue;

            if (hasPlayer)
            {
                _hasSeenPlayer = true;
                return;
            }

            if (!_hasSeenPlayer) return;
            if (_triggered) return;

            _triggered = true;
            Time.timeScale = 0f;
            OnPlayerDead?.Invoke();
        }
    }
}
