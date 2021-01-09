using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeCounter : MonoBehaviour
{
    public float Timer;

    TMPro.TextMeshProUGUI text;
    Image progressBar = null;

    float StartTime = 5f;

    private void Start()
    {
        text = transform.Find("Time").gameObject.GetComponent<TMPro.TextMeshProUGUI>();
        if (transform.Find("ProgressBar") != null)
            progressBar = transform.Find("ProgressBar").gameObject.GetComponent<Image>();
        StartTime = Timer;
    }

    public void Update()
    {
        if (Timer > 0)
        {
            Timer -= Time.deltaTime;

            int minutes = (int)(Timer / 60);
            int seconds = (int)(Timer - minutes * 60);

            text.text = String.Format("{0:00}:{1:00}", minutes, seconds);

            if (progressBar != null)
                progressBar.fillAmount = Timer / StartTime;
        }
    }    
}
