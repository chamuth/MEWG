using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsUI : MonoBehaviour
{
    public ToggleButton MusicToggle;
    public ToggleButton SFXToggle;
    
    private void Awake()
    {
        MusicToggle.Enabled = PlayerPrefs.GetInt("MUSIC_ON", 1) == 1;
        MusicToggle.UpdateColor();
        MusicToggle.OnToggle += () =>
        {
            PlayerPrefs.SetInt("MUSIC_ON", MusicToggle.Enabled ? 1 : 0);
            SoundManager.Instance.UpdateSettings();
        };

        SFXToggle.Enabled = PlayerPrefs.GetInt("SFX_ON", 1) == 1;
        SFXToggle.UpdateColor();
        SFXToggle.OnToggle += () =>
        {
            PlayerPrefs.SetInt("SFX_ON", SFXToggle.Enabled ? 1 : 0);
            SoundManager.Instance.UpdateSettings();
        };
    }
}
