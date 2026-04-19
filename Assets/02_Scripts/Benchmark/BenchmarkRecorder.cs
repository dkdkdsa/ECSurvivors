using System.IO;
using System.Text;
using UnityEngine;

namespace Game.Benchmark
{
    [DefaultExecutionOrder(9999)]
    public class BenchmarkRecorder : MonoBehaviour
    {
        public enum CountCategory { Total, Enemy, Bullet, Exp, Player }

        [SerializeField] private int[] targetObjectCounts = new int[] { 5, 10, 50, 100, 500, 1000, 2000, 5000 };
        [SerializeField] private CountCategory category = CountCategory.Total;
        [SerializeField] private float sampleDurationPerBucket = 2f;
        [SerializeField] private float perBucketTimeoutSeconds = 300f;
        [SerializeField] private string csvFileName = "benchmark_buckets.csv";
        [SerializeField] private bool appendMode = false;
        [SerializeField] private bool writeHeader = true;
        [SerializeField] private bool quitOnFinish = false;
        [SerializeField] private MonoBehaviour countProviderBehaviour;

        private IBenchmarkCountProvider _provider;

        private int _currentBucketIdx;
        private bool _sampling;
        private float _waitElapsed;
        private float _sampleElapsed;
        private double _frameTimeSum;
        private int _sampleFrames;
        private string _csvPath;
        private bool _finalized;

        private void Start()
        {
            _provider = countProviderBehaviour as IBenchmarkCountProvider;
            if (_provider == null) _provider = GetComponent<IBenchmarkCountProvider>();
            if (_provider == null)
            {
                enabled = false;
                return;
            }

            if (targetObjectCounts == null || targetObjectCounts.Length == 0)
            {
                enabled = false;
                return;
            }

            System.Array.Sort(targetObjectCounts);

            _csvPath = Path.Combine(Application.persistentDataPath, csvFileName);

            bool fileExists = File.Exists(_csvPath);
            if (!appendMode)
            {
                if (fileExists) File.Delete(_csvPath);
                fileExists = false;
            }

            if (writeHeader && !fileExists)
            {
                using (var sw = new StreamWriter(_csvPath, append: true, Encoding.UTF8))
                    sw.WriteLine("objectCount,frame_ms,frame_rate");
            }

            Debug.Log(
                $"[BucketRecorder] Category={category}, buckets=[{string.Join(",", targetObjectCounts)}], " +
                $"samplePerBucket={sampleDurationPerBucket}s");
            Debug.Log($"[BucketRecorder] CSV: {_csvPath}");
        }

        private void Update()
        {
            if (_finalized) return;
            if (_currentBucketIdx >= targetObjectCounts.Length)
            {
                Finish();
                return;
            }

            int current = GetCount();
            int target = targetObjectCounts[_currentBucketIdx];

            if (!_sampling)
            {
                _waitElapsed += Time.unscaledDeltaTime;

                if (current >= target)
                {
                    _sampling = true;
                    _sampleElapsed = 0f;
                    _frameTimeSum = 0.0;
                    _sampleFrames = 0;
                    Debug.Log($"[BucketRecorder] bucket={target} 도달 (current={current}), 샘플링 시작");
                }
                else if (_waitElapsed >= perBucketTimeoutSeconds)
                {
                    Debug.LogWarning(
                        $"[BucketRecorder] bucket={target} 타임아웃 ({perBucketTimeoutSeconds}s). " +
                        $"현재 카운트={current}. 스킵하고 다음 버킷으로.");
                    _waitElapsed = 0f;
                    _currentBucketIdx++;
                }
                return;
            }

            _sampleElapsed += Time.unscaledDeltaTime;
            _frameTimeSum += Time.unscaledDeltaTime;
            _sampleFrames++;

            if (_sampleElapsed >= sampleDurationPerBucket)
            {
                if (_sampleFrames == 0 || _frameTimeSum <= 0.0)
                {
                    Debug.LogWarning($"[BucketRecorder] bucket={target} 샘플이 유효하지 않음. 스킵.");
                }
                else
                {
                    double avgFrameTimeSec = _frameTimeSum / _sampleFrames;
                    double avgFrameMs = avgFrameTimeSec * 1000.0;
                    double fps = 1.0 / avgFrameTimeSec;
                    WriteRow(target, (float)avgFrameMs, (float)fps);

                    Debug.Log(
                        $"[BucketRecorder] bucket={target} frame={avgFrameMs:F3}ms fps={fps:F2} " +
                        $"(frames={_sampleFrames})");
                }

                _sampling = false;
                _waitElapsed = 0f;
                _currentBucketIdx++;
            }
        }

        private int GetCount()
        {
            switch (category)
            {
                case CountCategory.Enemy: return _provider.GetEnemyCount();
                case CountCategory.Bullet: return _provider.GetBulletCount();
                case CountCategory.Exp: return _provider.GetExpCount();
                case CountCategory.Player: return _provider.GetPlayerCount();
                case CountCategory.Total:
                default:
                    return _provider.GetEnemyCount()
                         + _provider.GetBulletCount()
                         + _provider.GetExpCount()
                         + _provider.GetPlayerCount();
            }
        }

        private void WriteRow(int bucket, float frameMs, float fps)
        {
            using (var sw = new StreamWriter(_csvPath, append: true, Encoding.UTF8))
                sw.WriteLine($"{bucket},{frameMs:F3},{fps:F2}");
        }

        private void Finish()
        {
            _finalized = true;
            Debug.Log($"[BucketRecorder] 모든 버킷 측정 완료. CSV: {_csvPath}");

            if (quitOnFinish)
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            }
        }
    }
}