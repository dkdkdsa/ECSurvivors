using Unity.Entities;
using UnityEngine;

public class RigidbodyAuthoring : MonoBehaviour
{
    public Vector2 initialVelocity;

    public class Baker : Baker<RigidbodyAuthoring>
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