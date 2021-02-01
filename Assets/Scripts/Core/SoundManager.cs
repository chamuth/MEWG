using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public AudioClip Woosh;
    public AudioClip Win;
    public AudioClip Select;
    public AudioClip CancelWrong;
    public AudioClip Correct;
    public AudioClip Pop;

    AudioSource general;
    AudioSource letterSFX;

    public static SoundManager Instance;

    private void Start()
    {
        general = GetComponent<AudioSource>();
        letterSFX = transform.GetChild(0).gameObject.GetComponent<AudioSource>();
        Instance = this;
    }

    public void PlayClip(string name)
    {
        AudioClip clip = Woosh;

        switch (name)
        {
            case "WOOSH":
                clip = Woosh;
                break;
            case "CANCELWRONG":
                clip = CancelWrong;
                break;
            case "CORRECT":
                clip = Correct;
                break;
            case "WIN":
                clip = Win;
                break;
            case "POP":
                clip = Pop;
                break;
        }

        general.PlayOneShot(clip);
    }
    
    public void SelectLetter(int letters)
    {
        var seminotes = (letters - 1) * 2;
        if (letters >= 4) seminotes--;

        letterSFX.pitch = Mathf.Pow(1.05946f, seminotes);
        letterSFX.PlayOneShot(Select);
    }
}