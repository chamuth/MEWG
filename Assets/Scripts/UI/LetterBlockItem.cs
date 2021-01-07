using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class LetterBlockItem : MonoBehaviour
{
    public string Letter = "A";
    public bool Visible = false;

    void Start()
    {
        
    }

    private void OnValidate()
    {
        UpdateValues();
    }

    public void UpdateValues()
    {
        var txt = transform.GetChild(0).gameObject.GetComponent<TMPro.TextMeshProUGUI>();
        txt.text = Letter;
        txt.gameObject.SetActive(Visible);
    }
}
