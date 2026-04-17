using UnityEngine;

public enum EnforceType
{
    BulletSize,
    PenetCount,
    BulletSpeed,
    Damage,
    LifeTime,
}

[System.Serializable]
public struct BulletEnforceData
{
    public EnforceType type;
    public float value;
}