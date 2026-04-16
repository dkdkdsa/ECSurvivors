using Unity.Entities;
using UnityEngine;

public class RigidbodyAuthoring : MonoBehaviour
{
    public Vector3 initialVelocity;

    public class RigidbodyBaker : Baker<RigidbodyAuthoring>
    {
        public override void Bake(RigidbodyAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new RigidbodyComponent
            {
                velocity = authoring.initialVelocity,
            });
        }
    }
}