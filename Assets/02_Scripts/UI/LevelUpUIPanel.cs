using System;
using TMPro;
using UnityEngine;

public class LevelUpUIPanel : MonoBehaviour
{
    [SerializeField] private TMP_Text _text;
    private LevelUpUIData _data;
    private Action<LevelUpUIData> _callback;

    public void Init(LevelUpUIData data, Action<LevelUpUIData> callback)
    {
        _data = data;
        _text.text = data.expText;
        _callback = callback;
    }

    public void Select()
    {
        _callback?.Invoke(_data);
    }
}
