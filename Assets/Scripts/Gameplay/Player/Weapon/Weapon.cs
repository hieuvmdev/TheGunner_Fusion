using DG.Tweening;
using DG.Tweening.Core.Easing;
using Fusion;
using FusionHelpers;
using System.Collections;
using System.Collections.Generic;
using System.IO.Pipes;
using UnityEngine;
using UnityEngine.UIElements;

public class Weapon : NetworkBehaviourWithState<Weapon.NetworkState>
{
    public float delay => _rateOfFire;
    public bool isShowing => _visible >= 1.0f;

    public float bulletSpeed => _bulletPrefab.Speed;

    [Networked] public override ref NetworkState State => ref MakeRef<NetworkState>();
    public struct NetworkState : INetworkStruct
    {
        [Networked, Capacity(12)]
        public NetworkArray<BulletState> bulletStates => default;
    }

    [SerializeField] private Transform[] _gunExits;

    [SerializeField] private Bullet _bulletPrefab;

    [Header("Config")]
    [SerializeField] private float _areaImpulse;
    [SerializeField] private LayerMask _hitMask;

    [SerializeField] private float _areaRadius;
    [SerializeField] private byte _areaDamage;
    [SerializeField] private float _range;
    [SerializeField] private float _rateOfFire;

    private SparseCollection<BulletState, Bullet> _bullets;
    private float _visible;
    private bool _active;
    private Player _player;

    private Vector3 _defaultScale;

    public void Init(Player player)
    {
        _player = player;
        _defaultScale = transform.localScale;
    }

    public void InitStat(CharacterStats stat)
    {
        _areaDamage = (byte) stat.Damage;
        _areaRadius = stat.ExplosionRadius;
        _range = (byte)stat.Range;
        _rateOfFire = stat.FireRate;
    }

    public override void Spawned()
    {
        _bullets = new SparseCollection<BulletState, Bullet>(State.bulletStates, _bulletPrefab);
    }

    public override void FixedUpdateNetwork()
    {
        _bullets.Process(this, (ref BulletState bullet, int tick) =>
        {
            if (bullet.Position.y < -.15f)
            {
                bullet.EndTick = Runner.Tick;
                return true;
            }

            if (bullet.EndTick > Runner.Tick)
            {
                Vector3 dir = bullet.Direction.normalized;
                float length = Mathf.Max(_bulletPrefab.Radius, _bulletPrefab.Speed * Runner.DeltaTime);

                Collider collider = null;
                Vector3 hitPoint = Vector3.zero;
                Vector3 normal = Vector3.zero;

                if (Runner.GameMode == GameMode.Shared)
                {
                    if(Runner.GetPhysicsScene().Raycast(bullet.Position - length * dir, dir, out var hitinfo, length, _hitMask.value))
                    {
                        collider = hitinfo.collider;
                        hitPoint = hitinfo.point;
                        normal = hitinfo.normal;
                    }
                } 
                else
                {
                    if (Runner.LagCompensation.Raycast(bullet.Position - length * dir, dir, length, Object.InputAuthority, out var hitinfo, _hitMask.value, HitOptions.IgnoreInputAuthority | HitOptions.IncludePhysX))
                    {
                        collider = hitinfo.Collider;
                        hitPoint = hitinfo.Point;
                        normal = hitinfo.Normal;
                    }
                }

                if(collider != null)
                {
                    Debug.Log("Hit Something: " + collider.name);
                    Player player = collider.GetComponent<Player>();

                    bullet.Position = hitPoint;


                    if (player != null || bullet.Bounce == 0)
                    {
                        Debug.Log("Apply Area Damage");
                        ApplyAreaDamage(collider, hitPoint, bullet.Power, bullet.Type);
                        bullet.EndTick = Runner.Tick;

                    }
                    else
                    {
                        if (Vector3.Distance(bullet.Position, bullet.LastBouncePos) > 0.05f)
                        {
                            bullet.LastBouncePos = hitPoint;
                            bullet.Bounce--;

                            Vector3 direction = Vector3.Reflect(dir, normal);
                            bullet.Direction = direction; 
                        }
                    }

              
                    return true;
                }
            }
            return false;
        });
    }

    public override void Render()
    {
        if (TryGetStateChanges(out var from, out var to))
        {

        }
        else
        {
            TryGetStateSnapshots(out from, out _, out _, out _, out _);
        }

        _bullets.Render(this, from.bulletStates);
    }

