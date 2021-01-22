using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConclusionUI : MonoBehaviour
{
    [Header("Sections")]
    public int CurrentStep = 0;
    public GameObject MatchStatus, Rewards, NextRound;

    [Header("Level Up Screen Rewards")]
    public NextLevelXPProgress NextLevelProgress;
    public TMPro.TextMeshProUGUI GainedXP, GainedHints;

    public void Continue()
    {
        if (CurrentStep == 0)
        {
            MatchStatus.SetActive(false);
            Rewards.SetActive(true);
        }
        else if (CurrentStep == 1)
        {
            Rewards.SetActive(false);
            NextRound.SetActive(true);
        }
        else
        {
            // Start next round

        }

        CurrentStep++;
    }
}
