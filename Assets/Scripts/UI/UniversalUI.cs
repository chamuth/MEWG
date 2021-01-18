using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UniversalUI : MonoBehaviour
{
    public GameObject HintsUI;
    public static UniversalUI Instance;

    private void Start()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void ShowHintsUI()
    {
        HintsUI.SetActive(true);
    }
}
