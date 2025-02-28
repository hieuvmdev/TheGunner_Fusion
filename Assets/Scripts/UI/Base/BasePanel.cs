using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BasePanel : MonoBehaviour
{
    [SerializeField] private PanelType type;
    [SerializeField] private CanvasGroup canvasGroup;

    public bool IsOpen
    {
        get { return _isOpen; }
    }
    public PanelType Type
    {
        get { return type; }
    }

    protected bool _isOpen;
    protected bool _isInit;


    public virtual void Init()
    {
        _isInit = true;
    }

    public virtual void SetData(object[] data)
    {

    }

    public virtual void Open()
    {
        if (_isOpen)
        {
            return;
        }

        if (!_isInit)
        {
            Init();
        }

        _isOpen = true;
        FadeIn();

        AfterOpen();
    }

    protected virtual void AfterOpen()
    {

    }

    protected virtual void AfterShowed()
    {

    }

    public virtual void Close()
    {
        //Debug.Log("Close");

        if (!_isOpen)
        {
            return;
        }
        //SoundManager.Instance.PlayClosePopup();

        _isOpen = false;
        UIReset();
        FadeOut();

    }

    public virtual void ForceClose()
    {
        _isOpen = false;
        UIReset();
        gameObject.SetActive(false);
    }

    public virtual void UIReset()
    {

    }

    protected void FadeIn()
    {
        gameObject.SetActive(true);
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0;
            canvasGroup.DOFade(1, GameConstants.TIME_FADDING_POPUP).OnComplete(() => { AfterShowed(); }).SetUpdate(true).Play();
        }
    }

    protected void FadeOut()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1;
            canvasGroup.DOFade(0, GameConstants.TIME_FADDING_POPUP).SetUpdate(true).OnComplete(() =>
            {
                gameObject.SetActive(false);
            }).Play();
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
