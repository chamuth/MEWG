using GooglePlayGames;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public static class AchievementsManager
{
    static string[] WinMatchesIncrements = { "CgkIwfymq44BEAIQAw", "CgkIwfymq44BEAIQBA", "CgkIwfymq44BEAIQBQ", "CgkIwfymq44BEAIQBg" };
    static string[] FindWordsIncrements = { "CgkIwfymq44BEAIQCg", "CgkIwfymq44BEAIQCw", "CgkIwfymq44BEAIQDA" };
    static string[] WinMatchesWithoutHintsIncrments = { "CgkIwfymq44BEAIQDw", "CgkIwfymq44BEAIQEA" };

    static string Addicted1_ID = "CgkIwfymq44BEAIQDQ";
    static string Addicted2_ID = "CgkIwfymq44BEAIQDg";

    static string Level10_ID = "CgkIwfymq44BEAIQBw";
    static string Level15_ID = "CgkIwfymq44BEAIQCA";
    static string Level20_ID = "CgkIwfymq44BEAIQCQ";

    static void IncrementSeries(string[] ids)
    {
        foreach (var id in ids)
        {
            GooglePlayGames.PlayGamesPlatform.Instance.IncrementAchievement(id, 1, (b) =>
            {
                if (b) Debug.Log(string.Format("Achivement {0}, Incremented by {1}", id, 1));
            });
        }
    }

    public static void MatchWon()
    {
        IncrementSeries(WinMatchesIncrements);
    }
    
    public static void MatchWonWithoutHints()
    {
        IncrementSeries(WinMatchesWithoutHintsIncrments);
    }

    public static void FoundWord()
    {
        IncrementSeries(FindWordsIncrements);
    }

    public static void LevelUp(int level)
    {
        var id = "";

        if (level == 10)
            id = Level10_ID;
        else if (level == 15)
            id = Level15_ID;
        else if (level == 20)
            id = Level20_ID;

        PlayGamesPlatform.Instance.UnlockAchievement(Level10_ID);
    }

    public static void ConsecutiveDays(int days)
    {
        if (days == 5)
            PlayGamesPlatform.Instance.UnlockAchievement(Addicted1_ID);
        else if (days == 10)
            PlayGamesPlatform.Instance.UnlockAchievement(Addicted2_ID);
    }
}
