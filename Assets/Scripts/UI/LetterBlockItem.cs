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

    public bool OnceAnimated = false;

    /// <summary>
    /// Letter index of the word
    /// </summary>
    public int WordOffset = 0;

    private void OnValidate()
    {
        UpdateValues();
    }

    IEnumerator SpawnLetter()
    {
        yield return new WaitForSeconds(0.1f * WordOffset);

        if (!OnceAnimated)
        {
            transform.localScale = Vector3.one * 1.5f;

            while (transform.localScale != Vector3.one)
            {
                transform.localScale = Vector2.MoveTowards(transform.localScale, Vector3.one, Time.deltaTime * 4f);
                yield return null;
            }
        }
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
                StartCoroutine(SpawnLetter());
                break;
            case Ownership.Theirs:
                image.color = ColorManager.Instance.GetColor("ENEMY_COLOR");
                txt.color = Color.white;
                StartCoroutine(SpawnLetter());
                break;
        }
    }
}

public enum Ownership
{
    Neutral, Ours, Theirs
}