using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(RigidbodySystem))]
[BurstCompile]
public partial struct CollisionSystem : ISystem
{
    private EntityQuery colliderQuery;

    public void OnCreate(ref SystemState state)
    {
        colliderQuery = SystemAPI.QueryBuilder()
            .WithAll<BoxColliderComponent, LocalTransform>()
            .Build();

        state.RequireForUpdate(colliderQuery);
    }

    public void OnUpdate(ref SystemState state)
    {
        var entities = colliderQuery.ToEntityArray(Allocator.TempJob);
        var colliders = colliderQuery.ToComponentDataArray<BoxColliderComponent>(Allocator.TempJob);
        var transforms = colliderQuery.ToComponentDataArray<LocalTransform>(Allocator.TempJob);

        var job = new DetectAndResolveJob
        {
            entities = entities,
            colliders = colliders,
            transforms = transforms
        };

        state.Dependency = job.ScheduleParallel(state.Dependency);

        entities.Dispose(state.Dependency);
        colliders.Dispose(state.Dependency);
        transforms.Dispose(state.Dependency);
    }

    [BurstCompile]
    public partial struct DetectAndResolveJob : IJobEntity
    {
        [ReadOnly] public NativeArray<Entity> entities;
        [ReadOnly] public NativeArray<BoxColliderComponent> colliders;
        [ReadOnly] public NativeArray<LocalTransform> transforms;

        private void Execute(
            Entity entity,
            ref LocalTransform transform,
            in BoxColliderComponent collider,
            ref DynamicBuffer<CollisionEvent> buffer)
        {
            buffer.Clear();

            float2 aCenter = transform.Position.xy + collider.offset;
            float2 aHalf = collider.size * 0.5f;
            float2 aMin = aCenter - aHalf;
            float2 aMax = aCenter + aHalf;

            float2 pushAccum = float2.zero;

            for (int i = 0; i < entities.Length; i++)
            {
                if (entities[i] == entity) continue;

                var otherCollider = colliders[i];
                var otherTransform = transforms[i];

                float2 bCenter = otherTransform.Position.xy + otherCollider.offset;
                float2 bHalf = otherCollider.size * 0.5f;
                float2 bMin = bCenter - bHalf;
                float2 bMax = bCenter + bHalf;

                if (aMax.x <= bMin.x || aMin.x >= bMax.x) continue;
                if (aMax.y <= bMin.y || aMin.y >= bMax.y) continue;

                float overlapX = math.min(aMax.x, bMax.x) - math.max(aMin.x, bMin.x);
                float overlapY = math.min(aMax.y, bMax.y) - math.max(aMin.y, bMin.y);

                float2 normal;
                float penetration;

                if (overlapX < overlapY)
                {
                    float dir = aCenter.x < bCenter.x ? -1f : 1f;
                    normal = new float2(dir, 0f);
                    penetration = overlapX;
                }
                else
                {
                    float dir = aCenter.y < bCenter.y ? -1f : 1f;
                    normal = new float2(0f, dir);
                    penetration = overlapY;
                }

                buffer.Add(new CollisionEvent
                {
                    other = entities[i],
                    normal = normal,
                    penetration = penetration
                });

                if (!collider.isStatic)
                {
                    float pushRatio = otherCollider.isStatic ? 1f : 0.5f;
                    pushAccum += normal * penetration * pushRatio;
                }
            }

            transform.Position += new float3(pushAccum, 0f);
        }
    }
}