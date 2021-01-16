using Coffee.UIEffects;
using Facebook.Unity;
using Firebase.Auth;
using Firebase.Database;
using Google;
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

    void Awake()
    {
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

    void Start()
    {
        FacebookLoginButton.onClick.AddListener(FacebookLogin);
        GoogleLoginButton.onClick.AddListener(GoogleLogin);
    }

    void FacebookLogin()
    {
        print("Initiating Facebook Login");

        FirebaseAuth auth = FirebaseAuth.DefaultInstance;

        var perms = new List<string>() { "public_profile", "email" };
        FB.LogInWithReadPermissions(perms, (ILoginResult result) =>
        {
            if (FB.IsLoggedIn)
            {
                PreloaderScreen.SetActive(true);

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

                            // User gonna have 5 hints as a startup gift
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

                });
            }
            else
            {
                print("Facebook Log in failed");
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

    void GoogleLogin()
    {
        print("Initiating Google Login");

        //FirebaseAuth auth = FirebaseAuth.DefaultInstance;

        //GoogleSignIn.Configuration = new GoogleSignInConfiguration
        //{
        //    RequestIdToken = true,
        //    WebClientId = "38208650817-8crs3a8417nuvjfemkshqurnd3cc2n1p.apps.googleusercontent.com"
        //};

        //Task<GoogleSignInUser> signIn = GoogleSignIn.DefaultInstance.SignIn();

        //TaskCompletionSource<FirebaseUser> signInCompleted = new TaskCompletionSource<FirebaseUser>();
        //signIn.ContinueWith(task => {
        //    if (task.IsCanceled)
        //    {
        //        signInCompleted.SetCanceled();
        //    }
        //    else if (task.IsFaulted)
        //    {
        //        signInCompleted.SetException(task.Exception);
        //    }
        //    else
        //    {
        //        Credential credential = Firebase.Auth.GoogleAuthProvider.GetCredential(((Task<GoogleSignInUser>)task).Result.IdToken, null);
        //        auth.SignInWithCredentialAsync(credential).ContinueWith(authTask => {
        //            if (authTask.IsCanceled)
        //            {
        //                signInCompleted.SetCanceled();
        //            }
        //            else if (authTask.IsFaulted)
        //            {
        //                signInCompleted.SetException(authTask.Exception);
        //            }
        //            else
        //            {
        //                var result = ((Task<FirebaseUser>)authTask).Result;
        //                signInCompleted.SetResult(result);
        //                Debug.LogFormat("User signed in successfully: {0} ({1})", result.DisplayName, result.UserId);
        //            }
        //        });
        //    }
        //});
    }
}
