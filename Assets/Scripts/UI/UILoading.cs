using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UILoading : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(IEWaitInit());
    }

    private IEnumerator IEWaitInit()
    {
        yield return new WaitForSeconds(1.0f);
        SceneManager.LoadScene(GameConstants.MAINMENU_SCENE_INDEX);
    }
}
