using Unity.Entities;
using UnityEngine;
using Random = Unity.Mathematics.Random;

public struct EnemySpawner : IComponentData
{
    public Entity prefab;
    public float radius;
    public float tick;
    public float timer;
    public int spawnPerTick;
    public Random random;
}