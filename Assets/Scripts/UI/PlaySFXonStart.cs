using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySFXonStart : MonoBehaviour
{
    public string ClipId = "";

    private void OnEnable()
    {
        SoundManager.Instance.PlayClip(ClipId);
    }
}
