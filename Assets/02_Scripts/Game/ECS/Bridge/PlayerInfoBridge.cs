using Unity.Entities;
using UnityEngine;
using UnityEngine.Events;

public class PlayerInfoBridge : MonoBehaviour
{
    private EntityQuery _query;
    private PlayerInfo? _oldInfo;

    public UnityEvent<PlayerInfo,PlayerInfo> OnPlayerInfoChanged;

    private void Awake()
    {
        var em = World.DefaultGameObjectInjectionWorld.EntityManager;
        _query = em.CreateEntityQuery(ComponentType.ReadOnly<PlayerInfo>());
    }

    private void Update()
    {
        if (_query.IsEmpty) 
            return;

        var info = _query.GetSingleton<PlayerInfo>();

        if(_oldInfo == null)
            _oldInfo = info;
        else if(!_oldInfo.Value.Equals(info))
        {
            OnPlayerInfoChanged?.Invoke(_oldInfo.Value, info);
            _oldInfo = info;
        }
    }
}
