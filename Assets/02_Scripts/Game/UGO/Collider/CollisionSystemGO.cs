using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Game.UGO
{
    [DefaultExecutionOrder(SystemOrderGO.COLLISION)]
    public class CollisionSystemGO : MonoBehaviour
    {
        public const float CellSize = 2f;

        public const int MaxEventsPerCollider = 8;

        private static CollisionSystemGO _instance;
        public static bool HasInstance => _instance != null;
        public static CollisionSystemGO Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject(nameof(CollisionSystemGO));
                    _instance = go.AddComponent<CollisionSystemGO>();
                }
                return _instance;
            }
        }

        private readonly List<BoxColliderGO> _colliders = new List<BoxColliderGO>(2048);

        private NativeList<float3> _positions;
        private NativeList<float2> _sizes;
        private NativeList<float2> _offsets;
        private NativeList<byte> _flags;

        private NativeList<int> _eventCounts;
        private NativeList<CollisionRecord> _events;
        private NativeList<float2> _pushOffsets;

        private NativeParallelMultiHashMap<int2, int> _grid;
        private int _gridCapacity;

        private void Awake()
        {
            if (_instance != null && _instance != this) { Destroy(gameObject); return; }
            _instance = this;

            _positions = new NativeList<float3>(2048, Allocator.Persistent);
            _sizes = new NativeList<float2>(2048, Allocator.Persistent);
            _offsets = new NativeList<float2>(2048, Allocator.Persistent);
            _flags = new NativeList<byte>(2048, Allocator.Persistent);
            _eventCounts = new NativeList<int>(2048, Allocator.Persistent);
            _events = new NativeList<CollisionRecord>(2048 * MaxEventsPerCollider, Allocator.Persistent);
            _pushOffsets = new NativeList<float2>(2048, Allocator.Persistent);

            _gridCapacity = 2048 * 4;
            _grid = new NativeParallelMultiHashMap<int2, int>(_gridCapacity, Allocator.Persistent);
        }

        private void OnDestroy()
        {
            if (_positions.IsCreated) _positions.Dispose();
            if (_sizes.IsCreated) _sizes.Dispose();
            if (_offsets.IsCreated) _offsets.Dispose();
            if (_flags.IsCreated) _flags.Dispose();
            if (_eventCounts.IsCreated) _eventCounts.Dispose();
            if (_events.IsCreated) _events.Dispose();
            if (_pushOffsets.IsCreated) _pushOffsets.Dispose();
            if (_grid.IsCreated) _grid.Dispose();
            if (_instance == this) _instance = null;
        }

        public void Register(BoxColliderGO collider)
        {
            collider.systemIndex = _colliders.Count;
            _colliders.Add(collider);
        }

        public void Unregister(BoxColliderGO collider)
        {
            int last = _colliders.Count - 1;
            int idx = collider.systemIndex;
            if (idx < 0 || idx > last) return;

            _colliders[idx] = _colliders[last];
            _colliders[idx].systemIndex = idx;
            _colliders.RemoveAt(last);
            collider.systemIndex = -1;
        }

        private void FixedUpdate()
        {
            int count = _colliders.Count;
            if (count == 0) return;

            _positions.ResizeUninitialized(count);
            _sizes.ResizeUninitialized(count);
            _offsets.ResizeUninitialized(count);
            _flags.ResizeUninitialized(count);

            for (int i = 0; i < count; i++)
            {
                var c = _colliders[i];
                _positions[i] = c.transform.position;
                _sizes[i] = new float2(c.size.x, c.size.y);
                _offsets[i] = new float2(c.offset.x, c.offset.y);
                byte f = 0;
                if (c.isStatic) f |= 1;
                if (c.isTrigger) f |= 2;
                _flags[i] = f;
            }

            int requiredGridCap = count * 4;
            if (requiredGridCap > _gridCapacity)
            {
                _grid.Dispose();
                _gridCapacity = requiredGridCap;
                _grid = new NativeParallelMultiHashMap<int2, int>(_gridCapacity, Allocator.Persistent);
            }
            else
            {
                _grid.Clear();
            }

            var buildJob = new BuildGridJob
            {
                positions = _positions.AsArray(),
                sizes = _sizes.AsArray(),
                offsets = _offsets.AsArray(),
                cellSize = CellSize,
                grid = _grid.AsParallelWriter()
            }.Schedule(count, 64);

            _eventCounts.Resize(count, NativeArrayOptions.ClearMemory);
            _pushOffsets.Resize(count, NativeArrayOptions.ClearMemory);
            _events.ResizeUninitialized(count * MaxEventsPerCollider);

            var detectJob = new DetectAndResolveJob
            {
                positions = _positions.AsArray(),
                sizes = _sizes.AsArray(),
                offsets = _offsets.AsArray(),
                flags = _flags.AsArray(),
                grid = _grid,
                cellSize = CellSize,
                maxEventsPerCollider = MaxEventsPerCollider,
                eventCounts = _eventCounts.AsArray(),
                events = _events.AsArray(),
                pushOffsets = _pushOffsets.AsArray()
            }.Schedule(count, 32, buildJob);

            detectJob.Complete();

            for (int i = 0; i < count; i++)
            {
                var c = _colliders[i];
                c.events.Clear();

                int n = _eventCounts[i];
                int baseIdx = i * MaxEventsPerCollider;
                for (int j = 0; j < n; j++)
                {
                    var rec = _events[baseIdx + j];
                    int otherIdx = rec.otherIndex;
                    if (otherIdx < 0 || otherIdx >= _colliders.Count) continue;

                    c.events.Add(new CollisionEventGO
                    {
                        other = _colliders[otherIdx],
                        normal = rec.normal,
                        penetration = rec.penetration,
                        isTrigger = rec.isTrigger != 0
                    });
                }

                float2 push = _pushOffsets[i];
                if (math.lengthsq(push) > 0f)
                {
                    var p = c.transform.position;
                    p.x += push.x;
                    p.y += push.y;
                    c.transform.position = p;
                }
            }

        }


        private struct CollisionRecord
        {
            public int otherIndex;
            public float2 normal;
            public float penetration;
            public byte isTrigger;
        }

        [BurstCompile]
        private struct BuildGridJob : IJobParallelFor
        {
            [ReadOnly] public NativeArray<float3> positions;
            [ReadOnly] public NativeArray<float2> sizes;
            [ReadOnly] public NativeArray<float2> offsets;
            public float cellSize;
            public NativeParallelMultiHashMap<int2, int>.ParallelWriter grid;

            public void Execute(int index)
            {
                float2 center = positions[index].xy + offsets[index];
                float2 half = sizes[index] * 0.5f;

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

        [BurstCompile]
        private struct DetectAndResolveJob : IJobParallelFor
        {
            [ReadOnly] public NativeArray<float3> positions;
            [ReadOnly] public NativeArray<float2> sizes;
            [ReadOnly] public NativeArray<float2> offsets;
            [ReadOnly] public NativeArray<byte> flags;
            [ReadOnly] public NativeParallelMultiHashMap<int2, int> grid;
            public float cellSize;
            public int maxEventsPerCollider;

            [NativeDisableParallelForRestriction] public NativeArray<int> eventCounts;
            [NativeDisableParallelForRestriction] public NativeArray<CollisionRecord> events;
            public NativeArray<float2> pushOffsets;

            public void Execute(int index)
            {
                byte myFlag = flags[index];
                bool aIsStatic = (myFlag & 1) != 0;
                bool aIsTrigger = (myFlag & 2) != 0;

                float2 aCenter = positions[index].xy + offsets[index];
                float2 aHalf = sizes[index] * 0.5f;
                float2 aMin = aCenter - aHalf;
                float2 aMax = aCenter + aHalf;

                int2 minCell = (int2)math.floor(aMin / cellSize);
                int2 maxCell = (int2)math.floor(aMax / cellSize);

                float2 pushAccum = float2.zero;
                int writtenCount = 0;
                int baseSlot = index * maxEventsPerCollider;

                for (int cy = minCell.y; cy <= maxCell.y; cy++)
                {
                    for (int cx = minCell.x; cx <= maxCell.x; cx++)
                    {
                        var cell = new int2(cx, cy);
                        if (!grid.TryGetFirstValue(cell, out int j, out var it)) continue;

                        do
                        {
                            if (j == index) continue;

                            float2 bCenter = positions[j].xy + offsets[j];
                            float2 bHalf = sizes[j] * 0.5f;

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

                            byte bFlag = flags[j];
                            bool bIsStatic = (bFlag & 1) != 0;
                            bool bIsTrigger = (bFlag & 2) != 0;
                            bool isTriggerEvent = aIsTrigger || bIsTrigger;

                            if (writtenCount < maxEventsPerCollider)
                            {
                                events[baseSlot + writtenCount] = new CollisionRecord
                                {
                                    otherIndex = j,
                                    normal = normal,
                                    penetration = penetration,
                                    isTrigger = (byte)(isTriggerEvent ? 1 : 0)
                                };
                                writtenCount++;
                            }

                            if (!aIsStatic && !isTriggerEvent)
                            {
                                float pushRatio = bIsStatic ? 1f : 0.5f;
                                pushAccum += normal * penetration * pushRatio;
                            }

                        } while (grid.TryGetNextValue(out j, ref it));
                    }
                }

                eventCounts[index] = writtenCount;
                pushOffsets[index] = pushAccum;
            }
        }
    }
}