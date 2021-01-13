using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProfileItem : MonoBehaviour
{
    public Sprite Picture;
    [Range(1,99)]
    public int Level;

    private void Start()
    {
        UpdateDetails();
    }

    public void UpdateDetails()
    {
        var levelText = transform.GetChild(1).GetChild(0).GetChild(0).gameObject.GetComponent<TMPro.TextMeshProUGUI>();
        levelText.text = Level.ToString();

        var profileImage = transform.GetChild(0).GetChild(0).gameObject.GetComponent<Image>();
        profileImage.sprite = Picture;
    }
}
