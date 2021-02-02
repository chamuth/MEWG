using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleButton : MonoBehaviour
{
    public bool Enabled = false;

    public TMPro.TextMeshProUGUI Label;
    public string PreText = "";
    public Action OnToggle;

    Image img;

    private void Start()
    {
        img = GetComponent<Image>();
        UpdateColor();
    }

    public void UpdateColor()
    {
        if (img != null)
        {
            if (Enabled)
            {
                img.color = new Color(1, 58f / 255, 0);
            }
            else
            {
                img.color = new Color(100 / 255f, 100 / 255f, 100 / 255f);
            }
        }
    }

    public void Toggle()
    {
        Enabled = !Enabled;
        UpdateColor();
        OnToggle?.Invoke();
    }

    private void Update()
    {
        Label.text = PreText + " : " + (Enabled ? "ON" : "OFF");
    }
}
