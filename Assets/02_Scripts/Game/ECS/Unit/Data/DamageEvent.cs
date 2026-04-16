using Unity.Entities;
using UnityEngine;

[InternalBufferCapacity(4)]
public partial struct DamageEvent : IBufferElementData
{
    public float amount;
}