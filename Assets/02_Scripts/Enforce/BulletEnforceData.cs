using UnityEngine;

public enum EnforceType
{
    BulletSize,
}

public struct BulletEnforceData
{
    public EnforceType type;
    public float value;
}