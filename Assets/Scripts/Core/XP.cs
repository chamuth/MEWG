using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XP
{
    const float FIRST_LEVEL_LIMIT = 100;
    const float XP_LEVEL_MULTIPLIER = 2.5f;

    /// <summary>
    /// Returns Level number for XP value
    /// </summary>
    /// <param name="XP">XP value</param>
    /// <returns>Number of the current level</returns>
    public static int XPToLevel(int XP)
    {
        var limit = (float) FIRST_LEVEL_LIMIT;

        for(var i = 0; i < Mathf.Infinity; i ++)
        {
            if (XP < limit)
            {
                return (i + 1);
            }
            
            limit = limit * XP_LEVEL_MULTIPLIER;
        }

        return -1;
    }

    public static XPReturner RemainingXPToLevelUp(int XP)
    {
        var limit = (float)FIRST_LEVEL_LIMIT;
        var returner = new XPReturner();

        for (var i = 0; i < Mathf.Infinity; i++)
        {
            if (XP < limit)
            {
                if (i == 0)
                {
                    returner.TotalXP = (int)(limit);
                    returner.RemainingXP = XP;
                    break;
                }
                else
                {
                    returner.TotalXP = (int)(limit - (limit / XP_LEVEL_MULTIPLIER));
                    returner.RemainingXP = XP - (int)(limit / XP_LEVEL_MULTIPLIER);
                    break;
                }
            }

            limit = limit * XP_LEVEL_MULTIPLIER;
        }

        return returner;
    }

    public static string AttributeCodeForXP(int level)
    {
        if (level > 50)
        {
            return "LOGOPHILE";
        }
        else if (level > 20)
        {
            return "WORD_MASTER";
        }
        else
        {
            return "";
        }
    }
}

public class XPReturner
{
    public int RemainingXP;
    public int TotalXP;
}
