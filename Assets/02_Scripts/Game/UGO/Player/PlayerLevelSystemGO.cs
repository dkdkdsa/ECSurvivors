using UnityEngine;

namespace Game.UGO
{
    [DefaultExecutionOrder(SystemOrderGO.PLAYER_LEVEL)]
    public class PlayerLevelSystemGO : MonoBehaviour
    {
        private static PlayerLevelSystemGO _instance;
        public static bool HasInstance => _instance != null;
        public static PlayerLevelSystemGO Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject(nameof(PlayerLevelSystemGO));
                    _instance = go.AddComponent<PlayerLevelSystemGO>();
                }
                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this) { Destroy(gameObject); return; }
            _instance = this;
        }

        private void Start()
        {
            PlayerInfoHolderGO.EnsureInitialized();
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }
    }
}
