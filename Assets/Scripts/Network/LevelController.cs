using Fusion;
using FusionHelpers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class LevelController : ManualSingletonMono<LevelController>
{
    public Player LocalPlayer
    {
        get
        {
            return _localPlayer;
        }
    }

    public GameplayMode GameMode
    {
        get
        {
            return gameMode;
        }
    }

    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private Cinemachine.CinemachineVirtualCamera tpsCamera;
    [SerializeField] private GameplayMode gameMode;

    private Camera _mainCamera;
    private Player _localPlayer;
    private float _timeCountIdle;
    private UIGame _uiGame;

    private int _killsCounter;
    private int _deathsCounter;

    public void Init()
    {
        IdleCheck.ReportAction();

        _uiGame = UIGame.Instance;
        _mainCamera = Camera.main;

        _killsCounter = 0;
        _deathsCounter = 0;
    }


    private void Update()
    {
        if (_localPlayer == null || !_localPlayer.IsActivated)
        {
            return;
        }


        if (Input.anyKey)
        {
            IdleCheck.ReportAction();
        }
    }

    private void LateUpdate()
    {
        if(_timeCountIdle >= 0)
        {
            _timeCountIdle-= Time.deltaTime;
            if(_timeCountIdle < 0)
            {
                _timeCountIdle = 1;
                CheckIdle();
            }
        }
    }


    public void Activate()
    {
        _uiGame.ResetUI();
    }

    public void SetLocalPlayer(Player player)
    {
        _localPlayer = player;
        SetTargetCamera(player);

        _uiGame.ActiveLoadingScenePanel(false);
    }

    public void SetTargetCamera(Player player)
    {
        tpsCamera.Follow = player.TargetCamera;
        tpsCamera.LookAt = player.TargetCamera;
    }

    private void CheckIdle()
    {
        if (IdleCheck.IsIdle)
        {
            _uiGame.Disconnect(true);
        }

    }

    /// <summary>
    /// Only for this player: sets the death text stating the killer on death.
    /// If Unity Ads is enabled, tries to show an ad during the respawn delay.
    /// By using the 'skipAd' parameter is it possible to force skipping ads.
    /// </summary>
    public void DisplayDeath(Player killer, bool skipAd = false)
    {
        string killedByName = "YOURSELF";
        if (killer != null && killer != _localPlayer)
        {
            killedByName = killer.PlayerName;
            SetTargetCamera(killer);
        }

        _deathsCounter++;
        _uiGame.UpdateKillsCounterTxt(_deathsCounter);

        if (killer == null)
        {
            _uiGame.SetKillerInformationText(killedByName, Color.white);

        }
        else
        {
            _uiGame.SetKillerInformationText(killedByName, killer.PlayerColor);

        }

        Respawn();
    }

    public void IncreaseKillsCounter()
    {
        _killsCounter++;
        _uiGame.UpdateKillsCounterTxt(_killsCounter);
    }

    public void Respawn()
    {
        int respawnTime = Global.Instance.DataMgr.ConfigMasterData.RESPAWN_TIME;

        _localPlayer.Respawn(respawnTime);
        _uiGame.OnPlayerRespawn(respawnTime);
    }

    /// <summary>
    /// Returns a random spawn position within the team's spawn area.
    /// </summary>
    public Vector3 GetSpawnPosition(int teamIndex)
    {
        Debug.Log(teamIndex);
        //init variables
        Vector3 pos = spawnPoints[teamIndex].position;
        BoxCollider col = spawnPoints[teamIndex].GetComponent<BoxCollider>();

        if (col != null)
        {
            //find a position within the box collider range, first set fixed y position
            //the counter determines how often we are calculating a new position if out of range
            pos.y = col.transform.position.y;
            int counter = 10;

            //try to get random position within collider bounds
            //if it's not within bounds, do another iteration
            do
            {
                pos.x = UnityEngine.Random.Range(col.bounds.min.x, col.bounds.max.x);
                pos.z = UnityEngine.Random.Range(col.bounds.min.z, col.bounds.max.z);
                counter--;
            }
            while (!col.bounds.Contains(pos) && counter > 0);
        }

        return pos;
    }

    public Vector3 GetRandomSpawnPoint()
    {
        return GetSpawnPosition(Random.Range(0, spawnPoints.Length));
    }
}
