using Game.UGO;
using UnityEngine;

namespace Game.Benchmark
{
    [DisallowMultipleComponent]
    public class BenchmarkCountProviderGO : MonoBehaviour, IBenchmarkCountProvider
    {
        public int GetEnemyCount()  => EnemyMoveSystemGO.HasInstance  ? EnemyMoveSystemGO.Instance.ActiveCount  : 0;
        public int GetBulletCount() => BulletSystemGO.HasInstance     ? BulletSystemGO.Instance.ActiveCount     : 0;
        public int GetExpCount()    => ExpAttractSystemGO.HasInstance ? ExpAttractSystemGO.Instance.ActiveCount : 0;
        public int GetPlayerCount() => PlayerSystemGO.HasInstance     ? PlayerSystemGO.Instance.ActiveCount     : 0;
    }
}
