using UnityEngine;

namespace Game.UGO
{
    [DisallowMultipleComponent]
    public class PlayerGO : MonoBehaviour
    {
        [HideInInspector] public RigidbodyGO    cachedRigid;
        [HideInInspector] public UnitGO         cachedUnit;
        [HideInInspector] public BaseRotationGO cachedBaseRotation;

        private void Awake()
        {
            cachedRigid        = GetComponent<RigidbodyGO>();
            cachedUnit         = GetComponent<UnitGO>();
            cachedBaseRotation = GetComponent<BaseRotationGO>();
        }

        private void OnEnable()
        {
            PlayerSystemGO.Instance.Register(this);
        }

        private void OnDisable()
        {
            if (PlayerSystemGO.HasInstance)
                PlayerSystemGO.Instance.Unregister(this);
        }
    }
}
