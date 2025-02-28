using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Fusion;
using FusionHelpers;
using UnityEngine.EventSystems;
using static Bullet;

/// <summary>
/// Networked player class implementing movement control and shooting.
/// Contains both server and client logic in an authoritative approach.
/// </summary> 
[RequireComponent(typeof(NetworkCharacterController))]
public class Player : FusionPlayer
{
    public bool IsActivated => (gameObject.activeInHierarchy && (state == PlayerState.Active || state == PlayerState.Appear));
    public bool IsRespawningDone => state == PlayerState.Appear && respawnTimer.Expired(Runner);
    public bool IsDeath => state == PlayerState.Dead;

    public Vector3 Velocity => Object != null && Object.IsValid ? networkCharacterController.Velocity : Vector3.zero;

    public CharacterStats Stats => _stats;

    public Color PlayerColor { get { return _playerColor; } }
    public Transform TargetCamera { get { return targetCamera; } }

    [Header("FX")]
    [SerializeField] private ParticleSystem getHeartFX;
    [SerializeField] private ParticleSystem shieldFX;
    [SerializeField] private TeleportInEffect _teleportIn;
    [SerializeField] private Transform targetCamera;

    [Header("Visuals")]
    [SerializeField] private PlayerUI playerUI;
    [SerializeField] private Transform visualParent;
    [SerializeField] private Transform skinContainer;
    [SerializeField] protected WeaponManager weaponMgr;
    [SerializeField] private Animator characterAnimator;
    [SerializeField] private PlayerSkinController skinController;

    [Header("Physic")]
    [SerializeField] private NetworkCharacterController networkCharacterController;
    [SerializeField] private DamageEffectController _damageEffectController;



    public struct DamageEvent : INetworkEvent
    {
        public int OwnerPlayerIndex;
        public Vector3 Impulse;
        public int Damage;
        public NetworkDamageData DamageEffect;
    }

    public struct PickupEvent : INetworkEvent
    {
        public int Powerup;
    }

    [Networked] public PlayerState state { get; set; }
    [Networked] private int skinId { get; set; }
    [Networked] private int health { get; set; }
    [Networked] private float shield { get; set; }
    [Networked] private TickTimer boostSpeedTimer { get; set; }
    [Networked] private TickTimer respawnTimer { get; set; }
    [Networked] private TickTimer invulnerabilityTimer { get; set; }

    private UIGame _uiGame;
    private float _respawnInSeconds = -1;
    private ChangeDetector _changes;
    private CharacterStats _stats;
    private Material _playerMaterial;
    private Color _playerColor;
    private int _killStreak = 0;
    private float _walkSpeed;

    public override void InitNetworkState()
    {
        Debug.Log("InitNetworkState" + PlayerName);
        state = PlayerState.New;
    }

    public override void Spawned()
    {
        base.Spawned();

        _uiGame = UIGame.Instance;

        ActiveVisual(false);
        SetupSkin();

        weaponMgr.Init(this);
        weaponMgr.InitStat(_stats);

        gameObject.name = PlayerName;
        _changes = GetChangeDetector(ChangeDetector.Source.SimulationState);

        _teleportIn.Initialize(this);
        OnShieldChange(0);

        // Proxies may not be in state "NEW" when they spawn, so make sure we handle the state properly, regardless of what it is
        OnStageChanged();
        _respawnInSeconds = 0;

        RegisterEventListener((DamageEvent evt) => ApplyAreaDamage(evt.OwnerPlayerIndex, evt.Impulse, evt.Damage, evt.DamageEffect));
        RegisterEventListener((PickupEvent evt) => OnPickup(evt));

        playerUI.SetLabel(PlayerName);
        _damageEffectController.Init(this);
    }

