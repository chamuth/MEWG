using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public static class AchievementsManager
{
    public static string[] WinMatchesIncrements = { };

    public static void IncrementAchivement(string id, int steps  = 1)
    {
         GooglePlayGames.PlayGamesPlatform.Instance.IncrementAchievement(id, steps, (b) =>
         {
             Debug.Log(string.Format("Achivement {0}, Incremented by {1}", id, steps));
         });
    }
}
