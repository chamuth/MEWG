using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppCopyrightsRenderer : MonoBehaviour
{
    private void Start()
    {
        GetComponent<TMPro.TextMeshProUGUI>().text = string.Format("© {0} Ninponix Games", DateTime.Now.Year);
    }
}
