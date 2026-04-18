using Game.ECS;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class PlayerLevelUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text _levelText;
        [SerializeField] private Slider _expSlider;

        public void OnPlayerInfoChanged(PlayerInfo oldInfo, PlayerInfo newInfo)
        {
            _levelText.text = newInfo.level.ToString();
            _expSlider.value = Mathf.InverseLerp(0, newInfo.needLevelUp, newInfo.exp);
        }
    }
}