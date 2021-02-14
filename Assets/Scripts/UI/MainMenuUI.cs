using Coffee.UIEffects;
using Firebase.Auth;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Firebase.Database;
using System;
using GooglePlayGames;
using Facebook.Unity;
using GooglePlayGames.BasicApi;
using GoogleMobileAds.Api;

public class MainMenuUI : MonoBehaviour
{
    public UITransitionEffect LoginUI;
    public UITransitionEffect MainMenu;
    public UITransitionEffect MatchmakingUI;
    public UITransitionEffect OpponentModeSelectionUI;
    public UITransitionEffect FriendsUI;

    public GameObject RatePanel;
    public GameObject RemoveAds;
    public StartupTutorial Tutorial;
    public GameObject GiftsUI;
    public GameObject SettingsUI;
    public GameObject ShareReferralUI;
    public GameObject HintsRewardedUI;

    [HideInInspector]
    public static MainMenuUI Instance;

    private void Start()
    {
        foreach(var x in (new UITransitionEffect[] { LoginUI, MainMenu, MatchmakingUI }))
        {
            // Hide everything in the start
            x.effectFactor = 0;
        }

        // Set singleton instance
        Instance = this;

        // Check for user authentication
        var auth = FirebaseAuth.DefaultInstance;

        if (auth.CurrentUser != null)
        {
            print(string.Format("Signed in as : {0} ({1})", auth.CurrentUser.DisplayName, auth.CurrentUser.PhotoUrl.ToString()));

            // Load the main menu
            SwitchMenu("MAIN MENU");
        }
        else
        {
            print("User not logged in");
            // Show the login ui
            SwitchMenu("LOGIN");
        }

        // Make screen awake
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        // Awarded the second time player launched main menu after logging in 
        if (auth.CurrentUser != null && UnityEngine.Random.Range(0,10f) > 8f)
            RewardFreeHints();
    }   

    RewardedInterstitialAd RewardedInterstitialAd;
    string MAIN_MENU_REWARD = "ca-app-pub-5103739755612302/9514713545";

    void RewardFreeHints()
    {
        var load = false;

        if (!GameInstance.AdRewardTimeSet)
            load = true;
        else
            load = (DateTime.Now - GameInstance.LastAdReward).Minutes > 2;

        if (load)
        {
            GameInstance.AdRewardTimeSet = true;
            GameInstance.LastAdReward = DateTime.Now;

            // Create an empty ad request.
            AdRequest request = new AdRequest.Builder().Build();
            // Load the rewarded ad with the request.
            RewardedInterstitialAd.LoadAd(MAIN_MENU_REWARD, request, adLoadCallback);
        }
    }

    private void adLoadCallback(RewardedInterstitialAd ad, string error)
    {
        if (error == null)
        {
            RewardedInterstitialAd = ad;
            RewardedInterstitialAd.Show(userEarnedRewardCallback);
        }
    }

    private void userEarnedRewardCallback(Reward reward)
    {
        StartCoroutine(RewardPlayer());
    }

    IEnumerator RewardPlayer()
    {
        yield return new WaitForSeconds(0.5f);

        var hintReference = FirebaseDatabase.DefaultInstance.RootReference
            .Child("user")
            .Child(FirebaseAuth.DefaultInstance.CurrentUser.UserId)
            .Child("hints")
            .Child("count");

        hintReference.SetValueAsync(User.CurrentUser.hints.count + 1).ContinueWith((s) => { });

        HintsRewardedUI.SetActive(true);
    }

    private void Awake()
    {
        #region Initialize Facebook SDK
        if (!FB.IsInitialized)
        {
            // Initialize the Facebook SDK
            FB.Init(InitCallback, OnHideUnity);
        }
        else
        {
            // Already initialized, signal an app activation App Event
            FB.ActivateApp();
        }
        #endregion

        #region Active Google Play Games SDK
        PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder()
            .RequestServerAuthCode(false /* Don't force refresh */)
            .Build();

        PlayGamesPlatform.InitializeInstance(config);
        PlayGamesPlatform.Activate();
        #endregion
    }

    private void OnHideUnity(bool isGameShown)
    {
        if (!isGameShown)
        {
            // Pause the game - we will need to hide
            Time.timeScale = 0;
        }
        else
        {
            // Resume the game - we're getting focus again
            Time.timeScale = 1;
        }
    }
    private void InitCallback()
    {
        if (FB.IsInitialized)
        {
            // Signal an app activation App Event
            FB.ActivateApp();
        }
        else
        {
            Debug.Log("Failed to Initialize the Facebook SDK");
        }
    }

