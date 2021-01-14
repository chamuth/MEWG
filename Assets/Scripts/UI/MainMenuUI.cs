using Coffee.UIEffects;
using Firebase.Auth;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    public UITransitionEffect LoginUI;
    public UITransitionEffect MainMenu;
    public UITransitionEffect MatchmakingUI;

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

    public void SwitchMenu(string code)
    {
        switch (code)
        {
            case "LOGIN":
                StartCoroutine(AnimateUI(LoginUI, true));
                break;
            case "MAIN MENU":
                // Successfully logged in load the main menu
                User.UpdateCurrentUser();

                StartCoroutine(AnimateUI(MainMenu, true));
                break;
            case "MATCHMAKING":
                StartCoroutine(AnimateUI(MatchmakingUI, true));
                break;
        }
    }
    
    public void StartMatchmaking()
    {
        print("STARTING MATCHMAKING");
        // Show the Matchmaking UI
        SwitchMenu("MATCHMAKING");

        StartCoroutine(StartTestMatch());
    }

    IEnumerator StartTestMatch()
    {
        yield return new WaitForSeconds(4);

        // Set test match id
        Game.CurrentMatchID = "abcd1234";
        SceneManager.LoadScene(1);
    }

    IEnumerator AnimateUI(UITransitionEffect menu, bool set)
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
