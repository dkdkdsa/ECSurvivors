using Unity.Entities;
using UnityEngine;
using UnityEngine.Events;

public class PlayerPositionBridge : MonoBehaviour
{
    private EntityQuery _query;

    public UnityEvent<Vector3> OnPlayerPosition;

    private void Awake()
    {
        var em = World.DefaultGameObjectInjectionWorld.EntityManager;
        _query = em.CreateEntityQuery(ComponentType.ReadOnly<PlayerPosition>());
    }

    private void Update()
    {
        if (_query.IsEmpty)
            return;

        var pos = _query.GetSingleton<PlayerPosition>();
        OnPlayerPosition?.Invoke(pos.Value);
    }
}