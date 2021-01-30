using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppVersionRenderer : MonoBehaviour
{
    void Start()
    {
        GetComponent<TMPro.TextMeshProUGUI>().text = "v" + Application.version;
    }
}
