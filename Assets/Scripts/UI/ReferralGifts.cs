using Facebook.Unity;
using Firebase.Auth;
using Firebase.Database;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ReferralGifts : MonoBehaviour
{
	public Button FacebookShareButton, WhatsappShareButton, TwitterShareButton;
	public GameObject FacebookSharedTick, WhatsappSharedTick, TwitterSharedTick;
	public GameObject FacebookGift, WhatsappGift, TwitterGift;
	public GameObject FacebookAlreadyRewarded, WhatsappAlreadyRewarded, TwitterAlreadyRewarded;

	public GameObject HintsRewardedUI;

	private void Start()
	{
		if(!FB.IsInitialized)
			FB.Init();

		UpdateButtons();
	}

	void UpdateButtons()
	{
		if (PlayerPrefs.GetInt("FACEBOOK_SHARED", 0) == 1)
		{
			FacebookShareButton.gameObject.SetActive(false);
			FacebookGift.SetActive(false);
			FacebookAlreadyRewarded.SetActive(true);
			FacebookSharedTick.SetActive(true);
		}

		if (PlayerPrefs.GetInt("WHATSAPP_SHARED", 0) == 1)
		{
			WhatsappShareButton.gameObject.SetActive(false);
			WhatsappGift.SetActive(false);
			WhatsappAlreadyRewarded.SetActive(true);
			WhatsappSharedTick.SetActive(true);
		}

		if (PlayerPrefs.GetInt("TWITTER_SHARED", 0) == 1)
		{
			TwitterShareButton.gameObject.SetActive(false);
			TwitterGift.SetActive(false);
			TwitterAlreadyRewarded.SetActive(true);
			TwitterSharedTick.SetActive(true);
		}
	}

	public void ShareFacebook()
	{
		if (PlayerPrefs.GetInt("FACEBOOK_SHARED", 0) == 0)
		{
			PlayerPrefs.SetInt("FACEBOOK_SHARED", 1);
			PlayerPrefs.Save();

			FB.ShareLink(
				contentURL: new System.Uri("http://play.google.com/store/apps/details?id=com.ninponix.mewg"),
				contentTitle: "Word Binders - A Multiplayer Word Matching Game",
				contentDescription: "Checkout this cool game I've found",
				callback: (IShareResult result) =>
			   {
					if (!result.Cancelled)
					{
						// Reward the player
						Reward();
						UpdateButtons();
					}
				}
			);
		}
	}

	public void ShareWhatsapp()
	{

	}

	public void ShareTwitter()
	{
		if (PlayerPrefs.GetInt("TWITTER_SHARED", 0) == 0)
		{
			PlayerPrefs.SetInt("TWITTER_SHARED", 1);
			PlayerPrefs.Save();

			Application.OpenURL("http://twitter.com/intent/tweet" +
				"?text=" + UnityWebRequest.EscapeURL("Check out this cool game I've found, you gotta match and find the words before your opponent http://play.google.com/store/apps/details?id=com.ninponix.mewg")
			);

			StartCoroutine(RewardLate());
		}
	}

	IEnumerator RewardLate()
	{
		yield return new WaitForSeconds(2);
		Reward();
		UpdateButtons();
	}

	public void Reward()
	{
		HintsRewardedUI.SetActive(true);

		var hintReference = FirebaseDatabase.DefaultInstance.RootReference
			.Child("user")
			.Child(FirebaseAuth.DefaultInstance.CurrentUser.UserId)
			.Child("hints")
			.Child("count");

		hintReference.SetValueAsync(User.CurrentUser.hints.count + 1).ContinueWith((s) =>
		{
		});
	}
}
