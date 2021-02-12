using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NextLevelXPProgress : MonoBehaviour
{
    public int CurrentLevel = 3;
    public float CurrentLevelProgress = 0.45f;
    public Image ProgressElement;
    public TMPro.TextMeshProUGUI LevelText;

    public void Setup()
    {
        StartCoroutine(AnimateNextLevel());
    }
    
    IEnumerator AnimateNextLevel()
    {
        ProgressElement.fillAmount = 0;
        LevelText.text = CurrentLevel.ToString();

        // Animate the progress
        while (ProgressElement.fillAmount < CurrentLevelProgress - 0.05f)
        {
            ProgressElement.fillAmount = Mathf.Lerp(ProgressElement.fillAmount, CurrentLevelProgress, Time.deltaTime * 1f);
            yield return null;
        }
    }
}
