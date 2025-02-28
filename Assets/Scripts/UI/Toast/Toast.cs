using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;

public class Toast : MonoBehaviour
{
    public RectTransform backgroundTransform;
    public RectTransform messageTransform;

    [HideInInspector]
    public bool isShowing = false;

    private Queue<AToast> queue = new Queue<AToast>();

    private class AToast
    {
        public string msg;
        public float time;
        public AToast(string msg, float time)
        {
            this.msg = msg;
            this.time = time;
        }
    }

    public void Init()
    {
        SetEnabled(false);
    }

    public void SetMessage(string msg)
    {
        messageTransform.GetComponent<TextMeshProUGUI>().text = msg;
#if UNITY_WEBGL
        DOVirtual.DelayedCall(0.0f, () =>
        {
            backgroundTransform.sizeDelta = new Vector2(messageTransform.GetComponent<TextMeshProUGUI>().preferredWidth + 20, backgroundTransform.sizeDelta.y);
        }).Play();
#endif
    }

    private void Show(AToast aToast)
    {
        SetMessage(aToast.msg);
        SetEnabled(true);
        GetComponent<Animator>().SetBool("show", true);
        Invoke("Hide", aToast.time);
        isShowing = true;
    }

    public void ShowMessage(string msg, float time = 1.5f)
    {
        AToast aToast = new AToast(msg, time);
        queue.Enqueue(aToast);
        ShowOldestToast();
    }

    private void Hide()
    {
        GetComponent<Animator>().SetBool("show", false);
        Invoke("CompleteHiding", 1);
    }

    private void CompleteHiding()
    {
        SetEnabled(false);
        isShowing = false;
        ShowOldestToast();
    }

    private void ShowOldestToast()
    {
        if (queue.Count == 0) return;
        if (isShowing) return;

        AToast current = queue.Dequeue();
        Show(current);
    }

    private void SetEnabled(bool enabled)
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(enabled);
        }
    }
}
