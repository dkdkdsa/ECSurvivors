using Unity.Entities;
using UnityEngine;

namespace Game.ECS
{
    public class BulletAuthoring : MonoBehaviour
    {
        public class Baker : Baker<BulletAuthoring>
        {
            public override void Bake(BulletAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent<BulletComponent>(entity);
            }
        }
    }
}