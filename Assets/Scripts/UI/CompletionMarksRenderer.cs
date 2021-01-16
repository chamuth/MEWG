using Firebase.Auth;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompletionMarksRenderer : MonoBehaviour
{
    public GameObject CorrectIcon;
    public GameObject IncorrectIcon;

    public TMPro.TextMeshProUGUI WordsCounterText;

    public void Start()
    {
        var matches = Game.CurrentMatchData.matches;
        int correct = 0;

        foreach(var match in matches)
        {
            var g = (match.uid == FirebaseAuth.DefaultInstance.CurrentUser.UserId) ? CorrectIcon : IncorrectIcon;
            Instantiate(g, transform);

            if (match.uid == FirebaseAuth.DefaultInstance.CurrentUser.UserId)
                correct++;
        }

        WordsCounterText.text = string.Format("{0}/{1} Words", correct, matches.Length);
    }
}
