using Fusion;
using FusionHelpers;
using System;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : NetworkSceneManagerDefault
{
    public Action<NetworkRunner, FusionLauncher.ConnectionStatus, string> onStatusUpdate { get; set; }

    [SerializeField] private int _mainMenu;

    private LevelController _currentLevel;
    private SceneRef _loadedScene = SceneRef.None;
    private int _selectLevel;

    public override void Shutdown()
    {
        Debug.Log("LevelManager.Shutdown();");
        _currentLevel = null;
        if (_loadedScene.IsValid)
        {
            Debug.Log($"LevelManager.UnloadLevel(); - _currentLevel={_currentLevel} _loadedScene={_loadedScene}");
           
            _loadedScene = SceneRef.None;
            SceneManager.LoadScene(_mainMenu);
        }
        base.Shutdown();
    }

    public Vector3 GetPlayerSpawnPoint(int teamIndex)
    {
        if (_currentLevel != null)
            return _currentLevel.GetSpawnPosition(teamIndex);
        return Vector3.zero;
    }

    public string GetPlayerName() 
    {
        return Global.Instance.GameD.NickName;
    }

    public void SetSelectedLevel(int level)
    {
        _selectLevel = level;
    }

    protected override IEnumerator UnloadSceneCoroutine(SceneRef prevScene)
    {
        Debug.Log($"LevelManager.UnloadSceneCoroutine({prevScene});");

        GameManager gameManager;
        while (!Runner.TryGetSingleton(out gameManager))
        {
            Debug.LogWarning("Waiting for GameManager");
            yield return null;
        }


        if (prevScene.AsIndex > 0)
        {
            yield return new WaitForSeconds(1.0f);

            InputController.fetchInput = false;

            // Despawn players with a small delay between each one
            Debug.Log("De-spawning all tanks");
            foreach (FusionPlayer fusionPlayer in gameManager.AllPlayers)
            {
                Player player = (Player)fusionPlayer;
                Debug.Log($"De-spawning tank {fusionPlayer.PlayerIndex}:{fusionPlayer}");
                yield return new WaitForSeconds(0.1f);
            }

            yield return new WaitForSeconds(1.5f - gameManager.PlayerCount * 0.1f);

            //_scoreManager.ResetAllGameScores();
            //if (gameManager.lastPlayerStanding != null)
            //{
            //    _scoreManager.ShowIntermediateLevelScore(gameManager);
            //    yield return new WaitForSeconds(1.5f);
            //    _scoreManager.ResetAllGameScores();
            //}
        }

        yield return base.UnloadSceneCoroutine(prevScene);
    }

    protected override IEnumerator OnSceneLoaded(SceneRef newScene, Scene loadedScene, NetworkLoadSceneParameters sceneFlags)
    {
        Debug.Log($"LevelManager.OnSceneLoaded({newScene},{loadedScene},{sceneFlags});");
        yield return base.OnSceneLoaded(newScene, loadedScene, sceneFlags);

        if (newScene.AsIndex == 0)
            yield break;

        onStatusUpdate?.Invoke(Runner, FusionLauncher.ConnectionStatus.Loading, "");

        yield return null;

        _loadedScene = newScene;
        Debug.Log($"Loading scene {newScene}");

        // Delay one frame
        yield return null;

        onStatusUpdate?.Invoke(Runner, FusionLauncher.ConnectionStatus.Loaded, "");

        // Activate the next level
        _currentLevel = FindObjectOfType<LevelController>();
        if (_currentLevel != null)
            _currentLevel.Activate();

        yield return new WaitForSeconds(0.3f);

      
        while (!GameManager.Instance)
        {
            Debug.Log($"Waiting for GameManager to Spawn!");
            yield return null;
        }

        GameManager gameManager = GameManager.Instance;

        // Respawn with slight delay between each player
        Debug.Log($"Respawning All {gameManager.PlayerCount} Players");
        foreach (FusionPlayer fusionPlayer in gameManager.AllPlayers)
        {
            Player player = (Player)fusionPlayer;
            Debug.Log($"Initiating Respawn of Player #{fusionPlayer.PlayerIndex} ID:{fusionPlayer.PlayerId}:{player}");
            player.Respawn();
            yield return new WaitForSeconds(0.3f);
        }

        // Enable inputs after countdow finishes
        InputController.fetchInput = true;
        Global.Instance.GameE.ActiveLoading(false);
    }
}
