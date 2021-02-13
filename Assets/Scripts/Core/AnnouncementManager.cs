using Firebase.Database;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class AnnouncementManager : MonoBehaviour
{
    [Header("Connections")]
    public GameObject UI;
    public Image AnnouncementSprite;

    Announcement currentData;
    bool updated = false;

    private void Start()
    {
        // Check for announcements
        FirebaseDatabase.DefaultInstance.RootReference.Child("announcement").GetValueAsync().ContinueWith((snapshot) =>
        {
            try
            {
                currentData = JsonConvert.DeserializeObject<Announcement>(snapshot.Result.GetRawJsonValue());
                updated = true;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }
        });
    }

    private void Update()
    {
        if (updated)
        {
            if (PlayerPrefs.GetString("ANNOUNCEMENT", "") != currentData.image.ToString())
            {
                // A new announcement
                UI.SetActive(true);
                StartCoroutine(LoadImage(currentData.image.ToString()));

                updated = false;
            }
        }
    }

    IEnumerator LoadImage(string url)
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
            Texture2D myTexture = ((DownloadHandlerTexture)www.downloadHandler).texture;
            var sprite = Sprite.Create(myTexture, new Rect(0, 0, myTexture.width, myTexture.height), Vector2.one * 0.5f);
            AnnouncementSprite.sprite = sprite;

            PlayerPrefs.SetString("ANNOUNCEMENT", currentData.image);
            PlayerPrefs.Save();
        }
    }

    public void ImageClicked()
    {
        if (currentData != null)
        {
            if (currentData.click != "")
            {
                Application.OpenURL(currentData.click);
            }
        }
    }
}

[Serializable]
public class Announcement
{
    public string image;
    public string click;
}