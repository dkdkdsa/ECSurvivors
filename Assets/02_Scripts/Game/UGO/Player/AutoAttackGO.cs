using Game.ECS;
using UnityEngine;

namespace Game.UGO
{
    [DisallowMultipleComponent]
    public class AutoAttackGO : MonoBehaviour
    {
        public GameObject prefab;
        public float radius;
        public float interval;
        public BulletSetup setup;

        [System.NonSerialized] public float currentInterval;

        private void Awake()
        {
            currentInterval = interval;
        }

        private void OnEnable()
        {
            currentInterval = interval;
            AutoAttackSystemGO.Instance.Register(this);
        }

        private void OnDisable()
        {
            if (AutoAttackSystemGO.HasInstance)
                AutoAttackSystemGO.Instance.Unregister(this);
        }
    }
}
