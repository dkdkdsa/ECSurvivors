using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Game.UGO
{
    [DefaultExecutionOrder(SystemOrderGO.PLAYER)]
    public class PlayerSystemGO : MonoBehaviour
    {
        private static PlayerSystemGO _instance;
        public static bool HasInstance => _instance != null;
        public static PlayerSystemGO Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject(nameof(PlayerSystemGO));
                    _instance = go.AddComponent<PlayerSystemGO>();
                }
                return _instance;
            }
        }

        private readonly List<PlayerGO> _players = new List<PlayerGO>(2);

        public int ActiveCount => _players.Count;

        private void Awake()
        {
            if (_instance != null && _instance != this) { Destroy(gameObject); return; }
            _instance = this;
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
            PlayerPositionHolderGO.Reset();
        }

        public void Register(PlayerGO player)
        {
            _players.Add(player);
            PlayerPositionHolderGO.Set(player.transform.position);
        }

        public void Unregister(PlayerGO player)
        {
            _players.Remove(player);
            if (_players.Count == 0)
                PlayerPositionHolderGO.Reset();
        }

        private void FixedUpdate()
        {
            float2 move = InputSystemGO.Move;

            for (int i = 0; i < _players.Count; i++)
            {
                var player = _players[i];

                var rb = player.cachedRigid;
                var unit = player.cachedUnit;
                var baseRot = player.cachedBaseRotation;

                if (rb != null && unit != null)
                {
                    rb.velocity = new Vector3(move.x, move.y, 0f) * unit.moveSpeed;
                }

                if (baseRot != null)
                {
                    player.transform.rotation = baseRot.baseValue;
                }

                PlayerPositionHolderGO.Set(player.transform.position);
            }
        }
    }
}