using UnityEngine;
using Fusion;
using FusionHelpers;


public class GameManager : FusionSession
{
    public const ShutdownReason ShutdownReason_GameAlreadyRunning = (ShutdownReason)100;

    public static GameManager Instance => _instance;
    private static GameManager _instance;

    private bool _restart;
    private int _matchScore = -1;
    private ChangeDetector _changeDetector;
    private UIGame _uiGame;
    private Team[] teamDatas;

    private void Awake()
    {
        _instance = this;
        Global.Instance.GameE.ActiveLoading(false);
    }

    private void Start()
    {
        teamDatas = Global.Instance.DataMgr.TeamMasterData.TeamData;

        _uiGame = UIGame.Instance;
        _uiGame.Initialized();

        SoundManager.Instance.PlayBackgroundMusic(false);

        EffectManager.Instance.Init();
        LevelController.Instance.Init();

        _uiGame.ActiveLoadingScenePanel(true);
    }

    public override void Spawned()
    {
        _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
    }

    protected override void OnPlayerAvatarAdded(FusionPlayer fusionPlayer)
    {

    }

    protected override void OnPlayerAvatarRemoved(FusionPlayer fusionPlayer)
    {

    }

    public Team GetTeamData(int teamId)
    {
        return teamDatas[teamId];
    }

    public override void Render()
    {
        base.Render();


        if (LevelController.Instance == null)
        {
            return;
        }



        foreach (var change in _changeDetector.DetectChanges(this))
        {
            switch (change)
            {

                case nameof(teamSize):
                    Debug.Log("Team Size Change");
                    _uiGame.OnTeamSizeChanged(teamSize.ToArray());
                    _uiGame.OnTeamScoreChanged(GetPlayerScores());
                    break;
            }
        }
    }

    public void OnUpdateScore()
    {
        _uiGame.OnTeamScoreChanged(GetPlayerScores());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            _uiGame.OnPauseGameTap();
        }
    }
}