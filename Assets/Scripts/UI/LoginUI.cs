using Facebook.Unity;
using Firebase.Auth;
using Google;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class LoginUI : MonoBehaviour
{
    public Button FacebookLoginButton, GoogleLoginButton;

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
                // User logged in successfully
                var token = AccessToken.CurrentAccessToken;
                print(token.TokenString);

                var credential = Firebase.Auth.FacebookAuthProvider.GetCredential(token.TokenString);

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
                    Debug.LogFormat("User signed in successfully: {0} ({1})",
                        newUser.DisplayName, newUser.UserId);
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
