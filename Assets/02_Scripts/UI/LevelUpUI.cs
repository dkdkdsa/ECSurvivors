using Game.ECS;
using Unity.Entities;
using UnityEngine;

namespace Game.UI
{
    public class LevelUpUI : MonoBehaviour
    {
        [SerializeField] private LevelUpUIData[] _datas;
        [SerializeField] private LevelUpUIPanel _prefab;
        [SerializeField] private Transform _panelRoot;

        private EntityManager _entityManager;
        private Entity _requestEntity;

        private void Awake()
        {
            _requestEntity = Entity.Null;
            TryResolveRequestEntity();
        }

        public void OnPlayerInfoChanged(PlayerInfo oldInfo, PlayerInfo newInfo)
        {
            if (oldInfo.level != newInfo.level)
            {
                Time.timeScale = 0;

                for (int i = 0; i < 3; i++)
                {
                    var panel = Instantiate(_prefab, _panelRoot);
                    panel.Init(_datas[Random.Range(0, _datas.Length)], Callback);
                }
            }
        }

        private void Callback(LevelUpUIData data)
        {
            if (TryResolveRequestEntity())
            {
                _entityManager.SetComponentData(_requestEntity, new BulletEnforceRequest
                {
                    data = data.data,
                    pending = 1
                });
            }
 
            Time.timeScale = 1;
            ClearPanels();
        }

        private bool TryResolveRequestEntity()
        {
            if (_entityManager == default)
            {
                var world = World.DefaultGameObjectInjectionWorld;
                if (world == null)
                    return false;

                _entityManager = world.EntityManager;
            }

            if (_requestEntity != Entity.Null && _entityManager.Exists(_requestEntity))
                return true;

            EntityQuery query = _entityManager.CreateEntityQuery(typeof(BulletEnforceRequest));
            if (query.TryGetSingletonEntity<BulletEnforceRequest>(out var entity))
            {
                _requestEntity = entity;
                return true;
            }

            return false;
        }

        private void ClearPanels()
        {
            int childCount = _panelRoot.childCount;
            for (int i = 0; i < childCount; i++)
            {
                Destroy(_panelRoot.GetChild(i).gameObject);
            }
        }
    }
}