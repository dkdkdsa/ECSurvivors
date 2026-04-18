using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace Game.UGO
{
    [DisallowMultipleComponent]
    public class EnemySpawnerGO : MonoBehaviour
    {
        public GameObject enemyPrefab;
        public float radius;
        public float tick;
        public int spawnPerTick;
        public uint randomSeed;

        [System.NonSerialized] public float timer;
        [System.NonSerialized] public Random random;

        private void Awake()
        {
            timer = 0f;
            random = Random.CreateFromIndex(randomSeed);
        }

        private void OnEnable()
        {
            EnemySpawnSystemGO.Instance.Register(this);
        }

        private void OnDisable()
        {
            if (EnemySpawnSystemGO.HasInstance)
                EnemySpawnSystemGO.Instance.Unregister(this);
        }
    }
}
