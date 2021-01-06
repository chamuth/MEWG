using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[ExecuteInEditMode]
public class ColorSetter : MonoBehaviour
{
    public enum SetType { ImageSpriteColor, TextColor, TMPROColor }

    public SetType Type;
    public string ID;

    private void Start()
    {
        SetColors();
    }

    void SetColors()
    {
        // Apply colors only in the Edit Mode
        if (!Application.isPlaying)
        {
            try
            {
                var color = ColorManager.Instance.GetColor(ID);

                if (color != null)
                {
                    switch (Type)
                    {
                        case SetType.ImageSpriteColor:
                            GetComponent<Image>().color = color;
                            break;
                        case SetType.TextColor:
                            GetComponent<Text>().color = color;
                            break;
                        case SetType.TMPROColor:
                            GetComponent<TMPro.TextMeshProUGUI>().color = color;
                            break;
                    }
                }
            }
            catch (Exception)
            {
            }
        }
    }

    private void OnValidate()
    {
        SetColors();
    }
}