    public void ShowSettingsUI()
    {
        SettingsUI.SetActive(true);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            // TODO: Sign out from the game dev purposes
            FirebaseAuth.DefaultInstance.SignOut();
            SwitchMenu("LOGIN");
        }
    }

    public void ShareGame()
    {
        ShareReferralUI.SetActive(true);
    }

    public void RateGame()
    {
        PlayerPrefs.SetInt("ALREADY_RATED", 1);
        PlayerPrefs.Save();

        Application.OpenURL("http://play.google.com/store/apps/details?id=com.ninponix.mewg");

        RatePanel.SetActive(false);
    }

    public void OpenLeaderboards()
    {
        if (PlayGamesPlatform.Instance.localUser.authenticated)
        {
            PlayGamesPlatform.Instance.ShowLeaderboardUI("CgkIwfymq44BEAIQAQ");
        }
    }

    public void OpenAchievementsUI()
    {
        if (PlayGamesPlatform.Instance.localUser.authenticated)
        {
            PlayGamesPlatform.Instance.ShowAchievementsUI();
        }
    }

    public void SwitchMenu(string code)
    {
        MatchmakingUI.gameObject.SetActive(false);

        switch (code)
        {
            case "LOGIN":
                StartCoroutine(AnimateUI(LoginUI, true));
                break;
            case "MAIN MENU":
                // Successfully logged in load the main menu
                User.UpdateCurrentUser();
                StartCoroutine(AnimateUI(MainMenu, true));

                // If first time show the tutorial
                if (PlayerPrefs.GetInt("FIRST_TIME", 1) == 1)
                {
                    Tutorial.gameObject.SetActive(true);
                    Tutorial.StartTutorial();

                    PlayerPrefs.SetInt("FIRST_TIME", 0);
                    PlayerPrefs.Save();
                }

                if (PlayerPrefs.GetInt("ALREADY_RATED", 0) == 0 && PlayerPrefs.GetInt("FIRST_TIME", 1) != 1)
                {
                    if (UnityEngine.Random.Range(0,10) > 7)
                    {
                        RatePanel.SetActive(true);
                    }
                }

                // If the player isn't logged in log in on Google Play Games
                if (!PlayGamesPlatform.Instance.IsAuthenticated())
                {
                    Social.localUser.Authenticate((bool success) => { });
                }

                GiftProcessing();

                break;
            case "MATCHMAKING":
                MatchmakingUI.gameObject.SetActive(true);
                StartCoroutine(AnimateUI(MatchmakingUI, true));
                break;
            case "OPPONENT MODES":
                StartCoroutine(AnimateUI(OpponentModeSelectionUI, true));
                break;
        }
    }

    private void GiftProcessing()
    {
        if (PlayerPrefs.GetInt("CONSECUTIVE_DAYS", -1) == -1)
        {
            // A new day
            PlayerPrefs.SetInt("CONSECUTIVE_DAYS", 0);
            print("A NEW DAY");
        }
        else
        {
            // If the player is playing on a different day than the last played one
            if (DateTime.Now.DayOfYear != PlayerPrefs.GetInt("LAST_PLAYED", DateTime.Now.DayOfYear))
            {
                if (DateTime.Now.DayOfYear - PlayerPrefs.GetInt("LAST_PLAYED", DateTime.Now.DayOfYear) == 1)
                {
                    // If now playing on a consecutive day 
                    PlayerPrefs.SetInt("CONSECUTIVE_DAYS", PlayerPrefs.GetInt("CONSECUTIVE_DAYS") + 1);
                }
                else
                {
                    PlayerPrefs.SetInt("CONSECUTIVE_DAYS", 0);
                }


                // Gift
                var cday = PlayerPrefs.GetInt("CONSECUTIVE_DAYS", 0);
                if (cday > 0)
                {
                    // Ready the gift
                    var count = 0;

                    if (cday == 1)
                    {
                        count = 1;
                    }
                    else if (cday == 3)
                    {
                        count = 2;
                    }
                    else if (cday == 5)
                    {
                        count = 3;
                    }
                    else if (cday == 9)
                    {
                        count = 4;
                    }

                    if (count > 0)
                    {
                        GiftsUI.SetActive(true);
                        GiftsUI.GetComponent<GiftsUI>().SetGiftAmount(count);
                    }

                    // Unlock achievements for consecutive days
                    AchievementsManager.ConsecutiveDays(cday + 1);
                }
            }
            else
            {
                print("SAME DAY");
            }
        }

        PlayerPrefs.SetInt("LAST_PLAYED", DateTime.Now.DayOfYear);
        PlayerPrefs.Save();
    }

    DatabaseReference MatchNode;

    public void Play()
    {
        StartMatchmaking();
    }

    public void StartMatchmaking()
    {
        // Show the Matchmaking UI
        SwitchMenu("MATCHMAKING");

        // Add player to the matchmaking queue
        MatchNode = FirebaseDatabase.DefaultInstance.RootReference.Child("matchmaking").Child(FirebaseAuth.DefaultInstance.CurrentUser.UserId);
        MatchNode.SetValueAsync("MATCHMAKING");
        MatchNode.OnDisconnect().SetValue(null);
        MatchNode.ValueChanged += MatchNode_ValueChanged;
    }

    public void StopMatchmaking()
    {
        try
        {
            MatchNode.SetValueAsync(null);
            MatchNode.ValueChanged -= MatchNode_ValueChanged;
        }
        catch (Exception) { }

        SwitchMenu("MAIN MENU");
    }

    private void MatchNode_ValueChanged(object a, ValueChangedEventArgs b)
    {
        if (b.Snapshot.GetValue(false) != null)
        {
            if (Application.isPlaying)
            {
                var gameid = b.Snapshot.GetValue(false).ToString();

                if (gameid != "MATCHMAKING")
                {
                    Game.CurrentMatchData = new MatchRef();
                    Game.CurrentMatchID = gameid;
                    SceneManager.LoadSceneAsync(2);
                }
            }
        }
    }

    private void OnDestroy()
    {
        try
        {
            MatchNode.ValueChanged -= MatchNode_ValueChanged;
        }
        catch (Exception) { };
    }

    public IEnumerator AnimateUI(UITransitionEffect menu, bool set)
    {
        menu.gameObject.SetActive(true);
        menu.effectFactor = (set) ? 0 : 1;

        while ((set && menu.effectFactor < 1) || (!set && menu.effectFactor > 0))
        {
            menu.effectFactor += ((set) ? (+1) : (-1)) * Time.deltaTime * 2f;
            yield return null;
        }

        // If setting something unset all the others after the animation
        if (set)
        {
            foreach (var m in (new UITransitionEffect[] { LoginUI, MainMenu }).Where(x => x != menu))
            {
                m.gameObject.SetActive(false);
            }
        }
    }
}
