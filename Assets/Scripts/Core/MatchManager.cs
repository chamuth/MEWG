using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Extensions;
using UnityEngine.UI;

public class MatchManager : MonoBehaviour
{
    public WordsBlocksContainer _WordsBlockContainer;
    public SelectionCircle _SelectionCircle;
    public Animator CorrectAnswerAnimator;
    public WinningProgressBar _WinningProgressBar;

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

        Game.OnNewWordMatched += (Ownership owner, WordMatch wordMatch) =>
        {
            CorrectAnswerAnimator.Play("Success");

            CorrectAnswerAnimator.gameObject.GetComponent<Image>().color 
                = ColorManager.Instance.GetColor((owner == Ownership.Ours) ? "FRIENDLY_COLOR" : "ENEMY_COLOR");

            CorrectAnswerAnimator.transform.GetChild(0).gameObject.GetComponent<TMPro.TextMeshProUGUI>().text = wordMatch.word;
        };

        Game.OnMatchedWordsChanged += (List<ProcessedWordMatch> List) =>
        {
            _WordsBlockContainer.WordMatches = List;
            _WordsBlockContainer.MarkCorrectWords();

            #region Update the Scoreboard
            var side1 = 0;
            var side2 = 0;

            foreach (var word in List)
            {
                if (word.owner == Ownership.Ours)
                    side1++;
                else
                    side2++;
            }

            _WinningProgressBar.Score1 = side1;
            _WinningProgressBar.Score2 = side2;
            #endregion
        };

        Game.Watch();
    }

    private void OnDestroy()
    {
        Game.OnMatchDataChanged = null;
        Game.OnFirstData = null;
        Game.OnMatchedWordsChanged = null;
        Game.OnNewWordMatched = null;
    }

}
