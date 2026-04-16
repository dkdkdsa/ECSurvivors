using Unity.Entities;
using UnityEngine;

public class BulletAuthoring : MonoBehaviour
{
    public float damage;
    public float lifeTime;
    public float moveSpeed;

    public class Baker : Baker<BulletAuthoring>
    {
        public override void Bake(BulletAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent<BulletComponent>(entity, new BulletComponent
            {
                damage = authoring.damage,
                lifeTime = authoring.lifeTime,
                moveSpeed = authoring.moveSpeed,
            });
        }
    }
}