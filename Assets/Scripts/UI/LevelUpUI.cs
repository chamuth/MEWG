﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelUpUI : MonoBehaviour
{
    public NextLevelXPProgress XPProgress;
    public GameObject LevelUpChild;

    private void Start()
    {
        StartCoroutine(Setup());
    }

    IEnumerator Setup()
    {
        // wait until user details load
        while (User.CurrentUser == null)
            yield return null;

        var currentXP = PlayerPrefs.GetInt("PLAYER_XP", 0);

        if (User.CurrentUser.xp > 0)
        {
            var savedLevel = XP.XPToLevel(currentXP);
            var serverLevel = XP.XPToLevel(User.CurrentUser.xp);

            if (savedLevel < serverLevel)
            {
                // Level Up
                XPProgress.CurrentLevel = serverLevel;
                LevelUpChild.SetActive(true);

                PlayerPrefs.SetInt("PLAYER_XP", User.CurrentUser.xp);
                PlayerPrefs.Save();
            }
        }
    }

    public void Close()
    {
        LevelUpChild.SetActive(false);
    }
}