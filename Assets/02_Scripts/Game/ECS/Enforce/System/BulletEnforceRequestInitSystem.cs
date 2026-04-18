using Unity.Entities;

namespace Game.ECS
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct BulletEnforceRequestInitSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            if (!SystemAPI.TryGetSingletonEntity<BulletEnforceRequest>(out _))
            {
                Entity entity = state.EntityManager.CreateEntity(typeof(BulletEnforceRequest));
                state.EntityManager.SetName(entity, "BulletEnforceRequest");
                state.EntityManager.SetComponentData(entity, new BulletEnforceRequest
                {
                    pending = 0
                });
            }
        }
    }
}