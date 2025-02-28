using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static Bullet;

public class DamageEffectController : NetworkBehaviour
{
    [Networked] public TickTimer poisonTime { get; set; }
    [Networked] public TickTimer burnTime { get; set; }

    [SerializeField] private GameObject poisonEffect;
    [SerializeField] private GameObject burnEffect;


    private Player _player;
    private float _updateTime;

    private int _poisonOwnerPlayerIndex;
    private int _poisonDPS;

    private int _burnOwnerPlayerIndex;
    private int _burnDPS;

    public void Init(Player player)
    {
        _player = player;
        _updateTime = 1;
    }

    public override void FixedUpdateNetwork()
    {
        base.FixedUpdateNetwork();

        poisonEffect.SetActive(!poisonTime.ExpiredOrNotRunning(Runner));
        burnEffect.SetActive(!burnTime.ExpiredOrNotRunning(Runner));

        if(HasStateAuthority)
        {
            if(_updateTime > 0)
            {
                _updateTime -= Runner.DeltaTime;
            } else
            {
                _updateTime = 1;
                UpdateEffect();
            }
        }
    }

    public override void Render()
    {
        base.Render();
      

        var interpolated = new NetworkBehaviourBufferInterpolator(this);


        burnEffect.SetActive(!GetPropertyReader<TickTimer>(nameof(burnTime)).Read(interpolated.From).ExpiredOrNotRunning(Runner));
        poisonEffect.SetActive(!GetPropertyReader<TickTimer>(nameof(poisonTime)).Read(interpolated.From).ExpiredOrNotRunning(Runner));
    }

    public void ActiveDamgeEffect(int ownerPlayerIndex, NetworkDamageData damageData)
    {
        if(damageData.EffectType == DamageEffectType.POISON)
        {
            float? remaingTime = poisonTime.RemainingTime(Runner);
            if (remaingTime.HasValue && remaingTime.Value > 0)
            {
                poisonTime = TickTimer.CreateFromSeconds(Runner, damageData.Duration + remaingTime.Value);
            }
            else
            {
                poisonTime = TickTimer.CreateFromSeconds(Runner, damageData.Duration);
            }

            _poisonOwnerPlayerIndex = ownerPlayerIndex;
        }

        if (damageData.EffectType == DamageEffectType.BURN)
        {
            float? remaingTime = burnTime.RemainingTime(Runner);
            if (remaingTime.HasValue && remaingTime.Value > 0)
            {
                burnTime = TickTimer.CreateFromSeconds(Runner, damageData.Duration + remaingTime.Value);
            } else
            {
                burnTime = TickTimer.CreateFromSeconds(Runner, damageData.Duration);
            }
            _burnOwnerPlayerIndex = ownerPlayerIndex;
        }
    }

    private void UpdateEffect()
    {
        float? remaingTime = burnTime.RemainingTime(Runner);

        if (remaingTime.HasValue && remaingTime.Value > 0)
        {
            Debug.Log("Burn Time: " + remaingTime.Value);
        }
  
        if (!poisonTime.ExpiredOrNotRunning(Runner))
        {
            _player.ApplyAreaDamage(_poisonOwnerPlayerIndex, Vector2.zero, _poisonDPS, new NetworkDamageData(), true);
        }

        if (!burnTime.ExpiredOrNotRunning(Runner))
        {
            _player.ApplyAreaDamage(_burnOwnerPlayerIndex, Vector2.zero, _burnDPS, new NetworkDamageData(), true);

        }
    }
}
