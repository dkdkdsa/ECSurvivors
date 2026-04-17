using Unity.Entities;
using UnityEngine;

public class AutoAttackAuthoring : MonoBehaviour
{
    public GameObject prefab;
    public float radius;
    public float interval;
    public BulletSetup setup;

    public class Baker : Baker<AutoAttackAuthoring>
    {
        public override void Bake(AutoAttackAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);

            AddComponent<AutoAttack>(entity, new AutoAttack
            {
                prefab = GetEntity(authoring.prefab, TransformUsageFlags.Dynamic),
                attackInterval = authoring.interval,
                currentInterval = authoring.interval,
                setup = authoring.setup,
                radius = authoring.radius,
            });
        }
    }
}