using Unity.Mathematics;
using UnityEngine;

namespace Game.UGO
{
    [DefaultExecutionOrder(SystemOrderGO.INPUT)]
    public class InputSystemGO : MonoBehaviour
    {
        public static float2 Move { get; private set; }

        private static InputSystemGO _instance;
        public static bool HasInstance => _instance != null;
        public static InputSystemGO Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject(nameof(InputSystemGO));
                    _instance = go.AddComponent<InputSystemGO>();
                }
                return _instance;
            }
        }

        private PlayerInputActions _actions;

        private void Awake()
        {
            if (_instance != null && _instance != this) { Destroy(gameObject); return; }
            _instance = this;

            _actions = new PlayerInputActions();
            _actions.Enable();
        }

        private void OnDestroy()
        {
            _actions?.Dispose();
            if (_instance == this) _instance = null;
            Move = default;
        }

        private void Update()
        {
            Vector2 v = _actions.Player.Move.ReadValue<Vector2>();
            Move = new float2(v.x, v.y);
        }
    }
}
