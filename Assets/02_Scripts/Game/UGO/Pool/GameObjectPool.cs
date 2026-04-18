using System.Collections.Generic;
using UnityEngine;

namespace Game.UGO
{
    public class GameObjectPool<T> where T : MonoBehaviour
    {
        private readonly T _prefab;
        private readonly Transform _parent;
        private readonly Stack<T> _idle;
        private readonly List<T> _active;

        public IReadOnlyList<T> Active => _active;
        public int ActiveCount => _active.Count;

        public GameObjectPool(T prefab, Transform parent, int capacity = 64)
        {
            _prefab = prefab;
            _parent = parent;
            _idle = new Stack<T>(capacity);
            _active = new List<T>(capacity);
        }

        public void Prewarm(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var instance = Object.Instantiate(_prefab, _parent);
                instance.gameObject.SetActive(false);
                _idle.Push(instance);
            }
        }

        public T Get(Vector3 position, Quaternion rotation)
        {
            T instance;
            if (_idle.Count > 0)
            {
                instance = _idle.Pop();
                instance.transform.SetPositionAndRotation(position, rotation);
            }
            else
            {
                instance = Object.Instantiate(_prefab, position, rotation, _parent);
            }

            instance.gameObject.SetActive(true);
            _active.Add(instance);
            return instance;
        }

        public void Release(T instance)
        {
            int last = _active.Count - 1;
            int index = _active.IndexOf(instance);
            if (index < 0) return;

            _active[index] = _active[last];
            _active.RemoveAt(last);

            instance.gameObject.SetActive(false);
            _idle.Push(instance);
        }

        public void ReleaseAt(int index)
        {
            int last = _active.Count - 1;
            var instance = _active[index];

            _active[index] = _active[last];
            _active.RemoveAt(last);

            instance.gameObject.SetActive(false);
            _idle.Push(instance);
        }

        public void Clear()
        {
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                _active[i].gameObject.SetActive(false);
                _idle.Push(_active[i]);
            }
            _active.Clear();
        }
    }
}
