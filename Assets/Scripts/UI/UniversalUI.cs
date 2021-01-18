using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UniversalUI : MonoBehaviour
{
    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }
}
