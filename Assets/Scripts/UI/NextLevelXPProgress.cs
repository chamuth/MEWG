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
        while (ProgressElement.fillAmount < 0.99)
        {
            ProgressElement.fillAmount = Mathf.Lerp(ProgressElement.fillAmount, 1, Time.deltaTime * 2f);
            yield return null;
        }

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