    /// <summary>
    /// Control the visual appearance of the weapon. This is controlled by the Player based
    /// on the currently selected weapon, so the boolean parameter is entirely derived from a
    /// networked property (which is why nothing in this class is sync'ed).
    /// </summary>
    /// <param name="show">True if this weapon is currently active and should be visible</param>
    public void Show(bool show)
    {
        if (_active && !show)
        {
            ToggleActive(false);
        }
        else if (!_active && show)
        {
            ToggleActive(true);
        }

        _visible = Mathf.Clamp(_visible + (show ? Time.deltaTime : -Time.deltaTime) * 5f, 0, 1);

        if (show)
            transform.localScale = DOVirtual.EasedValue(0, 1, _visible, Ease.OutElastic) * _defaultScale; 
        else
            transform.localScale = DOVirtual.EasedValue(0, 1, _visible, Ease.InExpo) * _defaultScale;
    }

    private void ToggleActive(bool value)
    {
        _active = value;
    }

    public void Fire(NetworkRunner runner, Vector3 aimForwardVector, Vector3 ownerVelocity, BulletType type)
    {
        if (_gunExits.Length == 0)
            return;

        Transform exit = GetExitPoint(Runner.Tick);
        SpawnNetworkShot(runner, aimForwardVector, exit, ownerVelocity, type);
    }


    private void SpawnNetworkShot(NetworkRunner runner, Vector3 aimForwardVector, Transform exit, Vector3 ownerVelocity, BulletType type)
    {

        int bounce = 0;
        float power = 0;

        switch (type)
        {
            case BulletType.Bounce:
                bounce = _bulletPrefab.Bounce;
                break;
            case BulletType.Power:
                power = _bulletPrefab.MultiDamagePowerUp;
                break;
            default:
                bounce = 0;
                power = 1;
                break;
        }

        Debug.DrawRay(exit.position, aimForwardVector * 10, Color.red, 2f);

        _bullets.Add(runner, new BulletState(exit.position, aimForwardVector, bounce, exit.position, type, power), _bulletPrefab.TimeToLive);
    }

    private void ApplyAreaDamage(Collider col, Vector3 hitPoint, float power, BulletType type)
    {
        Collider[] colliders = null;
        int hitCount = 0;
        Vector3 normal = Vector3.zero;

        if (Runner.GameMode == GameMode.Shared)
        {
            hitCount = Runner.GetPhysicsScene().OverlapSphere(hitPoint, _areaRadius + _bulletPrefab.Radius, colliders, _hitMask.value, QueryTriggerInteraction.UseGlobal);
        }
        else
        {
            var hits = new List<LagCompensatedHit>();
            hitCount = Runner.LagCompensation.OverlapSphere(hitPoint, _areaRadius + _bulletPrefab.Radius, Object.InputAuthority, hits, _hitMask.value, HitOptions.IgnoreInputAuthority | HitOptions.IncludePhysX);

            colliders = new Collider[hits.Count];
            for (int i = 0; i < hits.Count; i++)
            {
                colliders[i] = hits[i].Collider;
            }
        }

        if (hitCount > 0)
        {
            for (int i = 0; i < colliders.Length; i++)
            {
                GameObject other = colliders[i].gameObject;
                Debug.Log(other.gameObject.name);
                if (colliders[i] != col &&  other)
                {
                    Player target = other.GetComponent<Player>();
                    ApplyDamage(target, hitPoint, power);
                }
            }
        }

        if(col.gameObject.TryGetComponent(out Player player))
        {
            ApplyDamage(player, hitPoint, power);
        }
    }

    private void ApplyDamage(Player target, Vector3 hitPoint, float power)
    {
        if (target != null && target != _player)
        {
            Vector3 impulse = target.transform.position - hitPoint;
            float l = Mathf.Clamp(_areaRadius - impulse.magnitude, 0, _areaRadius);
            byte damage = (byte)(_areaDamage * power);
            impulse = _areaImpulse * l * impulse.normalized;

            target.RaiseEvent(new Player.DamageEvent { OwnerPlayerIndex = _player.PlayerIndex, Impulse = impulse, Damage = damage, DamageEffect = _bulletPrefab.DamageEff });

        }    
    }

    public Transform GetExitPoint(int tick)
    {
        Transform exit = _gunExits[tick % _gunExits.Length];
        return exit;
    }
}