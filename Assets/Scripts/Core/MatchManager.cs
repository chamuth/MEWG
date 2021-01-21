using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Extensions;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Firebase.Database;
using Firebase.Auth;

public class MatchManager : MonoBehaviour
{
    public WordsBlocksContainer _WordsBlockContainer;
    public SelectionCircle _SelectionCircle;
    public Animator CorrectAnswerAnimator;
    public WinningProgressBar _WinningProgressBar;

    public TMPro.TextMeshProUGUI OurPlayerName;
    public TMPro.TextMeshProUGUI TheirPlayerName;

    public GameObject ConclusionUI;
    public GameObject WinUI;
    public ParticleSystem WinParticles;
    public GameObject LossUI;
    public GameObject DrawUI;

    public ProfileItem OurProfileItem;
    public ProfileItem TheirProfileItem;

    public TimeCounter MatchTimeCounter;

    public GameObject ExitNotification;
    public GameObject ExitPreloader;

    public GameObject MatchEndingPreloader;

    bool Exiting = false;
    bool Exited = false;

    void Start()
    {
        OurPlayerName.text = FirebaseAuth.DefaultInstance.CurrentUser.DisplayName;
        
        ResetProgressbar();

        Game.OnMatchDataChanged += () =>
        {
            // Render the things first time only
            _WordsBlockContainer._Words = Game.CurrentMatchData.content.words;
            _WordsBlockContainer.Render();
            _SelectionCircle.Render(Game.CurrentMatchData.content.words);

            if (Game.CurrentMatchData.matches.Length == Game.CurrentMatchData.content.words.Length)
            {
                // match has ended
                MatchEndingPreloader.SetActive(true);
            }
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

        Game.OnMatchEnd += (MatchState state) =>
        {
            MatchEndingPreloader.SetActive(false);
            ConclusionUI.SetActive(true);

            switch (state)
            {
                case MatchState.Win:
                    WinUI.SetActive(true);
                    WinParticles.Play();
                    break;
                case MatchState.Loss:
                    LossUI.SetActive(true);
                    break;
                case MatchState.Draw:
                    DrawUI.SetActive(true);
                    break;
            }
        };

        // Start the match
        Game.Watch();
    }

    void ResetProgressbar()
    {
        _WinningProgressBar.Score1 = 0;
        _WinningProgressBar.Score2 = 0;
    }

    private void OnDestroy()
    {
        Game.Destroy();
        
        Game.OnMatchDataChanged = null;
        Game.OnFirstData = null;
        Game.OnMatchedWordsChanged = null;
        Game.OnNewWordMatched = null;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !Exiting)
        {
            // Android back key pressed
            ExitNotification.SetActive(!ExitNotification.activeSelf);
        }

        if (Exited)
        {
            Exited = false;
            SceneManager.LoadSceneAsync(1);
        }
    }

    public void ShowExitPanel()
    {
        if (!Exiting)
            ExitNotification.SetActive(true);
    }

    public void ReturnToHome()
    {
        Exiting = true;
        ExitPreloader.SetActive(true);

        // Remove the matchmaking thing
        FirebaseDatabase.DefaultInstance.RootReference.Child("matchmaking").Child(FirebaseAuth.DefaultInstance.CurrentUser.UserId).SetValueAsync(null).ContinueWith((t) =>
        {
            // set the disconnect variable
            FirebaseDatabase.DefaultInstance.RootReference.Child("match").Child(Game.CurrentMatchID).Child("disconnect").SetValueAsync(FirebaseAuth.DefaultInstance.CurrentUser.UserId).ContinueWith((b) =>
            {
                Exited = true;
            });
        });
    }

}
    