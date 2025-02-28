using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SystemMessageController : MonoBehaviour
{
    [SerializeField] private List<SystemMessage> systemMessages;
    [SerializeField] private Transform container;

    private List<SystemMessage> _freeMessage;
    private List<SystemMessage> _showMessage;

    public void Init()
    {
        _freeMessage = new List<SystemMessage>();
        _showMessage = new List<SystemMessage>();

        for (int i = 0; i < systemMessages.Count; i++)
        {
            systemMessages[i].Init(this);
            _freeMessage.Add(systemMessages[i]);
        }
    }

    public void ShowMessage(string txt)
    {
        if (_freeMessage.Count == 0)
        {
            _showMessage[0].Deactive();
        }

        SystemMessage msg = _freeMessage[0];
        _freeMessage.Remove(msg);
        _showMessage.Add(msg);

        msg.Active(txt);
        msg.transform.SetAsFirstSibling();
    }

    public void BackFreeList(SystemMessage msg)
    {
        if (!_freeMessage.Contains(msg))
        {
            _freeMessage.Add(msg);
        }
     
        if (_showMessage.Contains(msg))
        {
            _showMessage.Remove(msg);
        }
     
    }

    public void ResetList()
    {
        for (int i = 0; i < systemMessages.Count; i++)
        {
            systemMessages[i].Deactive();
        }
    }
}
