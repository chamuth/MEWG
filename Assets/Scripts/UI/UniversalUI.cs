using Firebase.Auth;
using Firebase.Database;
using GoogleMobileAds.Api;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UniversalUI : MonoBehaviour
{
    public GameObject HintsUI;
    public CanvasGroup DisconnectMessage;
    public CanvasGroup NetworkMessage;
    public static UniversalUI Instance;
    public Button ShowRewardingAdButton;
    public GameObject HintsRewardedUI;
    public ParticleSystem HintRewardedParticleSystem;

    private const string RewardedAdUnitID = "ca-app-pub-5103739755612302/8666927502";
    RewardedAd rewardedAd;
    bool videoAdLoaded = false;
    bool hideVideoAdButton = false;
    bool hintsRewarded = false;

    private void Start()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Load a video rewarding ad
        rewardedAd = new RewardedAd(RewardedAdUnitID);

        rewardedAd.OnAdLoaded += RewardedAd_OnAdLoaded;
        rewardedAd.OnUserEarnedReward += RewardedAd_OnUserEarnedReward;

        AdRequest request = new AdRequest.Builder().Build();
        rewardedAd.LoadAd(request);

        // Initially the show ad button is not enabled
        ShowRewardingAdButton.interactable = false;

        ShowRewardingAdButton.onClick.AddListener(() =>
        {
            ShowVideoAd();
        });
    }

    private void Update()
    {
        if (videoAdLoaded)
        {
            ShowRewardingAdButton.interactable = true;
            videoAdLoaded = false;
        }

        if (hideVideoAdButton)
        {
            ShowRewardingAdButton.interactable = false;
            hideVideoAdButton = false;
        }

        if (hintsRewarded)
        {
            HintsRewardedUI.SetActive(true);
            HintRewardedParticleSystem.Play();

            PlayerPrefs.SetString("LAST_VIDEO_AD_DATE", DateTime.Now.DayOfYear.ToString() + DateTime.Now.Hour.ToString());
            PlayerPrefs.Save();

            hintsRewarded = false;
        }
    }

    private void RewardedAd_OnUserEarnedReward(object sender, Reward e)
    {
        HintsUI.SetActive(false);

        var hintReference = FirebaseDatabase.DefaultInstance.RootReference
            .Child("user")
            .Child(FirebaseAuth.DefaultInstance.CurrentUser.UserId)
            .Child("hints")
            .Child("count");

        hintReference.SetValueAsync(User.CurrentUser.hints.count + 2).ContinueWith((s) =>
        {
            hintsRewarded = true;
        });

        // Hide the video ad button
        hideVideoAdButton = true;
    }

    private void RewardedAd_OnAdLoaded(object sender, System.EventArgs e)
    {
        // Load a video ad if it's not loaded for the last time
        if (PlayerPrefs.GetString("LAST_VIDEO_AD_DATE", "") != DateTime.Now.DayOfYear.ToString() + DateTime.Now.Hour.ToString())
            videoAdLoaded = true;
        else
            hideVideoAdButton = true;
    }

    public void ShowHintsUI()
    {
        HintsUI.SetActive(true);
    }

    public void ShowVideoAd()
    {
        if (rewardedAd.IsLoaded())
        {
            HintsUI.SetActive(false);
            rewardedAd.Show();
        }
    }

    public void ShowPlayerDisconnectMessage()
    {
        StartCoroutine(_ShowMessage(DisconnectMessage));
    }

    public void ShowNetworkConnectionWarning()
    {
        StartCoroutine(_ShowMessage(NetworkMessage));
    }

    IEnumerator _ShowMessage(CanvasGroup cg)
    {
        cg.alpha = 0;
        cg.gameObject.SetActive(true);

        while (cg.alpha < 1)
        {
            cg.alpha = Mathf.MoveTowards(cg.alpha, 1, Time.deltaTime * 3f);
            yield return null;
        }

        yield return new WaitForSeconds(4);

        while (cg.alpha > 0)
        {
            cg.alpha = Mathf.MoveTowards(cg.alpha, 0, Time.deltaTime * 4f);
            yield return null;
        }

        cg.gameObject.SetActive(false);
    }
}
