using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReferralGifts : MonoBehaviour
{
    public Button FacebookShareButton, WhatsappShareButton, TwitterShareButton;
    public GameObject FacebookSharedTick, WhatsappSharedTick, TwitterSharedTick;

    public GameObject HintsRewardedUI;

    private void Start()
    {
        UpdateButtons();
    }

    void UpdateButtons()
    {
        if (PlayerPrefs.GetInt("FACEBOOK_SHARED", 0) == 1)
        {
            FacebookShareButton.gameObject.SetActive(false);
            FacebookSharedTick.SetActive(true);
        }

        if (PlayerPrefs.GetInt("WHATSAPP_SHARED", 0) == 1)
        {
            WhatsappShareButton.gameObject.SetActive(false);
            WhatsappSharedTick.SetActive(true);
        }

        if (PlayerPrefs.GetInt("TWITTER_SHARED", 0) == 1)
        {
            TwitterShareButton.gameObject.SetActive(false);
            TwitterSharedTick.SetActive(true);
        }
    }

    public void ShareFacebook()
    {

    }

    public void ShareWhatsapp()
    {

    }

    public void ShareTwitter()
    {

    }

    public void Reward()
    {
        HintsRewardedUI.SetActive(true);
    }
}
