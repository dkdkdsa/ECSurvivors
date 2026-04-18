using Game.ECS;
using Game.UI;
using UnityEngine;

namespace Game.UGO
{

    public class LevelUpUIGO : MonoBehaviour
    {
        [SerializeField] private LevelUpUIData[] _datas;
        [SerializeField] private LevelUpUIPanel _prefab;
        [SerializeField] private Transform _panelRoot;

        public void OnPlayerInfoChanged(PlayerInfo oldInfo, PlayerInfo newInfo)
        {
            if (oldInfo.level == newInfo.level) return;

            Time.timeScale = 0f;

            for (int i = 0; i < 3; i++)
            {
                var panel = Instantiate(_prefab, _panelRoot);
                panel.Init(_datas[UnityEngine.Random.Range(0, _datas.Length)], Callback);
            }
        }

        private void Callback(LevelUpUIData data)
        {
            if (AttackEnforceSystemGO.HasInstance)
                AttackEnforceSystemGO.Instance.Enqueue(data.data);

            Time.timeScale = 1f;
            ClearPanels();
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
