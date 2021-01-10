using Coffee.UIEffects;
using Firebase.Auth;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuUI : MonoBehaviour
{
    public UITransitionEffect LoginUI;

    private void Start()
    {
        // Check for user authentication
        var auth = FirebaseAuth.DefaultInstance;

        if (auth.CurrentUser != null)
        {
            print(string.Format("Signed in as : {0} ({1})", auth.CurrentUser.DisplayName, auth.CurrentUser.PhotoUrl.ToString()));

            // Load the main menu
            
        }
        else
        {
            print("User not logged in");
            // Show the login ui
            StartCoroutine(AnimateUI(LoginUI, true));
        }
    }

    IEnumerator AnimateUI(UITransitionEffect menu, bool set)
    {
        while ((set && menu.effectFactor < 1) || (!set && menu.effectFactor > 0))
        {
            menu.effectFactor += ((set) ? (+1) : (-1)) * Time.deltaTime * 2f;
            yield return null;
        }
    }
}
