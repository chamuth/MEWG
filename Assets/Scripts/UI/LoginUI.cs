using Coffee.UIEffects;
using Facebook.Unity;
using Firebase.Auth;
using Firebase.Database;
using Google;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginUI : MonoBehaviour
{
    public MainMenuUI _MainMenuUI;
    public Button FacebookLoginButton, GoogleLoginButton;
    public GameObject PreloaderScreen;
   

    void Start()
    {
        FacebookLoginButton.onClick.AddListener(FacebookLogin);
        GoogleLoginButton.onClick.AddListener(GoogleLogin);
    }

    void FacebookLogin()
    {
        print("Initiating Facebook Login");
        
        PreloaderScreen.SetActive(true);

        FirebaseAuth auth = FirebaseAuth.DefaultInstance;

        var perms = new List<string>() { "public_profile", "email", "user_friends" };
        FB.LogInWithReadPermissions(perms, (ILoginResult result) =>
        {
            if (FB.IsLoggedIn)
            {
                // User logged in successfully
                var token = AccessToken.CurrentAccessToken;

                var credential = FacebookAuthProvider.GetCredential(token.TokenString);

                auth.SignInWithCredentialAsync(credential).ContinueWith(task =>
                {
                    if (task.IsCanceled)
                    {
                        Debug.LogError("SignInWithCredentialAsync was canceled.");
                        return;
                    }

                    if (task.IsFaulted)
                    {
                        Debug.LogError("SignInWithCredentialAsync encountered an error: " + task.Exception);
                        return;
                    }

                    FirebaseUser newUser = task.Result;
                    print("Signed in as " + newUser.DisplayName);

                    var uid = FirebaseAuth.DefaultInstance.CurrentUser.UserId;

                    CreateDatabaseUser(newUser, uid);

                });
            }
            else
            {
                print("Facebook Log in failed");
            }
        });
    }

    void GoogleLogin()
    {
        PreloaderScreen.SetActive(true);

        Social.localUser.Authenticate((bool success) => {
            if (success)
            {
                var authCode = PlayGamesPlatform.Instance.GetServerAuthCode();

                FirebaseAuth auth = FirebaseAuth.DefaultInstance;
                Credential credential = PlayGamesAuthProvider.GetCredential(authCode);

                auth.SignInWithCredentialAsync(credential).ContinueWith(task => {
                    if (task.IsCanceled)
                    {
                        Debug.LogError("GPG: SignInWithCredentialAsync was canceled.");
                        return;
                    }
                    if (task.IsFaulted)
                    {
                        Debug.LogError("GPG: SignInWithCredentialAsync encountered an error: " + task.Exception);
                        return;
                    }

                    FirebaseUser newUser = task.Result;
                    print("Signed in as " + newUser.DisplayName);

                    var uid = FirebaseAuth.DefaultInstance.CurrentUser.UserId;

                    CreateDatabaseUser(newUser, uid);
                });
            }
        });
    }

    void CreateDatabaseUser(FirebaseUser newUser, string uid)
    {
        FirebaseDatabase.DefaultInstance.RootReference.Child("user").Child(uid).GetValueAsync().ContinueWith((snapshot) =>
        {
            if (!snapshot.Result.Exists)
            {
                print("User profile doesn't exist on database");

                // User details doesn't exist save them
                var user = new User();
                user.name = newUser.DisplayName;
                user.profile = newUser.PhotoUrl.ToString();
                user.xp = 0;

                // User gonna have 0 wins and 0 losses when started
                user.statistics = new UserStatistics();
                user.statistics.wins = 0;
                user.statistics.losses = 0;

                // User gonna have free 5 hints as a startup gift
                user.hints = new Hints();
                user.hints.count = 5;

                FirebaseDatabase.DefaultInstance.RootReference.Child("user").Child(uid).SetRawJsonValueAsync(Newtonsoft.Json.JsonConvert.SerializeObject(user)).ContinueWith((t) =>
                {
                    print("User Profile created, loading main menu");
                    // Load the main menu
                    mmSwitch = true;
                });
            }
            else
            {
                print("User profile exists on the database loading main menu");
                mmSwitch = true;
            }
        });
    }

    private bool mmSwitch = false; 

    private void Update()
    {
        if (mmSwitch)
        {
            MainMenuUI.Instance.SwitchMenu("MAIN MENU");
            PreloaderScreen.SetActive(false);
            mmSwitch = false;
        }
    }

}
