using System.Collections.Generic;
using UnityEngine;

namespace Game.UGO
{
    [DefaultExecutionOrder(SystemOrderGO.ENFORCE)]
    public class AttackEnforceSystemGO : MonoBehaviour
    {
        private static AttackEnforceSystemGO _instance;
        public static bool HasInstance => _instance != null;
        public static AttackEnforceSystemGO Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject(nameof(AttackEnforceSystemGO));
                    _instance = go.AddComponent<AttackEnforceSystemGO>();
                }
                return _instance;
            }
        }

        private readonly Queue<BulletEnforceData> _pending = new Queue<BulletEnforceData>(8);

        private void Awake()
        {
            if (_instance != null && _instance != this) { Destroy(gameObject); return; }
            _instance = this;
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
            _pending.Clear();
        }

        public void Enqueue(BulletEnforceData data)
        {
            _pending.Enqueue(data);
        }

        private void Update()
        {
            if (_pending.Count == 0) return;
            if (!AutoAttackSystemGO.HasInstance) return;

            var attackers = AutoAttackSystemGO.Instance.GetAttackersForEnforce();
            if (attackers.Count == 0)
            {
                return;
            }

            while (_pending.Count > 0)
            {
                var data = _pending.Dequeue();
                ApplyToAll(attackers, data);
            }
        }

        private static void ApplyToAll(IReadOnlyList<AutoAttackGO> attackers, BulletEnforceData data)
        {
            for (int i = 0; i < attackers.Count; i++)
            {
                var setup = attackers[i].setup;
                switch (data.type)
                {
                    case EnforceType.BulletSize: setup.size += data.value; break;
                    case EnforceType.PenetCount: setup.penetCount += (int)data.value; break;
                    case EnforceType.BulletSpeed: setup.moveSpeed += data.value; break;
                    case EnforceType.Damage: setup.damage += data.value; break;
                    case EnforceType.LifeTime: setup.lifeTime += data.value; break;
                }
                attackers[i].setup = setup;
            }
        }
    }
}