using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashUI : MonoBehaviour
{
    public CanvasGroup FadeOuter;

    void Start()
    {
        Application.targetFrameRate = 60;
        StartCoroutine(FadeOutAfter(5));
    }

    IEnumerator FadeOutAfter(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        while (FadeOuter.alpha < 1)
        {
            FadeOuter.alpha = Mathf.MoveTowards(FadeOuter.alpha, 1, Time.deltaTime * 4f);
            yield return null;
        }

        // Load the next scene
        SceneManager.LoadSceneAsync(1);
    }
}
