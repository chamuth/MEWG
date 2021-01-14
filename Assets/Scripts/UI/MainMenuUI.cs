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

    public static MainMenuUI Instance;

    private void Start()
    {
        // Set singleton instance
        Instance = this;

        // Check for user authentication
        var auth = FirebaseAuth.DefaultInstance;

        if (auth.CurrentUser != null)
        {
            print(string.Format("Signed in as : {0} ({1})", auth.CurrentUser.DisplayName, auth.CurrentUser.PhotoUrl.ToString()));

            // Update user information
            User.UpdateCurrentUser();

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
                StartCoroutine(AnimateUI(MainMenu, true));
                break;
        }
    }

    IEnumerator AnimateUI(UITransitionEffect menu, bool set)
    {
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
