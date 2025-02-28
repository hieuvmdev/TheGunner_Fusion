using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RankingTab : MonoBehaviour
{
    public bool IsActive;
    public int PlayerID;
    public int CurScore;

    public Image medarImage;
    public TextMeshProUGUI rankingTxt;
    public TextMeshProUGUI scoreTxt;
    public TextMeshProUGUI userNameTxt;
    

    public void ActiveUser(int playerID, string userName, Color32 playerColor)
    {
        this.PlayerID = playerID;
        userNameTxt.SetText(userName);
        userNameTxt.color = playerColor;
        scoreTxt.color = playerColor;

        medarImage.enabled = false;
        rankingTxt.enabled = true;
        rankingTxt.SetText("-");

        SetScore(0);

        transform.SetAsLastSibling();
        gameObject.SetActive(true);
        IsActive = true;

        Debug.Log(userName + " " + playerColor);
    }

    public void Deactive()
    {
        gameObject.SetActive(false);
        CurScore = -1;
        IsActive = false;
    }

    public void SetRank(int rank)
    {
        rankingTxt.SetText(rank.ToString());
        medarImage.enabled = false;
        rankingTxt.enabled = true;
    }

    public void SetScore(int score)
    {

        CurScore = score;
        scoreTxt.SetText(score.ToString());
    }

    public void SetRank(Sprite medar)
    {
        medarImage.sprite = medar;
        medarImage.enabled = true;
        rankingTxt.enabled = false;
    }
}
