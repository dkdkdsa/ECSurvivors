using UnityEngine;

namespace Game.UGO
{

    [DisallowMultipleComponent]
    public class EnemyGO : MonoBehaviour
    {
        [HideInInspector] public GameObject sourcePrefab;
        [HideInInspector] public RigidbodyGO    cachedRigid;
        [HideInInspector] public UnitGO         cachedUnit;
        [HideInInspector] public BoxColliderGO  cachedCollider;
        [HideInInspector] public BaseRotationGO cachedBaseRotation;
        [HideInInspector] public DropTableGO    cachedDropTable;

        private void Awake()
        {
            cachedRigid        = GetComponent<RigidbodyGO>();
            cachedUnit         = GetComponent<UnitGO>();
            cachedCollider     = GetComponent<BoxColliderGO>();
            cachedBaseRotation = GetComponent<BaseRotationGO>();
            cachedDropTable    = GetComponent<DropTableGO>();
        }

        private void OnEnable()
        {
            EnemyMoveSystemGO.Instance.Register(this);
            EnemyAttackSystemGO.Instance.Register(this);
            EnemyAnimationSystemGO.Instance.Register(this);
        }

        private void OnDisable()
        {
            if (EnemyMoveSystemGO.HasInstance)      EnemyMoveSystemGO.Instance.Unregister(this);
            if (EnemyAttackSystemGO.HasInstance)    EnemyAttackSystemGO.Instance.Unregister(this);
            if (EnemyAnimationSystemGO.HasInstance) EnemyAnimationSystemGO.Instance.Unregister(this);
        }
    }
}
