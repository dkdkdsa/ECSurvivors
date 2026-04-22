using System.Collections.Generic;
using UnityEngine;

namespace Game.UGO
{
    [DefaultExecutionOrder(SystemOrderGO.ENEMY_ATTACK)]
    public class EnemyAttackSystemGO : MonoBehaviour
    {
        public const float AttackDamage = 1f;

        private static EnemyAttackSystemGO _instance;
        public static bool HasInstance => _instance != null;
        public static EnemyAttackSystemGO Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject(nameof(EnemyAttackSystemGO));
                    _instance = go.AddComponent<EnemyAttackSystemGO>();
                }
                return _instance;
            }
        }

        private readonly List<EnemyGO> _enemies = new List<EnemyGO>(2048);
        private readonly Dictionary<EnemyGO, int> _indexOf = new Dictionary<EnemyGO, int>(2048);

        private void Awake()
        {
            if (_instance != null && _instance != this) { Destroy(gameObject); return; }
            _instance = this;
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }

        public void Register(EnemyGO enemy)
        {
            _indexOf[enemy] = _enemies.Count;
            _enemies.Add(enemy);
        }

        public void Unregister(EnemyGO enemy)
        {
            if (!_indexOf.TryGetValue(enemy, out int idx)) return;
            int last = _enemies.Count - 1;
            if (idx != last)
            {
                var tail = _enemies[last];
                _enemies[idx] = tail;
                _indexOf[tail] = idx;
            }
            _enemies.RemoveAt(last);
            _indexOf.Remove(enemy);
        }

        public void CollectTransformsInto(List<Transform> dst)
        {
            for (int i = 0; i < _enemies.Count; i++)
                dst.Add(_enemies[i].transform);
        }

        private void FixedUpdate()
        {
            for (int i = 0; i < _enemies.Count; i++)
            {
                var enemy = _enemies[i];
                var collider = enemy.cachedCollider;
                if (collider == null) continue;

                var events = collider.events;
                for (int j = 0; j < events.Count; j++)
                {
                    var evt = events[j];
                    if (evt.isTrigger) continue;
                    if (evt.other == null) continue;

                    if (evt.other.cachedPlayerTag == null) continue;
                    var unit = evt.other.cachedUnit;
                    if (unit == null) continue;

                    unit.damageQueue.Add(new DamageEventGO { amount = AttackDamage });
                }
            }
        }
    }
}