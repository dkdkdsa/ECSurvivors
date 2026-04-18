using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;

namespace Game.UGO
{
    [DefaultExecutionOrder(SystemOrderGO.BRIDGE)]
    public class PlayerPositionBridgeGO : MonoBehaviour
    {
        public UnityEvent<Vector3> OnPlayerPositionChanged;

        private bool _hasValue;
        private float3 _last;

        private void Update()
        {
            if (!PlayerPositionHolderGO.HasValue) return;

            float3 current = PlayerPositionHolderGO.Value;

            if (!_hasValue)
            {
                _last = current;
                _hasValue = true;
                OnPlayerPositionChanged?.Invoke(new Vector3(current.x, current.y, current.z));
                return;
            }

            if (math.all(_last == current)) return;

            _last = current;
            OnPlayerPositionChanged?.Invoke(new Vector3(current.x, current.y, current.z));
        }
    }
}