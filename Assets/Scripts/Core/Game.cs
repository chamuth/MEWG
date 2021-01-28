using Firebase.Database;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Networking;
using Firebase.Auth;
using UnityEngine.SceneManagement;

public static class Game
{
    public static string CurrentMatchID = null;
    public static MatchRef CurrentMatchData = null;

    public static Action OnMatchDataChanged;
    public static Action OnFirstData;
    public static Action<List<ProcessedWordMatch>> OnMatchedWordsChanged;
    public static Action<Ownership, WordMatch> OnNewWordMatched;
    public static Action OnEnemyDataLoaded;
    public static Action<MatchState> OnMatchEnd;

    public static DatabaseReference MatchReference;

    static WordMatch[] previousWordMatch = new WordMatch[] { };

    public static User CurrentEnemy = null;

    static string enemyId = "";

    public static void Watch()
    {
        DatabaseReference db = FirebaseDatabase.DefaultInstance.RootReference;
        MatchReference = db.Child("match").Child(CurrentMatchID);

        // On disconnect save the player disconnected field
        MatchReference.Child("disconnect").OnDisconnect().SetValue(FirebaseAuth.DefaultInstance.CurrentUser.UserId);

        MatchReference.GetValueAsync().ContinueWith((snapshot) =>
        {
            CurrentMatchData = JsonUtility.FromJson<MatchRef>(snapshot.Result.GetRawJsonValue());

            // Load enemy player details
            User.GetUser(CurrentMatchData.players.First(x => x != FirebaseAuth.DefaultInstance.CurrentUser.UserId)).ContinueWith((x) =>
            {
                CurrentEnemy = x.Result;
                // Save the enemy id
                enemyId = CurrentMatchData.players.First((z) => z != FirebaseAuth.DefaultInstance.CurrentUser.UserId);
            });

            OnFirstData?.Invoke();
        });

        MatchReference.Child("disconnect").ValueChanged += DisconectValueChanged;
        MatchReference.ValueChanged += MatchReference_ValueChanged;

        UpdateMatchedWords();
    }

    private static void DisconectValueChanged(object sender, ValueChangedEventArgs e)
    {
        if (e.Snapshot.Exists)
        {
            // If disconnect value exists
            var playerId = e.Snapshot.Value.ToString();

            if (playerId == enemyId)
            {
                UniversalUI.Instance.ShowPlayerDisconnectMessage();

                // Enemy player has disconnected
                SceneManager.LoadSceneAsync(1);
            }
        }
    }

    public static void Destroy()
    {
        try
        {
            // Remove value changed reference
            MatchReference.ValueChanged -= MatchReference_ValueChanged;
        }
        catch (Exception) { }
        
        MatchReference = null;
    }

    private static void MatchReference_ValueChanged(object sender, ValueChangedEventArgs e)
    {
        CurrentMatchData = JsonUtility.FromJson<MatchRef>(e.Snapshot.GetRawJsonValue());

        try
        {
            if (CurrentMatchData.matches != null)
            {
                if (previousWordMatch != CurrentMatchData.matches)
                {
                    UpdateMatchedWords();
                }
            }
        }
        catch (Exception) { }

        try
        {
            if (CurrentMatchData.status != null)
            {
                if (CurrentMatchData.status.draw)
                {
                    OnMatchEnd?.Invoke(MatchState.Draw);
                }

                if (CurrentMatchData.status.winner != "")
                {
                    MatchState state;

                    if (CurrentMatchData.status.draw == true)
                    {
                        state = MatchState.Draw;
                    }
                    else
                    {
                        if (CurrentMatchData.status.winner == FirebaseAuth.DefaultInstance.CurrentUser.UserId)
                            state = MatchState.Win;
                        else
                            state = MatchState.Loss;
                    }

                    OnMatchEnd?.Invoke(state);
                }
            }
        }
        catch (Exception) { }

        OnMatchDataChanged?.Invoke();
    }

    static void UpdateMatchedWords()
    {
        if (CurrentMatchData != null)
        {
            if (CurrentMatchData.matches != null)
            {
                var list = new List<ProcessedWordMatch>();

                // Check for each words
                foreach (var wordmatch in CurrentMatchData.matches)
                {
                    if (!previousWordMatch.Contains(wordmatch))
                    {
                        // new word match detected
                        if (Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser.UserId != wordmatch.uid)
                        {
                            // Not a word match from the user
                            OnNewWordMatched?.Invoke(Ownership.Theirs, wordmatch);
                        }
                        else
                        {
                            // Word match from the user
                            OnNewWordMatched?.Invoke(Ownership.Ours, wordmatch);
                        }
                    }

                    list.Add(new ProcessedWordMatch
                    {
                        owner = (Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser.UserId != wordmatch.uid) ? Ownership.Theirs : Ownership.Ours,
                        word = wordmatch.word
                    });
                }

                OnMatchedWordsChanged?.Invoke(list);
                previousWordMatch = CurrentMatchData.matches;
            }
        }
    }

    public static void MatchWord(string word)
    {
        // If the word is a given word and it is not previously found
        var actualWord = CurrentMatchData.content.words.Contains(word);
        var original = true;
        
        // If theres previously found words scan them for originals
        if (CurrentMatchData.matches != null)
            if (CurrentMatchData.matches.Length > 0)
                original = CurrentMatchData.matches.Count((x) => {
                    return x.word == word;
                }) == 0;

        if (actualWord && original)
        {
            if (MatchReference != null)
            {
                var json = "";

                if (CurrentMatchData.matches != null)
                {
                    var temp = new List<WordMatch>(CurrentMatchData.matches);
                    temp.Add(new WordMatch
                    {
                        uid = Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser.UserId,
                        word = word
                    });

                    json = Newtonsoft.Json.JsonConvert.SerializeObject(temp.ToArray());
                }
                else
                {
                    var temp = new List<WordMatch>();
                    temp.Add(new WordMatch
                    {
                        uid = Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser.UserId,
                        word = word
                    });

                    json = Newtonsoft.Json.JsonConvert.SerializeObject(temp.ToArray());
                }

                MatchReference.Child("matches").SetRawJsonValueAsync(json);
            }
            else
            {
                Debug.Log("Matched reference not found");
            }
        }
    }
}

[Serializable]
public class MatchRef
{
    public string[] players;
    public MatchContent content;
    public MatchStatus status = null;
    public WordMatch[] matches;
    public Reaction[] reactions;
}

[Serializable]
public class Reaction
{
    public string uid;
    public int index;

    public override bool Equals(object obj)
    {
        return ((Reaction)obj).index == index && ((Reaction)obj).uid == uid;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public override string ToString()
    {
        return base.ToString();
    }
}

[Serializable]
public class MatchStatus
{
    public bool draw = false;
    public string winner = "";
}

[Serializable]
public class MatchContent
{
    public string[] words;
}

[Serializable]
public class ProcessedWordMatch
{
    public Ownership owner;
    public string word;
}

[Serializable]
public class WordMatch
{
    public string uid;
    public string word;

    // override object.Equals
    public override bool Equals(object obj)
    {

        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        } 
        else
        {
            return uid == ((WordMatch)obj).uid && word == ((WordMatch)obj).word;
        } 
    }

    public override int GetHashCode()
    {
        int hashCode = 1766846930;
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(uid);
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(word);
        return hashCode;
    }
}

public enum MatchState
{
    Win, Loss, Draw
}