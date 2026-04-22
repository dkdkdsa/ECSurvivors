using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace Game.ECS
{
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(RigidbodySystem))]
    [BurstCompile]
    public partial struct CollisionSystem : ISystem
    {
        private EntityQuery _colliderQuery;
        private const float CellSize = 2f;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            _colliderQuery = SystemAPI.QueryBuilder()
                .WithAll<BoxColliderComponent, LocalTransform>()
                .Build();

            state.RequireForUpdate(_colliderQuery);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            int count = _colliderQuery.CalculateEntityCount();
            if (count == 0) return;

            var alloc = state.WorldUpdateAllocator;

            var entities = _colliderQuery.ToEntityArray(alloc);
            var colliders = _colliderQuery.ToComponentDataArray<BoxColliderComponent>(alloc);
            var transforms = _colliderQuery.ToComponentDataArray<LocalTransform>(alloc);

            var grid = new NativeParallelMultiHashMap<int2, int>(count * 4, alloc);

            var buildJob = new BuildGridJob
            {
                transforms = transforms,
                colliders = colliders,
                cellSize = CellSize,
                grid = grid.AsParallelWriter()
            }.Schedule(count, 64, state.Dependency);

            var detectJob = new DetectAndResolveJob
            {
                entities = entities,
                colliders = colliders,
                transforms = transforms,
                grid = grid,
                cellSize = CellSize
            };

            state.Dependency = detectJob.ScheduleParallel(buildJob);
        }

        [BurstCompile]
        public partial struct DetectAndResolveJob : IJobEntity
        {
            [ReadOnly] public NativeArray<Entity> entities;
            [ReadOnly] public NativeArray<BoxColliderComponent> colliders;
            [ReadOnly] public NativeArray<LocalTransform> transforms;
            [ReadOnly] public NativeParallelMultiHashMap<int2, int> grid;
            public float cellSize;

             public void Execute(
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

                int2 minCell = (int2)math.floor(aMin / cellSize);
                int2 maxCell = (int2)math.floor(aMax / cellSize);

                for (int cy = minCell.y; cy <= maxCell.y; cy++)
                {
                    for (int cx = minCell.x; cx <= maxCell.x; cx++)
                    {
                        var cell = new int2(cx, cy);
                        if (!grid.TryGetFirstValue(cell, out int i, out var it)) continue;

                        do
                        {
                            if (entities[i] == entity) continue;

                            var otherCollider = colliders[i];
                            float2 bCenter = transforms[i].Position.xy + otherCollider.offset;
                            float2 bHalf = otherCollider.size * 0.5f;

                            int2 bMinCell = (int2)math.floor((bCenter - bHalf) / cellSize);
                            int2 overlapMin = math.max(minCell, bMinCell);
                            if (cx != overlapMin.x || cy != overlapMin.y) continue;

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

                            bool isTriggerEvent = collider.isTrigger || otherCollider.isTrigger;

                            buffer.Add(new CollisionEvent
                            {
                                other = entities[i],
                                normal = normal,
                                penetration = penetration,
                                isTrigger = isTriggerEvent
                            });

                            if (!collider.isStatic && !isTriggerEvent)
                            {
                                float pushRatio = otherCollider.isStatic ? 1f : 0.5f;
                                pushAccum += normal * penetration * pushRatio;
                            }

                        } while (grid.TryGetNextValue(out i, ref it));
                    }
                }

                transform.Position += new float3(pushAccum, 0f);
            }
        }

        [BurstCompile]
        public struct BuildGridJob : IJobParallelFor
        {
            [ReadOnly] public NativeArray<LocalTransform> transforms;
            [ReadOnly] public NativeArray<BoxColliderComponent> colliders;
            public float cellSize;
            public NativeParallelMultiHashMap<int2, int>.ParallelWriter grid;

            public void Execute(int index)
            {
                float2 center = transforms[index].Position.xy + colliders[index].offset;
                float2 half = colliders[index].size * 0.5f;

                int2 min = (int2)math.floor((center - half) / cellSize);
                int2 max = (int2)math.floor((center + half) / cellSize);

                for (int y = min.y; y <= max.y; y++)
                {
                    for (int x = min.x; x <= max.x; x++)
                    {
                        grid.Add(new int2(x, y), index);
                    }
                }
            }
        }
    }
}