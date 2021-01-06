using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinningProgressBar : MonoBehaviour
{
    public int Score1;
    public int Score2;

    RectTransform side1;
    RectTransform side2;
    TMPro.TextMeshProUGUI side1Text;
    TMPro.TextMeshProUGUI side2Text;
    float myWidth;

    void Start()
    {
        side1 = transform.GetChild(0).gameObject.GetComponent<RectTransform>();
        side2 = transform.GetChild(1).gameObject.GetComponent<RectTransform>();
        myWidth = GetComponent<RectTransform>().rect.width;
        side1Text = transform.GetChild(0).GetChild(0).gameObject.GetComponent<TMPro.TextMeshProUGUI>();
        side2Text = transform.GetChild(1).GetChild(0).gameObject.GetComponent<TMPro.TextMeshProUGUI>();
    }

    Vector2 wVector = new Vector2(-1, 0);

    void Update()
    {
        side1Text.text = Score1.ToString();
        side2Text.text = Score2.ToString();

        // s1, s2 represents rendering scales
        var s1 = Score1;
        var s2 = Score2;

        if (Score1 == 0 && Score2 == 0)
        {
            s1 = 1;
            s2 = 1;
        }

        var w1 = myWidth / (s1 + s2) * (s2);
        var w2 = myWidth / (s1 + s2) * (s1);

        side1.sizeDelta = Vector2.MoveTowards(side1.sizeDelta, wVector * w1, Time.deltaTime * 500f);
        side2.sizeDelta = Vector2.MoveTowards(side2.sizeDelta, wVector * w2, Time.deltaTime * 500f);

    }
}
