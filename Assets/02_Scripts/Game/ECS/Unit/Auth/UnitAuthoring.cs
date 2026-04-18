using Unity.Entities;
using UnityEngine;

namespace Game.ECS
{
    public class UnitAuthoring : MonoBehaviour
    {
        public float maxHP;
        public float moveSpeed;

        public class Baker : Baker<UnitAuthoring>
        {
            public override void Bake(UnitAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new UnitComponent
                {
                    maxHP = authoring.maxHP,
                    currentHP = authoring.maxHP,
                    moveSpeed = authoring.moveSpeed,
                });

                AddBuffer<DamageEvent>(entity);
            }
        }
    }
}