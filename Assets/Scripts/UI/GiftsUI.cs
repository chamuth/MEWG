using Firebase.Auth;
using Firebase.Database;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GiftsUI : MonoBehaviour
{
    public TMPro.TextMeshProUGUI GiftTitle;
    public TMPro.TextMeshProUGUI GiftAmount;
    public GameObject Preloader;

    int currentGiftAmount = 0;
    bool closeMyself = false;

    private void Start()
    {
        Preloader.SetActive(false);
    }

    private void Update()
    {
        if (closeMyself)
        {
            closeMyself = false;
            gameObject.SetActive(false);
        }
    }

    public void SetGiftAmount(int amount)
    {
        GiftTitle.text = (amount == 1) ? "Hint" : "Hints";
        GiftAmount.text = amount.ToString();
        currentGiftAmount = amount;
    }

    public void ClaimGift()
    {
        Preloader.SetActive(true);

        var hintReference = FirebaseDatabase.DefaultInstance.RootReference
            .Child("user")
            .Child(FirebaseAuth.DefaultInstance.CurrentUser.UserId)
            .Child("hints")
            .Child("count");

        hintReference.SetValueAsync(User.CurrentUser.hints.count + currentGiftAmount).ContinueWith((s) =>
        {
            // Hide the preloader and the gifts menu
            closeMyself = true;
            Preloader.SetActive(false);
        });

        currentGiftAmount = 0;
    }
}
