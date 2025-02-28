using DG.Tweening;
using Fusion;
using FusionHelpers;
using UnityEngine;

public struct BulletState : ISparseState<Bullet>
{
    /// <summary>
    /// Generic sparse state properties required by the interface
    /// </summary>
    public int StartTick { get; set; }
    public int EndTick { get; set; }

    /// <summary>
    /// Shot specific sparse properties
    /// </summary>
    public Vector3 Position;
    public Vector3 Direction;
    public int Bounce;
    public float Power;
    public Vector3 LastBouncePos;
    public BulletType Type;

    public BulletState(Vector3 startPosition, Vector3 direction, int bounce, Vector3 lastBouncePos, BulletType type, float power)
    {
        StartTick = 0;
        EndTick = 0;
        Position = startPosition;
        Direction = direction;
        Bounce = bounce;
        LastBouncePos = lastBouncePos;
        Type = type;
        Power = power;
    }

    public void Extrapolate(float t, Bullet prefab)
    {
        Position = GetPositionAt(t, prefab);
        Direction = GetDirectionAt(t, prefab);
    }

    public Vector3 GetTargetPosition(Bullet prefab)
    {
        float a = 0.5f * prefab.Gravity.y;
        float b = prefab.Speed * Direction.y;
        float c = Position.y;
        float d = b * b - 4 * a * c;
        float t = (-b - Mathf.Sqrt(d)) / (2 * a);
        Vector3 p = GetPositionAt(t, prefab);
        p.y = 0.05f; // Return the position with a slight y offset to avoid placing target where it will end up z-fighting with the ground;
        return p;
    }

    private Vector3 GetPositionAt(float t, Bullet prefab) => Position + t * (prefab.Speed * Direction + 0.5f * t * prefab.Gravity);
    private Vector3 GetDirectionAt(float t, Bullet prefab) => prefab.Speed == 0 ? Direction : (prefab.Speed * Direction + t * prefab.Gravity).normalized;
}

public class Bullet : MonoBehaviour, ISparseVisual<BulletState, Bullet>
{
    [System.Serializable]
    public struct NetworkDamageData : INetworkStruct
    {
        public DamageEffectType EffectType;
        public float DPS;
        public float SpeedReduced;
        public float Duration;
    }

    public Vector3 Gravity => _gravity;
    public float Speed => _speed;
    public float Radius => _radius;
    public float TimeToLive => _timeToLive;

    public int Bounce => _bouncePowerup;
    public float MultiDamagePowerUp => _multiDamagePowerup;

    public NetworkDamageData DamageEff => _damageEffect;

    [Header("Settings")]
    [SerializeField] private Vector3 _gravity;
    [SerializeField] private float _speed;
    [SerializeField] private float _radius;
    [SerializeField] private float _timeToLive;
    [SerializeField] private bool _isHitScan;
    [SerializeField] private Material[] _materials;
    [SerializeField] private int _bouncePowerup = 3;
    [SerializeField] private float _multiDamagePowerup = 1.5f;
    [SerializeField] private NetworkDamageData _damageEffect;

    [Header("Fx Prefabs")]
    [SerializeField] private GameObject _bulletFx;
    [SerializeField] private MeshRenderer _renderer;
    [SerializeField] private string _detonationName;
    [SerializeField] private string _muzzleFxName;

    private Transform _xform;
    private Tween _delayShowVisualTween;

    private void Awake()
    {
        _xform = transform;
    }

    public void ApplyStateToVisual(NetworkBehaviour owner, BulletState state, float t, bool isFirstRender, bool isLastRender)
    {
        if (isLastRender)
        {
            Debug.Log("Destroy ------ ");
            if(!string.IsNullOrEmpty(_detonationName))
            {
                PoolManager.Instance.GetObjectFromPool(_detonationName, state.Position, Quaternion.identity);
               
            }
            _bulletFx.SetActive(false);
        }
        if (isFirstRender)
        {
            if(_delayShowVisualTween != null)
            {
                _delayShowVisualTween.Kill();
                _delayShowVisualTween = null;
            }

            Debug.Log(state.Type.ToString());
            _delayShowVisualTween = DOVirtual.DelayedCall(0.0f, () => _bulletFx.SetActive(true));

            if (_renderer != null && _materials.Length > 0 && _materials.Length - 1 >= (int)state.Type)
            {
                _renderer.material = _materials[(int)state.Type];
            }

            if (!string.IsNullOrEmpty(_muzzleFxName))
                PoolManager.Instance.GetObjectFromPool(_muzzleFxName, state.Position, Quaternion.LookRotation(state.Direction));
        }
        _xform.forward = state.Direction;
        _xform.position = state.Position;
    }
}

