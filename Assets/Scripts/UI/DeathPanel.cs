using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class DeathPanel : BasePanel
{
    private const string TITTLE_DEATH_PANEL = "You Died!";

    [SerializeField] private GameObject container;

    [SerializeField] private TextMeshProUGUI titleTxt;

    [SerializeField] private TextMeshProUGUI killTxt;
    [SerializeField] private TextMeshProUGUI deathTxt;

    [SerializeField] private GameObject backHomeBtn;
    [SerializeField] private GameObject playAgainBtn;

    public void SetData(string kill, string death)
    {

        titleTxt.SetText(TITTLE_DEATH_PANEL);

        killTxt.SetText(kill);
        deathTxt.SetText(death);
    
        backHomeBtn.SetActive(false);
        DOVirtual.DelayedCall(3.0f, () => { backHomeBtn.SetActive(true); });
    }

    public void OnTapToBackHomeButtonClick()
    {
        SoundManager.Instance.PlaySFX(AudioEnum.ButtonClick, 0);
        //LevelController.Instance.UIGame.Disconnect();
    }

    public void OnPlayAgainTap()
    {
        base.Close();
        LevelController.Instance.Respawn();
    }

    public void OnShowTap()
    {
        SoundManager.Instance.PlaySFX(AudioEnum.ButtonClick, 0);
        container.SetActive(true);
    }

    public void OnHideTap()
    {
        SoundManager.Instance.PlaySFX(AudioEnum.ButtonClick, 0);
        container.SetActive(false);
    }
}
 