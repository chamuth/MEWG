using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Extensions;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Firebase.Database;
using Firebase.Auth;
using GooglePlayGames;
using GoogleMobileAds.Api;

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
    bool OnceStatusGiven = false;

    InterstitialAd MatchEndingInterstitialAd;
    string MATCH_ENDING_INTERSTITIAL_AD_ID = "ca-app-pub-5103739755612302/6783000198";

    void Start()
    {
        OurPlayerName.text = FirebaseAuth.DefaultInstance.CurrentUser.DisplayName;
        
        ResetProgressbar();

        Game.OnMatchDataChanged += () =>
        {
            if (Game.CurrentMatchData != null)
            {
                // Render the things first time only
                _WordsBlockContainer._Words = Game.CurrentMatchData.content.words;
                _WordsBlockContainer.Render();
                _SelectionCircle.Render(Game.CurrentMatchData.content.words);

                if (Game.CurrentMatchData.matches != null && Game.CurrentMatchData.content != null)
                {
                    if (Game.CurrentMatchData.matches.Length == Game.CurrentMatchData.content.words.Length)
                    {
                        // match has ended
                        MatchEndingPreloader.SetActive(true);
                    }
                }
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
            if (!OnceStatusGiven)
            {
                OnceStatusGiven = true; 

                MatchEndingPreloader.SetActive(false);
                ConclusionUI.SetActive(true);
                ConclusionUI.gameObject.GetComponent<ConclusionUI>().CurrentMatchState = state;

                switch (state)
                {
                    case MatchState.Win:
                        WinUI.SetActive(true);
                        WinParticles.Play();
                        SoundManager.Instance.PlayClip("WIN");
                        break;
                    case MatchState.Loss:
                        LossUI.SetActive(true);
                        break;
                    case MatchState.Draw:
                        DrawUI.SetActive(true);
                        break;
                }

                if (PlayGamesPlatform.Instance.localUser.authenticated)
                {
                    // Report to XP leaderboard
                    PlayGamesPlatform.Instance.ReportScore(User.CurrentUser.xp, "CgkIwfymq44BEAIQAQ", (b) => { });
                    // Report to W/L leaderboard, which is measured in kilo metrics, multiplied by 1000
                    float wl = (User.CurrentUser.statistics.losses == 0) ? User.CurrentUser.statistics.wins : (User.CurrentUser.statistics.wins / (float)User.CurrentUser.statistics.losses);
                    PlayGamesPlatform.Instance.ReportScore((int)(wl * 1000), "CgkIwfymq44BEAIQAQ", (b) => { });
                }


                // Show the ad
                StartCoroutine(ShowMatchEndingAd());
            }
        };

        // Start the match
        Game.Watch();

        // Load the ads
        LoadAds();
    }

    void LoadAds()
    {
        MatchEndingInterstitialAd = new InterstitialAd(MATCH_ENDING_INTERSTITIAL_AD_ID);
        // Create an empty ad request.
        AdRequest request = new AdRequest.Builder().Build();
        // Load the interstitial with the request.
        MatchEndingInterstitialAd.LoadAd(request);
    }
    
    IEnumerator ShowMatchEndingAd()
    {
        yield return new WaitForSeconds(1);

        if (PlayerPrefs.GetInt("REMOVE_ADS", 0) == 1)
        {
            // If the admob ad is actually loaded
            if (MatchEndingInterstitialAd.IsLoaded())
            {
                MatchEndingInterstitialAd.Show();
            }
        }
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
        Game.OnMatchEnd = null;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !Exiting)
        {
            // Android back key pressed
            ExitNotification.SetActive(!ExitNotification.activeSelf);
        }

        if (MatchTimeCounter.Timer <= 0)
        {
            // Time has ran out
            MatchEndingPreloader.SetActive(true);
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
    