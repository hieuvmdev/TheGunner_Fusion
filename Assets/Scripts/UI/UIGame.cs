using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;


public class UIGame : SingletonMonoAwake<UIGame>
{
    [SerializeField] private TextMeshProUGUI killsCounterTxt;
    [SerializeField] private TextMeshProUGUI deathsCounterTxt;

    [SerializeField] private LeaderboardUI leaderboardUI;
    [SerializeField] private TextMeshProUGUI killerInformationText;
    [SerializeField] private TextMeshProUGUI spawnDelayText;
    [SerializeField] private Slider spawnDelaySlider;
    [SerializeField] private PlayerInfoUI playerInfoUI;
    [SerializeField] private SystemMessageController systemMessageController;

    [SerializeField] private PausePanel pausePanel;
    [SerializeField] private LoadingPanel loadingScenePanel;

    // Start is called before the first frame update
    public void Initialized()
    {
        playerInfoUI.Init();
        systemMessageController.Init();

        pausePanel.Init();
        loadingScenePanel.Init();

        ResetUI();
        Global.Instance.GameE.OnDisconnect += Disconnect;
    }

    private void OnDestroy()
    {
        Global.Instance.GameE.OnDisconnect -= Disconnect;
    }

    public void ActiveLoadingScenePanel(bool isActive)
    {
        if(isActive)
        {
            loadingScenePanel.Open();
        }
        else
        {
            loadingScenePanel.Close();
        }
    }


    public void OnPauseGameTap()
    {
        SoundManager.Instance.PlaySFX(AudioEnum.ButtonClick, 0);
        pausePanel.Open();
    }

    /// <summary>
    /// This is an implementation for changes to the team fill,
    /// updating the slider values (updates UI display of team fill).
    /// </summary>
    public void OnTeamSizeChanged(int[] size)
    {
        Debug.Log("Team Change");
        GameManager gameManager = GameManager.Instance;

        if (LevelController.Instance.GameMode == GameplayMode.INFINITY_WAR)
        {
            for (int i = 0; i < size.Length; i++)
            {
                if (size[i] > 0)
                {
                    if (!leaderboardUI.IsUserActived(i))
                    {
                        string userName = "unknown";
                        bool foundUser = false;
                        for (int j = 0; j < gameManager.PlayerCount; j++)
                        {
                            Player player = gameManager.GetPlayerByArrayIndex<Player>(j);
                            if (player.NetworkedTeamData.TeamId == i)
                            {
                                foundUser = true;
                                userName = player.PlayerName;
                                break;
                            }
                        }

                        if (foundUser)
                        {
                            Team team = gameManager.GetTeamData(i);
                            leaderboardUI.ActiveUser(i, userName, team.TeamColor);
                        }
                        else
                        {
                            Debug.Log("UNKNOW USER ------- ");

                            if (leaderboardUI.IsUserActived(i))
                            {
                                leaderboardUI.DeactiveUser(i);
                            }
                        }


                    }
                }
                else
                {
                    if (leaderboardUI.IsUserActived(i))
                    {
                        leaderboardUI.DeactiveUser(i);
                    }
                }
            }
        }
    }


    /// <summary>
    /// This is an implementation for changes to the team score,
    /// updating the text values (updates UI display of team scores).
    /// </summary>
    public void OnTeamScoreChanged(int[] score)
    {
        Debug.Log(JsonUtility.ToJson(score));
        for (int i = 0; i < score.Length; i++)
        {
            leaderboardUI.UpdateScore(i, score[i]);
        }
    }

    public void OnPlayerRespawn(int respawnTime)
    {
        StartCoroutine(IESpawnDelay(respawnTime));
    }
    //coroutine spawning the player after a respawn delay
    IEnumerator IESpawnDelay(int respawnTime)
    {
        //calculate point in time for respawn
        float targetTime = Time.time + respawnTime;

        while (targetTime - Time.time > 0)
        {
            SetSpawnDelay(targetTime - Time.time, respawnTime);
            yield return null;
        }

        //respawn now: send request to the server
        DisableDeath();
    }

    public void UpdateKillsCounterTxt(int val)
    {

        killsCounterTxt.text = val.ToString();
        //killsCounterTxt.GetComponent<Animator>().Play("Animation");

    }

    public void UpdateDeathsCounterTxt(int val)
    {

        deathsCounterTxt.text = val.ToString();
        //deathsCounterTxt.GetComponent<Animator>().Play("Animation");

    }

    public void ShowMessage(string message)
    {
        systemMessageController.ShowMessage(message);
    }

    public void SetKillerInformationText(string playerName, Color teamColor)
    {
        //show killer name and colorize the name converting its team color to an HTML RGB hex value for UI markup
        killerInformationText.text = "KILLED BY\n<color=#" + ColorUtility.ToHtmlStringRGB(teamColor) + ">" + playerName + "</color>";
    }

    public void SetSpawnDelay(float time, float respawnTime)
    {
        if (time > 0 && !spawnDelaySlider.gameObject.activeInHierarchy)
        {
            spawnDelaySlider.gameObject.SetActive(true);
        }

        spawnDelayText.text = Mathf.Ceil(time) + "";
        spawnDelaySlider.value = time / respawnTime;
    }

    public void DisableDeath()
    {
        //clear text component values
        killerInformationText.text = string.Empty;
        spawnDelaySlider.gameObject.SetActive(false);
    }

    /// <summary>
    /// Stops receiving further network updates by hard disconnecting, then load starting scene.
    /// </summary>
    public void Disconnect(bool showInformationPanel = false)
    {
        NetworkManager.Instance.IsKickFromRoom = showInformationPanel;
        NetworkManager.Instance.Disconnect();
    }

    private IEnumerator IEQuit()
    {

        yield return new WaitUntil(() => !NetworkManager.Instance.LocalRunner.IsConnectedToServer);
        BackMainMenu();

    }

    public void BackMainMenu()
    {
        SceneManager.LoadScene(NetworkManager.Instance.MainMenuSceneIndex);
    }

    public void ResetUI()
    {
        killsCounterTxt.SetText("0");
        deathsCounterTxt.SetText("0");

        spawnDelaySlider.gameObject.SetActive(false);
        killerInformationText.text = String.Empty;

    }
}
