using UnityEngine;

namespace Game.Benchmark
{
    [DefaultExecutionOrder(9999)]
    public class BenchmarkHUD : MonoBehaviour
    {
        [SerializeField] private MonoBehaviour countProviderBehaviour;
        [SerializeField] private int fontSize = 20;
        [SerializeField] private Vector2 margin = new Vector2(16f, 16f);
        [SerializeField] private Vector2 size = new Vector2(320f, 180f);
        [SerializeField] private float fpsSmoothing = 0.1f;
        [SerializeField] private Color textColor = Color.white;
        [SerializeField] private Color backgroundColor = new Color(0f, 0f, 0f, 0.55f);

        private IBenchmarkCountProvider _provider;
        private float _fpsSmoothed;
        private GUIStyle _labelStyle;
        private GUIStyle _boxStyle;
        private Texture2D _bgTex;

        private void Start()
        {
            _provider = countProviderBehaviour as IBenchmarkCountProvider;
            if (_provider == null) _provider = GetComponent<IBenchmarkCountProvider>();
        }

        private void Update()
        {
            float fps = Time.unscaledDeltaTime > 0f ? 1f / Time.unscaledDeltaTime : 0f;
            if (_fpsSmoothed <= 0f) _fpsSmoothed = fps;
            else _fpsSmoothed = Mathf.Lerp(_fpsSmoothed, fps, fpsSmoothing);
        }

        private void OnDestroy()
        {
            if (_bgTex != null) Destroy(_bgTex);
        }

        private void EnsureStyles()
        {
            if (_labelStyle == null)
            {
                _labelStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = fontSize,
                    alignment = TextAnchor.UpperLeft,
                    richText = true
                };
                _labelStyle.normal.textColor = textColor;
            }

            if (_bgTex == null)
            {
                _bgTex = new Texture2D(1, 1);
                _bgTex.SetPixel(0, 0, backgroundColor);
                _bgTex.Apply();
            }

            if (_boxStyle == null)
            {
                _boxStyle = new GUIStyle(GUI.skin.box);
                _boxStyle.normal.background = _bgTex;
            }
        }

        private void OnGUI()
        {
            EnsureStyles();

            float x = margin.x;
            float y = Screen.height - size.y - margin.y;
            var rect = new Rect(x, y, size.x, size.y);

            GUI.Box(rect, GUIContent.none, _boxStyle);

            int enemy = 0, bullet = 0, exp = 0, player = 0;
            if (_provider != null)
            {
                enemy = _provider.GetEnemyCount();
                bullet = _provider.GetBulletCount();
                exp = _provider.GetExpCount();
                player = _provider.GetPlayerCount();
            }
            int total = enemy + bullet + exp + player;

            string text =
                $"FPS: {_fpsSmoothed:F0}\n" +
                $"Enemy:  {enemy}\n" +
                $"Bullet: {bullet}\n" +
                $"Exp:    {exp}\n" +
                $"Player: {player}\n" +
                $"Total:  {total}";

            var inner = new Rect(rect.x + 10f, rect.y + 8f, rect.width - 20f, rect.height - 16f);
            GUI.Label(inner, text, _labelStyle);
        }
    }
}
