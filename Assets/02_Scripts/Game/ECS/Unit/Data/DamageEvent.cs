using Unity.Entities;
using UnityEngine;

namespace Game.ECS
{
    [InternalBufferCapacity(4)]
    public partial struct DamageEvent : IBufferElementData
    {
        public float amount;
    }
}