    private void SetupSkin()
    {
        skinId = NetworkedCharacterData.SkinId;
        GameManager gameManager = GameManager.Instance;

        Team team = gameManager.GetTeamData(NetworkedTeamData.TeamId);

        _playerColor = team.TeamColor;
        _playerMaterial = team.TeamMaterial;

        SkinMasterData skinData = Global.Instance.DataMgr.GetSkinMasterData(skinId);
        _stats = skinData.Stats;

        // TODO: Initialize the player with the skin data

        // End of TODO

        networkCharacterController.maxSpeed = _stats.MoveSpeed;
        skinController.SetTeam(team);

        OnHealthChange(_stats.Health);
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        Debug.Log($"Despawned PlayerAvatar for PlayerRef {PlayerId}");
        base.Despawned(runner, hasState);
        GameObject explosion = PoolManager.Instance.GetObjectFromPool("PlayerExplosion", transform.position, Quaternion.identity);
        explosion.SetActive(true);

        ColorChanger.ChangeColor(explosion.transform, _playerColor);

    }


    public void ActiveVisual(bool isActive)
    {
        visualParent.gameObject.SetActive(isActive);
    }

    private void OnPickup(PickupEvent evt)
    {
        // TODO: Implement the pickup drop powerup item event
    }

    public override void FixedUpdateNetwork()
    {
        if (InputController.fetchInput && state == PlayerState.Active && !IsBot)
        {
            // Get our input struct and act accordingly. This method will only return data if we
            // have Input or State Authority - meaning on the controlling player or the server.
            if (GetInput(out NetworkInputData input))
            {
                transform.forward = input.aimForward;

                //Cancel out rotation on X axis as we don't want our character to tilt
                Quaternion rotation = transform.rotation;
                rotation.eulerAngles = new Vector3(0, rotation.eulerAngles.y, rotation.eulerAngles.z);
                transform.rotation = rotation;

                SetMoveDirections(input.moveInput);

                Vector2 walkVector = new Vector2(networkCharacterController.Velocity.x, networkCharacterController.Velocity.z);
                walkVector.Normalize();

                _walkSpeed = Mathf.Lerp(_walkSpeed, Mathf.Clamp01(walkVector.magnitude), Runner.DeltaTime * 5);

                if(!HasInputAuthority)
                {
                    Debug.Log(PlayerName + " Walk Speed: " + _walkSpeed);
                }

                characterAnimator.SetFloat("walkSpeed", _walkSpeed);

                if (input.IsDown(NetworkInputData.BUTTON_FIRE_PRIMARY))
                {
                    weaponMgr.FireWeapon(WeaponManager.WeaponInstallationType.Primary, input.aimForward);
                }

                if (input.IsDown(NetworkInputData.BUTTON_FIRE_SECONDARY))
                {
                    weaponMgr.FireWeapon(WeaponManager.WeaponInstallationType.Secondary, input.aimForward);
                }

                if(input.isJumping)
                {
                    Debug.Log("Jumping: " + networkCharacterController.Grounded);
                    networkCharacterController.Jump();
                }
            }
        }

        if (Object.HasStateAuthority)
        {
            CheckRespawn();

            if (IsRespawningDone)
                ResetPlayer();

            if (state == PlayerState.Dead)
            {
                return;
            }

            if (shield > 0)
            {
                OnShieldChange(shield - Time.deltaTime);
            }
        }
    }

    public override void Render()
    {

        foreach (var change in _changes.DetectChanges(this))
        {
            switch (change)
            {
                case nameof(state):
                    OnStageChanged();
                    break;
                case nameof(health):

                    break;
                case nameof(NetworkedTeamData):
                    GameManager.Instance.OnUpdateScore();
                    break;
            }
        }

        if (IsBot)
        {
            return;
        }


        var interpolated = new NetworkBehaviourBufferInterpolator(this);
        OnHealthChange(GetPropertyReader<int>(nameof(health)).Read(interpolated.To));


    }

    public float GetShield()
    {
        return shield;
    }

