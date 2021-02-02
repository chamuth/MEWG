﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySFXonStart : MonoBehaviour
{
    public string ClipId = "";

    private void Start()
    {
        SoundManager.Instance.PlayClip(ClipId);
    }
}