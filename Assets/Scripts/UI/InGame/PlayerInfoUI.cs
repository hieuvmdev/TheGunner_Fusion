using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInfoUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerName;
    [SerializeField] private Slider healthSlider;

    public void Init()
    {
        SetPlayerName(Global.Instance.GameD.NickName);
        Global.Instance.GameE.OnUpdateHealth += SetHealth;
    }
    private void OnDestroy()
    {
        Global.Instance.GameE.OnUpdateHealth -= SetHealth;
    }

    public void SetPlayerName(string name)
    {
        playerName.text = name;
    }

    public void SetHealth(float val)
    {
        healthSlider.value = val;
    }
}   
