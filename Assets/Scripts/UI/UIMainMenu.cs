using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIMainMenu : SingletonMono<UIMainMenu>
{
    [SerializeField] private TextMeshProUGUI playerNameTxt;

    private void Awake()
    {
        SoundManager.Instance.PlayBackgroundMusic(true);
        playerNameTxt.text = Global.Instance.GameD.NickName;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (NetworkManager.Instance.IsKickFromRoom)
        {
           
            NetworkManager.Instance.IsKickFromRoom = false;
            Global.Instance.GameE.OpenPanelWithData(PanelType.INFORMATION, new object[] { "Disconnected", "You are AFK!\n\n Moved to the Main Menu" });
        }
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.P))
        {
            SoundManager.Instance.PlaySFX(AudioEnum.ButtonClick, 0);
        }
    }

    public void OnFindMatchTap()
    {
        Global.Instance.GameE.ActiveLoadingWithMessage(true, GameConstants.FIND_MATCH_TEXT);
        int selectMap = 2;

        NetworkManager.Instance.StartMatch("", GameplayMode.INFINITY_WAR, selectMap);//UnityEngine.Random.Range(2, 5));
        SoundManager.Instance.PlaySFX(AudioEnum.ButtonClick, 0);
    }

    private void OnDestroy()
    {

    }


    public void Reset()
    {
        DebugUtils.LogColor("Reset HUD", "blue");
    }
}
