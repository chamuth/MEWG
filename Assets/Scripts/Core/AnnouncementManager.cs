using Firebase.Database;
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

    string currentURL = "";
    bool updated = false;

    private void Start()
    {
        // Check for announcements
        FirebaseDatabase.DefaultInstance.RootReference.Child("announcement").GetValueAsync().ContinueWith((snapshot) =>
        {
            var url = snapshot.Result.GetValue(false);
            currentURL = url.ToString();
            updated = true;
        });
    }

    private void Update()
    {
        if (updated)
        {
            if (PlayerPrefs.GetString("ANNOUNCEMENT", "") != currentURL.ToString())
            {
                // A new announcement
                UI.SetActive(true);
                StartCoroutine(LoadImage(currentURL.ToString()));

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

            PlayerPrefs.SetString("ANNOUNCEMENT", currentURL);
            PlayerPrefs.Save();
        }
    }
}
