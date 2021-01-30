using Coffee.UIEffects;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchStartVerticalFader : MonoBehaviour
{
    UITransitionEffect effector;

    private void Start()
    {
        effector = GetComponent<UITransitionEffect>();
        effector.effectFactor = 1;
    }

    private void Update()
    {
        if (effector.effectFactor > 0)
        {
            effector.effectFactor -= Time.deltaTime * 3f;
        }
        else
        {
            if (gameObject.activeInHierarchy)
                gameObject.SetActive(false);
        }
    }
}
