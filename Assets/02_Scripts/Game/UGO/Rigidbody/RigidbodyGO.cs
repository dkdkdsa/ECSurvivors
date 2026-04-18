using UnityEngine;

namespace Game.UGO
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BaseRotationGO))]
    public class RigidbodyGO : MonoBehaviour
    {
        public Vector3 initialVelocity;

        [System.NonSerialized] public Vector3 velocity;

        [System.NonSerialized] public BaseRotationGO baseRotation;

        private void Awake()
        {
            velocity = initialVelocity;
            baseRotation = GetComponent<BaseRotationGO>();
        }

        private void OnEnable()
        {
            RigidbodySystemGO.Instance.Register(this);
        }

        private void OnDisable()
        {
            if (RigidbodySystemGO.HasInstance)
                RigidbodySystemGO.Instance.Unregister(this);
        }
    }
}