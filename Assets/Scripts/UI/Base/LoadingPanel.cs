using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LoadingPanel : BasePanel
{
    public TextMeshProUGUI message;

    public override void Init()
    {
        base.Init();

        Global.Instance.GameE.ActiveLoading += (isActive) =>
        {
            if (isActive)
            {
                Open();
                message.enabled = false;
            }
            else
            {
                Close();
            }
        };

        Global.Instance.GameE.ActiveLoadingWithMessage += (isActive, msg) =>
        {
            if (isActive)
            {
                Open();
                message.enabled = true;
                message.SetText(msg);
            }
            else
            {
                Close();
            }
        };
    }

    public override void Open()
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

     //   _cacheShareUi = _shareUi.IsActive;
      //  _shareUi.SetActive(IsActiveShareUI);

        FadeIn();

        AfterOpen();
    }

    public override void Close()
    {
        Debug.Log("Close");

        if (!_isOpen)
        {
            return;
        }

        _isOpen = false;
      //  _shareUi.SetActive(_cacheShareUi);
        UIReset();
        FadeOut();

    }
}
