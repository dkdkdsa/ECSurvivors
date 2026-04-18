using UnityEngine;

namespace Game.UGO
{

    [DefaultExecutionOrder(SystemOrderGO.INPUT - 100)]
    public class GameObjectBootstrap : MonoBehaviour
    {
        [SerializeField] private bool _dontDestroyOnLoad = false;

        private void Awake()
        {
            _ = InputSystemGO.Instance;
            _ = PlayerSystemGO.Instance;
            _ = RigidbodySystemGO.Instance;
            _ = CollisionSystemGO.Instance;
            _ = BulletSystemGO.Instance;
            _ = EnemyAttackSystemGO.Instance;
            _ = ExpDestroySystemGO.Instance;
            _ = AutoAttackSystemGO.Instance;
            _ = EnemyMoveSystemGO.Instance;
            _ = ExpAttractSystemGO.Instance;
            _ = HPSystemGO.Instance;
            _ = DropSystemGO.Instance;
            _ = EnemySpawnSystemGO.Instance;
            _ = FlipSystemGO.Instance;
            _ = EnemyAnimationSystemGO.Instance;
            _ = PlayerLevelSystemGO.Instance;
            _ = AttackEnforceSystemGO.Instance;

            if (_dontDestroyOnLoad)
                DontDestroyOnLoad(gameObject);
        }
    }
}
