using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BasePopup : BasePanel
{
    [SerializeField] private Transform popup;

    private Vector3 _defaultScale;

    public override void Init()
    {
        base.Init();
        _defaultScale = popup.localScale;

    }

    public override void Open()
    {
        if (_isOpen)
        {
            return;
        }

        base.Open();

        popup.localScale = Vector3.zero;
        popup.DOScale(_defaultScale, GameConstants.TIME_FADDING_POPUP).SetEase(Ease.OutBack).SetUpdate(true).Play();
    }
}
