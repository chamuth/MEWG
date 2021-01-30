using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleButton : MonoBehaviour
{
    public bool Enabled = false;

    public TMPro.TextMeshProUGUI Label;
    public string PreText = "";

    public void Toggle()
    {
        Enabled = !Enabled;
    }

    private void Update()
    {
        Label.text = PreText + " : " + (Enabled ? "ON" : "OFF");
    }
}
