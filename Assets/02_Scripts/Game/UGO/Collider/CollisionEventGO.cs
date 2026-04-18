using Unity.Mathematics;
using UnityEngine;

namespace Game.UGO
{
    public struct CollisionEventGO
    {
        public BoxColliderGO other;
        public float2 normal;
        public float penetration;
        public bool isTrigger;
    }
}
