using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FpsDisplay : MonoBehaviour
{
    float deltaTime = 0.0f;

    public Text FpsTxt;

    private float minFps = 60;
    private float maxFps = 0;
    private bool isShow = false;

    float msec = 0;
    float fps = 0;

    void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        msec = deltaTime * 1000.0f;
        fps = 1.0f / deltaTime;
        FpsTxt.text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);
    }
}