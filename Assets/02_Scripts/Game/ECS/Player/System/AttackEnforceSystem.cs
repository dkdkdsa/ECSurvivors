using Unity.Burst;
using Unity.Entities;
using UnityEngine;

[BurstCompile]
public partial struct AttackEnforceSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach(var item in SystemAPI.Query<RefRW<AutoAttack>>())
        {
            foreach(var data in EnforceDataQ.Get())
            {
                switch (data.type)
                {
                    case EnforceType.BulletSize:
                        item.ValueRW.setup.size += data.value;
                        break;
                    case EnforceType.PenetCount:
                        item.ValueRW.setup.penetCount += (int)data.value;
                        break;
                    case EnforceType.BulletSpeed:
                        item.ValueRW.setup.moveSpeed += data.value;
                        break;
                    case EnforceType.Damage:
                        item.ValueRW.setup.damage += data.value;
                        break;
                    case EnforceType.LifeTime:
                        item.ValueRW.setup.lifeTime += data.value;
                        break;
                }
            }
        }

        EnforceDataQ.Clear();
    }
}