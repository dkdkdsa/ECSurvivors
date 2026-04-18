using System.Collections.Generic;
using UnityEngine;

namespace Game.UGO
{
    [DefaultExecutionOrder(SystemOrderGO.BULLET)]
    public class BulletSystemGO : MonoBehaviour
    {
        private static BulletSystemGO _instance;
        public static bool HasInstance => _instance != null;
        public static BulletSystemGO Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject(nameof(BulletSystemGO));
                    _instance = go.AddComponent<BulletSystemGO>();
                }
                return _instance;
            }
        }

        private readonly List<BulletGO> _bullets = new List<BulletGO>(2048);

        private void Awake()
        {
            if (_instance != null && _instance != this) { Destroy(gameObject); return; }
            _instance = this;
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }

        public void Register(BulletGO bullet) => _bullets.Add(bullet);
        public void Unregister(BulletGO bullet) => _bullets.Remove(bullet);

        private void FixedUpdate()
        {
            float dt = RigidbodySystemGO.FixedDT;

            for (int i = _bullets.Count - 1; i >= 0; i--)
            {
                var bullet = _bullets[i];

                var rb = bullet.cachedRigid;
                if (rb != null) rb.velocity = bullet.dir * bullet.setup.moveSpeed;

                bullet.lifeTime -= dt;
                if (bullet.lifeTime <= 0f)
                {
                    AutoAttackSystemGO.Instance.ReleaseBullet(bullet);
                    continue;
                }

                var collider = bullet.cachedCollider;
                if (collider == null) continue;

                var events = collider.events;
                bool destroyed = false;

                for (int j = 0; j < events.Count; j++)
                {
                    var evt = events[j];
                    if (evt.other == null) continue;

                    var enemyTag = evt.other.cachedEnemyTag;
                    if (enemyTag == null) continue;

                    var unit = enemyTag.cachedUnit;
                    if (unit == null) continue;

                    unit.damageQueue.Add(new DamageEventGO { amount = bullet.setup.damage });

                    if (bullet.setup.penetCount > 0)
                    {
                        bullet.setup.penetCount--;
                    }
                    else
                    {
                        AutoAttackSystemGO.Instance.ReleaseBullet(bullet);
                        destroyed = true;
                        break;
                    }
                }

                if (destroyed) continue;
            }
        }
    }
}
