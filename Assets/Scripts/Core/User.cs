using System;
using System.Linq;
using System.Collections;
using Firebase.Auth;
using Firebase.Database;
using System.Threading.Tasks;

[Serializable]
public class User
{
    public string name;
    public int xp;
    public string profile;
    public UserStatistics statistics;
    public Hints hints;

    public static User CurrentUser = null;
    public static Action OnUserDataUpdated;
    public static Action OnLevelUp;

    static int previousLevel = -1;

    /// <summary>
    /// Updates information about the current user (use if XP is most likely changed)
    /// </summary>
    public static void UpdateCurrentUser()
    {
        var uid = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
        var userRef = FirebaseDatabase.DefaultInstance.RootReference.Child("user").Child(uid);

        userRef.ValueChanged += (x, e) =>
        {
            CurrentUser = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(e.Snapshot.GetRawJsonValue());
            OnUserDataUpdated?.Invoke();

            var currentLevel = XP.XPToLevel(CurrentUser.xp);

            if (previousLevel != -1 && previousLevel < currentLevel)
            {
                OnLevelUp?.Invoke();
            }

            previousLevel = currentLevel;
        };
    }

    public static async Task<User> GetUser(string uid)
    {
        var userRef = FirebaseDatabase.DefaultInstance.RootReference.Child("user").Child(uid);
        var snapshot = await userRef.GetValueAsync();

        return Newtonsoft.Json.JsonConvert.DeserializeObject<User>(snapshot.GetRawJsonValue());
    }
}

[Serializable]
public class UserStatistics
{
    public int wins;
    public int losses;
}

[Serializable]
public class Hints
{
    public int count;
}