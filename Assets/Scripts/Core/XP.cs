using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XP
{
    const float FIRST_LEVEL_LIMIT = 500;

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
            
            limit = limit * 1.25f;
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
                returner.TotalXP = (int)(limit - (limit / 1.25));
                returner.RemainingXP = XP - (int)(limit / 1.25);
                break;
            }

            limit = limit * 1.25f;
        }

        return returner;
    }
}

public class XPReturner
{
    public int RemainingXP;
    public int TotalXP;
}
