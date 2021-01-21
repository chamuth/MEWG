using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI.Extensions;

public class ShineAnimator : MonoBehaviour
{
    ShineEffector shiner;

    private void Start()
    {
        shiner = GetComponent<ShineEffector>();
        shiner.YOffset = -1;
    }

    private float tempTimeCounter = 2f;

    private void Update()
    {
        if (shiner.YOffset < 1)
        {
            shiner.YOffset += Time.deltaTime * 3f;
        }
        else
        {
            if (tempTimeCounter > 0)
            {
                tempTimeCounter -= Time.deltaTime;
            }
            else
            {
                tempTimeCounter = 2f;
                shiner.YOffset = -1;
            }
        }
    }
}