    //hook for updating health locally
    //(the actual value updates via player properties)
    protected void OnHealthChange(int value)
    {
        health = value;

        float sliderVal = (float)value / _stats.Health;
        playerUI.UpdateHealthSlider(sliderVal);

        if(HasInputAuthority)
        {
            Global.Instance.GameE.OnUpdateHealth?.Invoke(sliderVal);
        }
    }


    //hook for updating shield locally
    //(the actual value updates via player properties)
    protected void OnShieldChange(float value)
    {
        if (value >= 0.99f)
        {
            shieldFX.Play(true);
        } else if (value <= 0.01f)
        {
            shieldFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        shield = value;
        playerUI.UpdateShieldSlider(value);
    }


   

    /// <summary>
    /// Set the direction of movement and aim
    /// </summary>
    protected void SetMoveDirections(Vector2 movementInput)
    {
        if (!IsActivated)
            return;

        if(!HasInputAuthority)
        {
            Debug.Log(PlayerName + " Move Direction: " + movementInput);
        }
      

        Vector3 moveDirection = transform.forward * movementInput.y + transform.right * movementInput.x;
        moveDirection.Normalize();

        networkCharacterController.Move(moveDirection);
    }

    /// <summary>
    /// Apply damage to Player with an associated impact impulse
    /// </summary>
    /// <param name="impulse"></param>
    /// <param name="damage"></param>
    /// <param name="attacker"></param>
    public void ApplyAreaDamage(int ownerPlayerIndex, Vector3 impulse, int damage, NetworkDamageData damageData, bool isIgnoreDefense = false)
    {
        if (!IsActivated || !invulnerabilityTimer.Expired(Runner))
        {
            Debug.Log("Could not take damage ------------ ");
            return;

        }

        Debug.Log("Damage: " + damage);

        if (GameManager.Instance)
        {
            GameManager gameManager = GameManager.Instance;
            //reduce shield on hit
            if (shield > 0)
            {
                OnShieldChange(shield - 1);
                return;
            }

            networkCharacterController.Velocity += impulse / 10.0f;
            networkCharacterController.Move(Vector3.zero);

            int realDamage = damage;

            if(!isIgnoreDefense)
            {
                realDamage = damage - _stats.Defense;
                if (realDamage <= 0)
                {
                    realDamage = 1;
                }

                SoundManager.Instance.Play3DSFX(AudioEnum.GetHit, transform.position);

            }

            if (realDamage >= health)
            {
                OnHealthChange(0);
                state = PlayerState.Dead;
                Player other = null;
                if (ownerPlayerIndex >= 0)
                {
                    other = gameManager.GetPlayerByIndex<Player>(ownerPlayerIndex);
                }

                if (HasStateAuthority)
                {
                    if(!IsBot)
                    {
                        Debug.Log("sender: " + ownerPlayerIndex + "-" + PlayerName);
                        OnDeath(other);
                    } else
                    {
                        Respawn(Global.Instance.DataMgr.ConfigMasterData.RESPAWN_TIME);
                    }
                  
                    if (other != null)
                    {
                        RpcOnDeath(ownerPlayerIndex);

                    }
                }
            }
            else
            {
                int newHealth = health - realDamage;
                OnHealthChange(newHealth);

                Debug.Log("Damage Effect: " + damageData.EffectType);

                if(damageData.EffectType != DamageEffectType.NONE)
                {
                    _damageEffectController.ActiveDamgeEffect(ownerPlayerIndex, damageData);
                }
            }
        }

        invulnerabilityTimer = TickTimer.CreateFromSeconds(Runner, 0.1f);
    }

    //Local User Only
    private void OnDeath(Player killer)
    {
        _killStreak = 0;
        if (LevelController.Instance.GameMode == GameplayMode.INFINITY_WAR)
        {
            LevelController.Instance.DisplayDeath(killer);
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All, Channel = RpcChannel.Reliable)]
    public void RpcOnDeath(int senderId) 
    {
        _uiGame.ShowMessage(senderId + " Kills " + PlayerName);

        Debug.Log("sender: " + senderId + "-" + PlayerIndex + "-" + PlayerName + "-" + LevelController.Instance.LocalPlayer.PlayerIndex + "-" + IsBot);
        if (LevelController.Instance.LocalPlayer.PlayerIndex == senderId)
        {
            LevelController.Instance.LocalPlayer.KillerHandle();
        } else
        {
            // Bot
            Player other = null;
            if (senderId >= 0)
            {
                other = GameManager.Instance.GetPlayerByIndex<Player>(senderId);
            }

            if(other != null && other.IsBot)
            {
                other.KillerHandleForBot();
            }
        }
    }

    public void KillerHandleForBot()
    {
        NetworkTeamData data = new NetworkTeamData(NetworkedTeamData.TeamId, NetworkedTeamData.TeamScore + 1, NetworkedTeamData.OwnerScore + 1);
        NetworkedTeamData = data;
    }
    
    public void KillerHandle()
    {
        Debug.Log("Handle Score");
        _killStreak++;

        NetworkTeamData data = new NetworkTeamData(NetworkedTeamData.TeamId, NetworkedTeamData.TeamScore + 1, NetworkedTeamData.OwnerScore + 1);
        NetworkedTeamData = data;

        Debug.Log("Team + " + NetworkedTeamData.TeamId + " -" + NetworkedTeamData.OwnerScore);

        LevelController.Instance.IncreaseKillsCounter();
    }


    public void Respawn(float inSeconds = 0)
    {
        _respawnInSeconds = inSeconds;
    }

    private void CheckRespawn()
    {
        if (_respawnInSeconds >= 0)
        {
            _respawnInSeconds -= Runner.DeltaTime;

            if (_respawnInSeconds <= 0)
            {
                
                Vector3 spawnpt = Runner.GetLevelManager().GetPlayerSpawnPoint(NetworkedTeamData.TeamId);

                if (spawnpt == null || spawnpt == Vector3.zero)
                {
                    _respawnInSeconds = Runner.DeltaTime;
                    return;
                }

                // Make sure we don't get in here again, even if we hit exactly zero
                _respawnInSeconds = -1;

                // Restore health
                OnHealthChange(Stats.Health);

                // Start the respawn timer and trigger the teleport in effect
                respawnTimer = TickTimer.CreateFromSeconds(Runner, 1);
                invulnerabilityTimer = TickTimer.CreateFromSeconds(Runner, 1);

                // Place the tank at its spawn point. This has to be done in FUN() because the transform gets reset otherwise

                networkCharacterController.Teleport(spawnpt, Quaternion.identity);

                // If the player was already here when we joined, it might already be active, in which case we don't want to trigger any spawn FX, so just leave it ACTIVE
                if (state != PlayerState.Active)
                    state = PlayerState.Appear;

            }
        }
    }

    public void OnStageChanged()
    {
        switch (state)
        {
            case PlayerState.Appear:
                _teleportIn.StartTeleport();
                break;
            case PlayerState.Active:
                _teleportIn.EndTeleport();
                break;
            case PlayerState.Dead:
                SoundManager.Instance.Play3DSFX(AudioEnum.Player_Explosion, transform.position);

                networkCharacterController.Velocity = Vector3.zero;
                ActiveVisual(false);

                break;
        }
        ActiveVisual(state == PlayerState.Active);

        playerUI.SetActive(state == PlayerState.Active && !Object.HasInputAuthority);
        playerUI.SetAimActive(Object.HasInputAuthority);
    }

    private void ResetPlayer()
    {
        weaponMgr.ResetAllWeapons();
        state = PlayerState.Active;

        if (HasStateAuthority && !IsBot)
        {
            LevelController.Instance.SetLocalPlayer(this);
        }
    }
}

[System.Serializable]
public struct CharacterStats
{
    public int Health;
    public int Damage;
    public int Defense;
    public float FireRate;
    public int Range;
    public float ExplosionRadius;
    public int MoveSpeed;
}

