using Unity.Entities;
using UnityEngine;

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
