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
    public AudioClip Hint;
    public AudioClip LevelUp;

    AudioSource general;
    AudioSource letterSFX;

    public static SoundManager Instance;

    bool MusicON = true;
    bool SFXON = true;

    private void Start()
    {
        general = GetComponent<AudioSource>();
        letterSFX = transform.GetChild(0).gameObject.GetComponent<AudioSource>();
        Instance = this;
        UpdateSettings();
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
            case "HINT":
                clip = Hint;
                break;
            case "LEVELUP":
                clip = LevelUp;
                break;
        }

        if (SFXON)
            general.PlayOneShot(clip);
    }

    public void UpdateSettings()
    {
        MusicON = PlayerPrefs.GetInt("MUSIC_ON", 1) == 1;
        SFXON = PlayerPrefs.GetInt("SFX_ON", 1) == 1;
    }
    
    public void SelectLetter(int letters)
    {
        if (SFXON)
        {
            var seminotes = (letters - 1) * 2;
            if (letters >= 4) seminotes--;

            letterSFX.pitch = Mathf.Pow(1.05946f, seminotes);
            letterSFX.PlayOneShot(Select);
        }
    }
}