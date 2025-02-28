public enum PlayerState
{
    New,
    Appear,
    Active,
    Dead
}

[System.Serializable]
public enum BulletType
{
    Normal, Power, Bounce
}

public enum ScoreType
{
    Kill,
    Capture_Flag,
}

public enum GameplayMode

{
    INFINITY_WAR,
}

public enum DamageEffectType
{
    NONE, POISON, BURN
}
