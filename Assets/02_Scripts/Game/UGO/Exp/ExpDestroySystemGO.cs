using System.Collections.Generic;
using UnityEngine;

namespace Game.UGO
{
    [DefaultExecutionOrder(SystemOrderGO.EXP_DESTROY)]
    public class ExpDestroySystemGO : MonoBehaviour
    {
        private static ExpDestroySystemGO _instance;
        public static bool HasInstance => _instance != null;
        public static ExpDestroySystemGO Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject(nameof(ExpDestroySystemGO));
                    _instance = go.AddComponent<ExpDestroySystemGO>();
                }
                return _instance;
            }
        }

        private readonly List<ExpGO> _items = new List<ExpGO>(4096);
        private readonly Dictionary<ExpGO, int> _indexOf = new Dictionary<ExpGO, int>(4096);

        private void Awake()
        {
            if (_instance != null && _instance != this) { Destroy(gameObject); return; }
            _instance = this;
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }

        public void Register(ExpGO exp)
        {
            _indexOf[exp] = _items.Count;
            _items.Add(exp);
        }

        public void Unregister(ExpGO exp)
        {
            if (!_indexOf.TryGetValue(exp, out int idx)) return;
            int last = _items.Count - 1;
            if (idx != last)
            {
                var tail = _items[last];
                _items[idx] = tail;
                _indexOf[tail] = idx;
            }
            _items.RemoveAt(last);
            _indexOf.Remove(exp);
        }

        private void FixedUpdate()
        {
            for (int i = _items.Count - 1; i >= 0; i--)
            {
                var exp = _items[i];
                var collider = exp.cachedCollider;
                if (collider == null) continue;

                var events = collider.events;
                bool consumed = false;

                for (int j = 0; j < events.Count; j++)
                {
                    var evt = events[j];
                    if (evt.other == null) continue;
                    if (evt.other.cachedPlayerTag == null) continue;

                    PlayerInfoHolderGO.AddExp(1);
                    consumed = true;
                    break;
                }

                if (consumed)
                {
                    if (DropSystemGO.HasInstance)
                        DropSystemGO.Instance.Release(exp);
                    else
                        exp.gameObject.SetActive(false);
                }
            }
        }
    }
}