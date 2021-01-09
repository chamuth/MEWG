using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeCounter : MonoBehaviour
{
    public float Timer;

    TMPro.TextMeshProUGUI text;

    private void Start()
    {
        text = transform.Find("Time").gameObject.GetComponent<TMPro.TextMeshProUGUI>();
    }

    public void Update()
    {
        if (Timer > 0)
        {
            Timer -= Time.deltaTime;

            int minutes = (int)(Timer / 60);
            int seconds = (int)(Timer - minutes * 60);

            text.text = String.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }    
}
