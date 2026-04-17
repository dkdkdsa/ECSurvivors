using Unity.Entities;
using UnityEngine;

public class LevelUpUI : MonoBehaviour
{
    public void OnPlayerInfoChanged(PlayerInfo oldInfo, PlayerInfo newInfo)
    {
        if(oldInfo.level != newInfo.level)
        {
            Debug.Log("LevelUp");
            Time.timeScale = 0;
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