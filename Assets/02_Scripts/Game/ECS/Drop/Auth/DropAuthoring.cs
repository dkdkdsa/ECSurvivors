using Unity.Entities;
using UnityEngine;

public class DropAuthoring : MonoBehaviour
{
    public int dropCount;
    public GameObject prefab;

    public class Baker : Baker<DropAuthoring>
    {
        public override void Bake(DropAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent<DropTable>(entity, new DropTable
            {
                dropCount = authoring.dropCount,
                prefab = GetEntity(authoring.prefab, TransformUsageFlags.Dynamic)
            });
        }
    }
}
