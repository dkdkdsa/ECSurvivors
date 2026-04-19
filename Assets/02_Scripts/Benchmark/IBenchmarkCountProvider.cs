namespace Game.Benchmark
{
    public interface IBenchmarkCountProvider
    {
        int GetEnemyCount();
        int GetBulletCount();
        int GetExpCount();
        int GetPlayerCount();
    }
}
