using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class LetterBlockItem : MonoBehaviour
{
    public string Letter = "A";
    public Ownership _Ownership = Ownership.Neutral;
    public bool Visible = false;

    private void OnValidate()
    {
        UpdateValues();
    }

    public void UpdateValues()
    {
        var image = GetComponent<Image>();
        var txt = transform.GetChild(0).gameObject.GetComponent<TMPro.TextMeshProUGUI>();
        txt.text = Letter;
        txt.gameObject.SetActive(Visible);

        switch (_Ownership)
        {
            case Ownership.Neutral:
                txt.color = new Color(64 / 255f, 71 / 255f, 91 / 255f, 1);
                image.color = Color.white;
                break;
            case Ownership.Ours:
                image.color = ColorManager.Instance.GetColor("FRIENDLY_COLOR");
                txt.color = Color.white;
                break;
            case Ownership.Theirs:
                image.color = ColorManager.Instance.GetColor("ENEMY_COLOR");
                txt.color = Color.white;
                break;
        }
    }
}

public enum Ownership
{
    Neutral, Ours, Theirs
}