using Firebase.Database;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static class Game
{
    public static string CurrentMatchID = null;
    public static MatchRef CurrentMatchData = null;

    public static Action OnMatchDataChanged;
    public static Action OnFirstData;
    public static Action<bool, string> WordMatched;

    static DatabaseReference MatchReference;
    
    public static void Watch()
    { 
        DatabaseReference db = FirebaseDatabase.DefaultInstance.RootReference;
        MatchReference = db.Child("match").Child(CurrentMatchID);

        MatchReference.GetValueAsync().ContinueWith((snapshot) =>
        {
            CurrentMatchData = JsonUtility.FromJson<MatchRef>(snapshot.Result.GetRawJsonValue());
            OnFirstData?.Invoke();
        });

        MatchReference.ValueChanged += MatchReference_ValueChanged;
    }

    private static void MatchReference_ValueChanged(object sender, ValueChangedEventArgs e)
    {
        CurrentMatchData = JsonUtility.FromJson<MatchRef>(e.Snapshot.GetRawJsonValue());
        OnMatchDataChanged?.Invoke();
    }

    public static void MatchWord(string word)
    {
        var correct = CurrentMatchData.content.words.Contains(word);
        WordMatched?.Invoke(correct, word);
    }
}

[Serializable]
public class MatchRef
{
    public string[] players;
    public MatchContent content;
}

[Serializable]
public class MatchContent
{
    public string[] words;
}