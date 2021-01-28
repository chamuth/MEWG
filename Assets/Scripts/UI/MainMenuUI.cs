using Coffee.UIEffects;
using Firebase.Auth;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Firebase.Database;
using System;

public class MainMenuUI : MonoBehaviour
{
    public UITransitionEffect LoginUI;
    public UITransitionEffect MainMenu;
    public UITransitionEffect MatchmakingUI;

    public GameObject RatePanel;
    public GameObject RemoveAds;
    public StartupTutorial Tutorial;

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
    }

    public void ShowRemoveAdsMenu()
    {
        RemoveAds.SetActive(true);
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
        var sharer = new NativeShare();

        sharer.SetSubject("Checkout this cool game!");
        sharer.SetText("Play Multiplayer English Word-matching Game for Free http://play.google.com/store/apps/details?id=com.ninponix.mewg");

        sharer.Share();
    }

    public void RateGame()
    {
        Application.OpenURL("http://play.google.com/store/apps/details?id=com.ninponix.mewg");
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
                    //PlayerPrefs.Save();
                }

                if (PlayerPrefs.GetInt("ALREADY_RATED", 0) == 0 && PlayerPrefs.GetInt("FIRST_TIME", 1) != 1)
                {
                    if (UnityEngine.Random.Range(0,10) > 7)
                    {
                        RatePanel.SetActive(true);
                    }
                }

                break;
            case "MATCHMAKING":
                MatchmakingUI.gameObject.SetActive(true);
                StartCoroutine(AnimateUI(MatchmakingUI, true));
                break;
        }
    }

    DatabaseReference MatchNode;

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
