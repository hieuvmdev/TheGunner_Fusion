using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InformationPanel : BasePanel
{
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private TextMeshProUGUI message;

    public override void Init()
    {
        base.Init();

        
    }

    public override void SetData(object[] data)
    {
        base.SetData(data);

        title.SetText((string)data[0]);
        message.SetText((string)data[1]);
    }


}
