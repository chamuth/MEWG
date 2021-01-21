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

    private void Start()
    {
        StartCoroutine(AnimateNextLevel());
    }
    
    IEnumerator AnimateNextLevel()
    {
        // Set progress to 0
        ProgressElement.fillAmount = 0;
        LevelText.text = (CurrentLevel - 1).ToString();

        // Wait for the reward animation to finish
        yield return new WaitForSeconds(0.6f);

        // Animate the progress
        while (ProgressElement.fillAmount < 1)
        {
            ProgressElement.fillAmount = Mathf.MoveTowards(ProgressElement.fillAmount, 1, Time.deltaTime * 5f);
            yield return null;
        }

        ProgressElement.fillAmount = 0;
        LevelText.text = CurrentLevel.ToString();

        // Animate the progress
        while (ProgressElement.fillAmount < CurrentLevelProgress)
        {
            ProgressElement.fillAmount = Mathf.MoveTowards(ProgressElement.fillAmount, CurrentLevelProgress, Time.deltaTime * 5f);
            yield return null;
        }
    }
}
