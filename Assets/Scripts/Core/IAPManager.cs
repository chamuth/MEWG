using Firebase.Auth;
using Firebase.Database;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;

public class IAPManager : MonoBehaviour
{
    public GameObject PurchasePreloader;

    public void RemoveAds()
    {
        print("ADS HAVE BEEN REMOVED FROM YOUR GAME");
        PlayerPrefs.SetInt("REMOVE_ADS", 1);
        PlayerPrefs.Save();
    }

    public void BuyHints(int count)
    {
        print("BOUGHT SOME HINTS : " + count.ToString());

        var hintRef = FirebaseDatabase.DefaultInstance.RootReference.Child("user").Child(FirebaseAuth.DefaultInstance.CurrentUser.UserId).Child("hints").Child("count");

        hintRef.GetValueAsync().ContinueWith((snap) =>
        {
            var currentHintsCount = (int)snap.Result.Value;
            hintRef.SetValueAsync(currentHintsCount + count).ContinueWith((t) =>
            {
                if (t.IsCompleted && !t.IsFaulted)
                {
                    // Updated the database
                    PurchasePreloader.SetActive(false);
                }
            });
        });
    }
}