using System.Collections.Generic;
using UnityEngine;

namespace Game.UGO
{
    [DefaultExecutionOrder(SystemOrderGO.HP)]
    public class HPSystemGO : MonoBehaviour
    {
        private static HPSystemGO _instance;
        public static bool HasInstance => _instance != null;
        public static HPSystemGO Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject(nameof(HPSystemGO));
                    _instance = go.AddComponent<HPSystemGO>();
                }
                return _instance;
            }
        }

        private readonly List<UnitGO> _units = new List<UnitGO>(2048);

        private void Awake()
        {
            if (_instance != null && _instance != this) { Destroy(gameObject); return; }
            _instance = this;
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }

        public void Register(UnitGO unit) => _units.Add(unit);
        public void Unregister(UnitGO unit) => _units.Remove(unit);

        private void Update()
        {
            for (int i = _units.Count - 1; i >= 0; i--)
            {
                var unit = _units[i];

                var queue = unit.damageQueue;
                for (int q = 0; q < queue.Count; q++)
                    unit.currentHP -= queue[q].amount;
                queue.Clear();

                if (unit.currentHP > 0f) continue;

                var enemy = unit.GetComponent<EnemyGO>();
                var dropTable = enemy != null ? enemy.cachedDropTable : unit.GetComponent<DropTableGO>();

                if (dropTable != null)
                {
                    DropSystemGO.Instance.EnqueueDrop(new DropEventGO
                    {
                        prefab = dropTable.prefab,
                        dropCount = dropTable.dropCount,
                        position = unit.transform.position
                    });
                }

                if (enemy != null && EnemySpawnSystemGO.HasInstance)
                    EnemySpawnSystemGO.Instance.Release(enemy);
                else
                    unit.gameObject.SetActive(false);
            }
        }
    }
}
