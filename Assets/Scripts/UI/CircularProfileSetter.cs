using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class CircularProfileSetter : MonoBehaviour
{
    ProfileItem profile;
    public TMPro.TextMeshProUGUI PlayerName;
    public TMPro.TextMeshProUGUI XPLevel;
    public Image LevelProgressBar;

    private void Start()
    {
        profile = GetComponent<ProfileItem>();

        // Set user setters
        User.OnUserDataUpdated += () =>
        {
            StartCoroutine(ReadLocalUser());
        };
    }

    IEnumerator ReadLocalUser()
    {
        while (User.CurrentUser == null)
            yield return null;

        UnityWebRequest www = UnityWebRequestTexture.GetTexture(User.CurrentUser.profile);

        yield return www.SendWebRequest();

        if (www.isNetworkError)
        {
            Debug.Log(www.error);
        }
        else
        {
            Texture2D webTexture = ((DownloadHandlerTexture)www.downloadHandler).texture as Texture2D;
            Sprite webSprite = SpriteFromTexture2D(webTexture);

            profile.Picture = webSprite;
        }

        profile.Level = XP.XPToLevel(User.CurrentUser.xp);

        profile.UpdateDetails();
    }

    public static Sprite SpriteFromTexture2D(Texture2D texture)
    {
        return Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f);
    }
}
