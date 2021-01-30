using GooglePlayGames;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FriendsUI : MonoBehaviour
{
    public GameObject NoFriendsList;
    public Transform FriendList;
    public GameObject FriendItemGO;

    private void Start()
    {
        // Delete all pre-existing items
        foreach(Transform item in FriendList)
        {
            Destroy(item.gameObject);
        }

        if (PlayGamesPlatform.Instance.IsAuthenticated())
        {
            // If authenticated via google play games
            var friends = PlayGamesPlatform.Instance.localUser.friends;

            if (friends.Length > 0)
            {
                NoFriendsList.SetActive(false);

                foreach (var friend in friends)
                {
                    var fitem = (Instantiate(FriendItemGO, FriendList) as GameObject).GetComponent<FriendItem>();
                    fitem.Name = friend.userName;
                    fitem.Status = (friend.state == UnityEngine.SocialPlatforms.UserState.Online) ? PlayerStatus.Online : PlayerStatus.Offline;
                    fitem.Picture = Sprite.Create(friend.image, new Rect(0, 0, 75, 75), Vector3.one * 0.5f);
                    fitem.UpdateStats();
                }
            }
            else
            {
                NoFriendsList.SetActive(true);
            }
        }
    }
}
