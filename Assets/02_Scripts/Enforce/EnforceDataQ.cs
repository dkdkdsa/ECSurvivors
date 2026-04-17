using System.Collections;
using System.Collections.Generic;

public static class EnforceDataQ
{
    private static Queue<BulletEnforceData> _dataQ = new();

    public static void Enqueue(BulletEnforceData data)
    {
        _dataQ.Enqueue(data);
    }

    public static BulletEnforceData Dequeue()
    {
        return _dataQ.Dequeue();
    }

    public static void Clear()
    {
        _dataQ.Clear();
    }

    public static IEnumerable<BulletEnforceData> Get() => _dataQ;
}