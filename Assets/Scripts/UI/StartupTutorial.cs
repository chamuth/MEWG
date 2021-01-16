using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class StartupTutorial : MonoBehaviour
{
    public CanvasGroup[] Steps;
    public int CurrentStep = 0;

    private void Start()
    {
        StartTutorial();
    }

    public void StartTutorial()
    {
        print("FIRST TIME STARTING TUTORIAL");

        foreach(var step in Steps)
        {
            step.alpha = 0;
            step.gameObject.SetActive(false);
        }

        StartCoroutine(SwitchStep());
    }

    public void Next()
    {
        if (CurrentStep + 1 < Steps.Length)
        {
            CurrentStep++;
            StartCoroutine(SwitchStep());
        }
        else
        {
            // Tutorial ended
            StartCoroutine(EndTutorial());
        }
    }

    IEnumerator EndTutorial()
    {
        while (Steps[CurrentStep].alpha > 0)
        {
            Steps[CurrentStep].alpha = Mathf.MoveTowards(Steps[CurrentStep].alpha, 0, Time.deltaTime * 3f);
            yield return null;
        }

        // Hide myself
        gameObject.SetActive(false);
    }

    IEnumerator SwitchStep()
    {
        // Hide the previous step if there is a previous step
        if (CurrentStep > 0)
        {
            while(Steps[CurrentStep - 1].alpha > 0)
            {
                Steps[CurrentStep - 1].alpha = Mathf.MoveTowards(Steps[CurrentStep - 1].alpha, 0, Time.deltaTime * 3f);
                yield return null;
            }

            // Hide the previous step
            Steps[CurrentStep - 1].gameObject.SetActive(false);
        }

        Steps[CurrentStep].gameObject.SetActive(true);

        // Show the next step
        while (Steps[CurrentStep].alpha < 1)
        {
            Steps[CurrentStep].alpha = Mathf.MoveTowards(Steps[CurrentStep].alpha, 1, Time.deltaTime * 3f);
            yield return null;
        }
    }
}
