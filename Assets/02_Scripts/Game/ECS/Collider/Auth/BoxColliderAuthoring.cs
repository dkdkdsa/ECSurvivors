using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class BoxColliderAuthoring : MonoBehaviour
{
    public Vector2 size = Vector2.one;
    public Vector2 offset = Vector2.zero;
    public bool isStatic;

    public class Baker : Baker<BoxColliderAuthoring>
    {
        public override void Bake(BoxColliderAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new BoxColliderComponent
            {
                size = new float2(authoring.size.x, authoring.size.y),
                offset = new float2(authoring.offset.x, authoring.offset.y),
                isStatic = authoring.isStatic
            });

            AddBuffer<CollisionEvent>(entity);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = isStatic ? new Color(0.3f, 0.8f, 1f, 0.8f)
                                 : new Color(0.3f, 1f, 0.3f, 0.8f);

        var center = transform.position + (Vector3)(Vector2)offset;
        var size3 = new Vector3(size.x, size.y, 0.01f);
        Gizmos.DrawWireCube(center, size3);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        var center = transform.position + (Vector3)(Vector2)offset;
        var size3 = new Vector3(size.x, size.y, 0.01f);
        Gizmos.DrawWireCube(center, size3);
    }
}