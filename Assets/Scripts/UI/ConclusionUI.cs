using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Firebase.Auth;

public class ConclusionUI : MonoBehaviour
{
    [HideInInspector]
    public MatchState CurrentMatchState;

    [Header("Sections")]
    public int CurrentStep = 0;
    public GameObject MatchStatus, Rewards, NextRound;

    [Header("Level Up Screen Rewards")]
    public NextLevelXPProgress NextLevelProgress;
    public TMPro.TextMeshProUGUI GainedXP, MatchFinalConclusionText;
           
    public void Continue()
    {
        if (CurrentStep == 0)
        {
            var myAnswersCount = Game.CurrentMatchData.matches.Count(x => x.uid == FirebaseAuth.DefaultInstance.CurrentUser.UserId);

            switch(CurrentMatchState)
            {
                case MatchState.Win:
                    MatchFinalConclusionText.text = "Match Won";
                    break;
                case MatchState.Loss:
                    MatchFinalConclusionText.text = "Match Loss";
                    break;
                case MatchState.Draw:
                    MatchFinalConclusionText.text = "Match Draw";
                    break;
            }

            GainedXP.text = string.Format("+{0} XP Gained", myAnswersCount * 25 + ((CurrentMatchState == MatchState.Win) ? 100 : 0));

            NextLevelProgress.CurrentLevel = XP.XPToLevel(User.CurrentUser.xp);

            MatchStatus.SetActive(false);
            Rewards.SetActive(true);
        }
        else if (CurrentStep == 1)
        {
            Rewards.SetActive(false);
            NextRound.SetActive(true);

            // Reset current match data even from here
            Game.CurrentMatchData = new MatchRef() { players = Game.CurrentMatchData.players };
        }

        CurrentStep++;
    }
}
