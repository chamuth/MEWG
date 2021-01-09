using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Extensions;

public class MatchManager : MonoBehaviour
{
    public WordsBlocksContainer _WordsBlockContainer;
    public SelectionCircle _SelectionCircle;
    public Animator CorrectAnswerAnimator;

    void Start()
    {
        Game.CurrentMatchID = "abcd1234";
        Game.OnMatchDataChanged += () =>
        {
            // Render the things first time only
            _WordsBlockContainer._Words = Game.CurrentMatchData.content.words;
            _WordsBlockContainer.Render();
            _SelectionCircle.Render(Game.CurrentMatchData.content.words);
        };
        Game.WordMatched += (correct, word) =>
        {
            if (correct)
            {
                CorrectAnswerAnimator.Play("Sucess");
                CorrectAnswerAnimator.transform.GetChild(0).gameObject.GetComponent<TMPro.TextMeshProUGUI>().text = word;

                _WordsBlockContainer.MarkWord();
            }
        };
        Game.Watch();
    }

    private void OnDestroy()
    {
        Game.OnMatchDataChanged = null;
        Game.OnFirstData = null;
    }

}
