using Firebase.Database;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchEnder : MonoBehaviour
{
    TimeCounter counter;
    bool matchEnded = false;

    void Start()
    {
        counter = GetComponent<TimeCounter>();
    }

    private void Update()
    {
        if (counter.Timer <= 0)
        {
            // timer has passed
            if (!matchEnded)
            {
                FirebaseDatabase.DefaultInstance.RootReference.Child("match").Child(Game.CurrentMatchID).Child("time").SetValueAsync("up");
                matchEnded = true;
            }
        }
    }
}
