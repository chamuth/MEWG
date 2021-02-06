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

        var hintReference = FirebaseDatabase.DefaultInstance.RootReference
            .Child("user")
            .Child(FirebaseAuth.DefaultInstance.CurrentUser.UserId)
            .Child("hints")
            .Child("count");

        hintReference.SetValueAsync(User.CurrentUser.hints.count + count).ContinueWith((s) =>
        {

        });
    }
}