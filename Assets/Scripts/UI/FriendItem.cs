using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FriendItem : MonoBehaviour
{
    [HideInInspector]
    public string Name;
    [HideInInspector]
    public PlayerStatus Status;
    [HideInInspector]
    public Sprite Picture;

    public TMPro.TextMeshProUGUI PlayerName;
    public TMPro.TextMeshProUGUI _PlayerStatus;
    public Image ProfilePicture;

    public void UpdateStats()
    {
        PlayerName.text = Name;

        switch (Status)
        {
            case PlayerStatus.Online:
                _PlayerStatus.text = "Online";
                break;
            case PlayerStatus.Offline:
                _PlayerStatus.text = "Offline";
                break;
            case PlayerStatus.Ingame:
                _PlayerStatus.text = "Ingame";
                break;
        }
    }
}

public enum PlayerStatus
{
    Offline, Online, Ingame
}