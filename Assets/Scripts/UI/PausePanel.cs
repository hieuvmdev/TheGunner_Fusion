using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PausePanel : BasePanel
{
    public void OnResumeButtonTap()
    {
        Close();
    }

    protected override void AfterOpen()
    {
        base.AfterOpen();

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Close();
        }
    }


    public override void UIReset()
    {
        base.UIReset();

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void OnBackHomeButtonTap()
    {
        SoundManager.Instance.PlaySFX(AudioEnum.ButtonClick, 0);
        Global.Instance.GameE.OnDisconnect?.Invoke(false);
    }
}
