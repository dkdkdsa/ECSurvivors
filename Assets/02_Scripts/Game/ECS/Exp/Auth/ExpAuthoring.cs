using Unity.Entities;
using UnityEngine;

namespace Game.ECS
{
    public class ExpAuthoring : MonoBehaviour
    {
        public class Baker : Baker<ExpAuthoring>
        {
            public override void Bake(ExpAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<ExpTag>(entity);
            }
        }
    }
}