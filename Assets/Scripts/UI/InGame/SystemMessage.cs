using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class SystemMessage : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;

    private Tween _tween;
    private SystemMessageController _parent;

    public void Init(SystemMessageController parent)
    {
        this._parent = parent;
    }

    public void Active(string msg)
    {
        gameObject.SetActive(true);
        text.SetText(msg);
        text.DOFade(1, 0.0f).Play();
        _tween = DOVirtual.DelayedCall(3.0f, () => {
            text.DOFade(0, 0.25f).OnComplete(() =>
            {
                Deactive();
            }).Play();
        }).Play();
    }

    public void Deactive()
    {
        if (_tween != null)
        {
            _tween.Kill();
            _tween = null;
        }

        gameObject.SetActive(false);
        _parent.BackFreeList(this);
    }

}
