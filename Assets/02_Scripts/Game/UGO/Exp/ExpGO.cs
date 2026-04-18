using UnityEngine;

namespace Game.UGO
{
    [DisallowMultipleComponent]
    public class ExpGO : MonoBehaviour
    {
        // DropSystemGO 가 풀 키로 사용
        [System.NonSerialized] public GameObject sourcePrefab;

        [System.NonSerialized] public RigidbodyGO   cachedRigid;
        [System.NonSerialized] public BoxColliderGO cachedCollider;

        private void Awake()
        {
            cachedRigid    = GetComponent<RigidbodyGO>();
            cachedCollider = GetComponent<BoxColliderGO>();
        }

        private void OnEnable()
        {
            ExpAttractSystemGO.Instance.Register(this);
            ExpDestroySystemGO.Instance.Register(this);
        }

        private void OnDisable()
        {
            if (ExpAttractSystemGO.HasInstance) ExpAttractSystemGO.Instance.Unregister(this);
            if (ExpDestroySystemGO.HasInstance) ExpDestroySystemGO.Instance.Unregister(this);
        }
    }
}
