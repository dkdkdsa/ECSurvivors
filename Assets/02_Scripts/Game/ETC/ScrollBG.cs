using UnityEngine;


public class ScrollBG : MonoBehaviour
{
    [SerializeField] private GameObject _tilePrefab;
    [SerializeField] private Vector2 _tileSize = new Vector2(10f, 10f);
    [SerializeField] private int _viewRadius = 1;
    [SerializeField] private float _zPosition = 0f;

    private Transform[] _tiles;
    private Vector2Int[] _cells;
    private int _gridSize;

    private void Awake()
    {
        _gridSize = _viewRadius * 2 + 1;
        int count = _gridSize * _gridSize;
        _tiles = new Transform[count];
        _cells = new Vector2Int[count];

        int i = 0;
        for (int x = -_viewRadius; x <= _viewRadius; x++)
        {
            for (int y = -_viewRadius; y <= _viewRadius; y++)
            {
                var go = Instantiate(_tilePrefab, transform);
                go.transform.position = new Vector3(x * _tileSize.x, y * _tileSize.y, _zPosition);
                _tiles[i] = go.transform;
                _cells[i] = new Vector2Int(x, y);
                i++;
            }
        }
    }

    public void OnPlayerPosition(Vector3 playerPos)
    {
        int px = Mathf.FloorToInt(playerPos.x / _tileSize.x);
        int py = Mathf.FloorToInt(playerPos.y / _tileSize.y);

        for (int i = 0; i < _tiles.Length; i++)
        {
            var cell = _cells[i];
            int dx = cell.x - px;
            int dy = cell.y - py;

            bool changed = false;
            while (dx >  _viewRadius) { cell.x -= _gridSize; dx -= _gridSize; changed = true; }
            while (dx < -_viewRadius) { cell.x += _gridSize; dx += _gridSize; changed = true; }
            while (dy >  _viewRadius) { cell.y -= _gridSize; dy -= _gridSize; changed = true; }
            while (dy < -_viewRadius) { cell.y += _gridSize; dy += _gridSize; changed = true; }

            if (changed)
            {
                _cells[i] = cell;
                _tiles[i].position = new Vector3(cell.x * _tileSize.x, cell.y * _tileSize.y, _zPosition);
            }
        }
    }
}
