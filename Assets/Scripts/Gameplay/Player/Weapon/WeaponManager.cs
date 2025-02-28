using DG.Tweening;
using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : NetworkBehaviour
{
    public float primaryWeaponBulletSpeed
    {
        get
        {
            return primaryWeapon.bulletSpeed;
        }
    }

    [SerializeField]
    public enum WeaponInstallationType
    {
        Primary,
        Secondary,
        Buff
    };

    [SerializeField] private Weapon primaryWeapon;
    [SerializeField] private Weapon secondaryWeapon;

    [SerializeField] private AudioClip primaryShotClip;
    [SerializeField] private AudioClip secondaryShotClip;

    [Networked]
    public TickTimer primaryFireDelay { get; set; }

    [Networked]
    public TickTimer secondaryFireDelay { get; set; }

    [Networked]
    [HideInInspector]
    public byte primaryPowerupAmmo { get; set; }

    [Networked]
    [HideInInspector]
    public byte secondaryPowerupAmmo { get; set; }

    [Networked]
    [HideInInspector]
    public int bulletType { get; set; }

    private Player _player;
    private bool _isSpawned = false;
    private ChangeDetector _changes;


    public void Init(Player player)
    {
        _player = player;
        _changes = GetChangeDetector(ChangeDetector.Source.SimulationState);

        primaryWeapon.Init(player);
        primaryWeapon.Show(true);
        if (secondaryWeapon != null)
        {
            secondaryWeapon.gameObject.SetActive(true);

            Debug.Log("Init Secondary Weapon");
            secondaryWeapon.Init(player);
            secondaryWeapon.Show(false);
        }
    }

    public void InitStat(CharacterStats stat)
    {
        primaryWeapon.InitStat(stat);
        if(secondaryWeapon != null)
        {
            secondaryWeapon.InitStat(stat);
        }
    }

    public override void Spawned()
    {
        base.Spawned();
        _isSpawned = true;
    }

    public override void Render()
    {
        foreach (var change in _changes.DetectChanges(this))
        {
            switch (change)
            {
                case nameof(bulletType):
                case nameof(primaryPowerupAmmo):
                    break;
            }
        }

        ShowAndHideWeapons();
    }

    public BulletType GetBulletType()
    {
        return (BulletType) bulletType;
    }

    public int GetBulletAmmo(WeaponInstallationType type)
    {
        if(type == WeaponInstallationType.Primary)
        {
            return primaryPowerupAmmo;
        } else
        {
            return secondaryPowerupAmmo;
        }
   
    }

    private void ShowAndHideWeapons()
    {
        if(primaryWeapon != null)
        {
            primaryWeapon.Show(true);
        }

        if (secondaryWeapon != null)
        {
          
            secondaryWeapon.Show(secondaryPowerupAmmo > 0);
        }

    }

    /// <summary>
    /// Fire the current weapon. This is called from the Input Auth Client and on the Server in
    /// response to player input. Input Auth Client spawns a dummy shot that gets replaced by the networked shot
    /// whenever it arrives
    /// </summary>
    public void FireWeapon(WeaponInstallationType weaponType, Vector3 airForward)
    {
        if (!IsWeaponFireAllowed(weaponType))
            return;

        byte powerupAmmo = 0;
        TickTimer tickTimer = primaryFireDelay;
        Weapon weapon = null;
        switch (weaponType)
        {
            case WeaponInstallationType.Primary:
                powerupAmmo = primaryPowerupAmmo;
                tickTimer = primaryFireDelay;
                weapon = primaryWeapon;
                break;
            case WeaponInstallationType.Secondary:
                powerupAmmo = secondaryPowerupAmmo;
                tickTimer = secondaryFireDelay;
                weapon = secondaryWeapon;
                break;
            default:
                return;
        }

        if (tickTimer.ExpiredOrNotRunning(Runner))
        {
            if(powerupAmmo == 0 && weaponType == WeaponInstallationType.Secondary)
            {
                return;
            }

            BulletType type = (BulletType)bulletType;

            weapon.Fire(Runner, airForward, _player.Velocity, type);
  
            //if (!weapon.infiniteAmmo)
            powerupAmmo--;

            if (weaponType == WeaponInstallationType.Primary)
            {
                primaryFireDelay = TickTimer.CreateFromSeconds(Runner, weapon.delay);
                primaryPowerupAmmo = powerupAmmo;
                SoundManager.Instance.PlayAtPoint(primaryShotClip, transform.position);
            }
            else
            {
                secondaryFireDelay = TickTimer.CreateFromSeconds(Runner, weapon.delay);
                secondaryPowerupAmmo = powerupAmmo;
                SoundManager.Instance.PlayAtPoint(secondaryShotClip, transform.position);
            }

            if (/*Object.HasStateAuthority &&*/ powerupAmmo == 0)
            {
                ResetWeapon(weaponType);
            }
        }
    }

    private bool IsWeaponFireAllowed(WeaponInstallationType weaponType)
    {
        if (!_player.IsActivated)
            return false;

        // Has the selected weapon become fully visible yet? If not, don't allow shooting
        if (weaponType == WeaponInstallationType.Primary && primaryWeapon == null)
            return false;
        if (weaponType == WeaponInstallationType.Secondary && secondaryWeapon == null)
            return false;
        return true;
    }

    public void ResetAllWeapons()
    {
        if(!_isSpawned) return;

        ResetWeapon(WeaponInstallationType.Primary);
        ResetWeapon(WeaponInstallationType.Secondary);
    }

    void ResetWeapon(WeaponInstallationType weaponType)
    {
        if (weaponType == WeaponInstallationType.Primary)
        {
            primaryPowerupAmmo = 0;
            bulletType = (byte)BulletType.Normal;
        }
        else if (weaponType == WeaponInstallationType.Secondary)
        {
            secondaryPowerupAmmo = 0;
        }



    }
}