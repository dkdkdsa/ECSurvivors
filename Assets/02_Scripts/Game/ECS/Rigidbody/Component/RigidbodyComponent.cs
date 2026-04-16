using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct RigidbodyComponent : IComponentData
{
    public float2 velocity;
}