using Firebase.Auth;
using Firebase.Database;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Networking;

public class PlayerDetailsScreen : MonoBehaviour
{
    [Header("Settings")]
    public bool CurrentUser = false;
    
    [Header("GO Connections")]
    public ProfileItem[] _ProfileItem;
    public TMPro.TextMeshProUGUI[] PlayerName;
    public TMPro.TextMeshProUGUI WLRatio;
    public PlayerAttribute[] Attributes;
    
    string UID = "";

    public void Start()
    {
        if (CurrentUser)
        {
            User.UpdateCurrentUser();
            UID = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
            StartCoroutine(RenderCurrentUser());
        }
        else
        {
            StartCoroutine(RenderEnemyUser());
        }
    }

    IEnumerator RenderEnemyUser()
    {
        // wait until the current enemy details are loaded
        while (Game.CurrentEnemy == null)
            yield return null;

        StartCoroutine(RenderUser(Game.CurrentEnemy));
    }

    IEnumerator RenderCurrentUser()
    {
        // wait until the current user object is loaded
        while (User.CurrentUser == null)
            yield return null;

        StartCoroutine(RenderUser(User.CurrentUser));
    }

    IEnumerator RenderUser(User user)
    {
        yield return new WaitForSeconds(0);

        // Set player name
        foreach (var text in PlayerName)
        {
            text.text = user.name;
        }

        // Load profile picture from the internet
        StartCoroutine(GetTexture(user.profile, _ProfileItem));

        // Set player level from xp
        var playerLevel = XP.XPToLevel(user.xp);

        foreach (var pi in _ProfileItem)
        {
            pi.Level = playerLevel;
            pi.UpdateDetails();
        }

        WLRatio.text = string.Format("{0:0.##} W/L", user.statistics.wins / (float) ((user.statistics.losses == 0) ? 1 : user.statistics.losses));

        var attrId = XP.AttributeCodeForXP(playerLevel);

        if (attrId != "")
        {
            Attributes.Where(x => x.Id == attrId).First().Target.gameObject.SetActive(true);
        }
    }

    public static IEnumerator GetTexture(string url, ProfileItem[] profile)
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);

        yield return www.SendWebRequest();

        if (www.isNetworkError)
        {
            Debug.Log(www.error);
        }
        else
        {
            Texture2D webTexture = ((DownloadHandlerTexture)www.downloadHandler).texture as Texture2D;
            Sprite webSprite = SpriteFromTexture2D(webTexture);

            foreach (var pp in profile)
            {
                pp.Picture = webSprite;
                pp.UpdateDetails();
            }
        }
    }

    public static Sprite SpriteFromTexture2D(Texture2D texture)
    {
        return Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f);
    }
}

[Serializable]
public class PlayerAttribute
{
    public string Id;
    public GameObject Target;
}
