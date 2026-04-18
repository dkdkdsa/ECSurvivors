using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Game.ECS
{
    public struct BaseRotation : IComponentData
    {
        public quaternion Value;
    }


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

                AddComponent(entity, new BaseRotation
                {
                    Value = quaternion.identity,
                });
            }
        }
    }
}