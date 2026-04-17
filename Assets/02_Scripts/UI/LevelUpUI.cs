using System.Linq;
using Unity.Entities;
using UnityEngine;

public class LevelUpUI : MonoBehaviour
{
    [SerializeField] private LevelUpUIData[] _datas;
    [SerializeField] private LevelUpUIPanel _prefab;
    [SerializeField] private Transform _panelRoot;

    private void Awake()
    {
        EnforceDataQ.Clear();
    }

    public void OnPlayerInfoChanged(PlayerInfo oldInfo, PlayerInfo newInfo)
    {
        if(oldInfo.level != newInfo.level)
        {
            Time.timeScale = 0;

            for(int i = 0; i < 3; i++)
            {
                var panel = Instantiate(_prefab, _panelRoot);
                panel.Init(_datas[Random.Range(0, _datas.Length)], Callback);
            }
        }
    }

    private void Callback(LevelUpUIData data)
    {
        EnforceDataQ.Enqueue(data.data);

        Time.timeScale = 1;

        int childCount = _panelRoot.childCount;

        for(int i = 0; i < childCount; i++)
        {
            Destroy(_panelRoot.GetChild(i).gameObject);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            Time.timeScale = 1;
        }
    }
}