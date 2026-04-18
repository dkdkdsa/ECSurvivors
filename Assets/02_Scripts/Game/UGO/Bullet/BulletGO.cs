using Game.ECS;
using UnityEngine;

namespace Game.UGO
{
    [DisallowMultipleComponent]
    public class BulletGO : MonoBehaviour
    {
        [HideInInspector] public BulletSetup  setup;
        [HideInInspector] public Vector3      dir;
        [HideInInspector] public float        lifeTime;
        [HideInInspector] public GameObject   sourcePrefab;

        [HideInInspector] public RigidbodyGO   cachedRigid;
        [HideInInspector] public BoxColliderGO cachedCollider;

        private void Awake()
        {
            cachedRigid    = GetComponent<RigidbodyGO>();
            cachedCollider = GetComponent<BoxColliderGO>();
        }

        private void OnEnable()
        {
            BulletSystemGO.Instance.Register(this);
        }

        private void OnDisable()
        {
            if (BulletSystemGO.HasInstance)
                BulletSystemGO.Instance.Unregister(this);
        }
    }
}
