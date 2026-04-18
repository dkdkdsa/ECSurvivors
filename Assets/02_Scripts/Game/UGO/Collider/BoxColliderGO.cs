using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Game.UGO
{
    [DisallowMultipleComponent]
    public class BoxColliderGO : MonoBehaviour
    {
        public Vector2 size = Vector2.one;
        public Vector2 offset = Vector2.zero;
        public bool isStatic;
        public bool isTrigger;

        [HideInInspector] public List<CollisionEventGO> events = new List<CollisionEventGO>(8);
        [HideInInspector] public int systemIndex = -1;
        [HideInInspector] public EnemyGO  cachedEnemyTag;
        [HideInInspector] public PlayerGO cachedPlayerTag;
        [HideInInspector] public UnitGO   cachedUnit;

        public float2 Center => new float2(transform.position.x + offset.x, transform.position.y + offset.y);

        private void Awake()
        {
            cachedEnemyTag  = GetComponent<EnemyGO>();
            cachedPlayerTag = GetComponent<PlayerGO>();
            cachedUnit      = GetComponent<UnitGO>();
        }

        private void OnEnable()
        {
            CollisionSystemGO.Instance.Register(this);
        }

        private void OnDisable()
        {
            if (CollisionSystemGO.HasInstance)
                CollisionSystemGO.Instance.Unregister(this);
        }

        private void OnDrawGizmos()
        {
            if (isTrigger)
                Gizmos.color = new Color(1f, 0.85f, 0.2f, 0.8f);
            else if (isStatic)
                Gizmos.color = new Color(0.3f, 0.8f, 1f, 0.8f);
            else
                Gizmos.color = new Color(0.3f, 1f, 0.3f, 0.8f);

            var center = transform.position + (Vector3)offset;
            var size3 = new Vector3(size.x, size.y, 0.01f);
            Gizmos.DrawWireCube(center, size3);
        }
    }
}
