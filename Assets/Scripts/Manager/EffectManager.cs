using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EffectManager : SingletonMono<EffectManager>
{
    private PoolManager _poolManager;

    public void Init()
    {
        _poolManager = PoolManager.Instance;

        _poolManager.Init();
    }

    public void Reset()
    {
        StopAllCoroutines();
    }
}
