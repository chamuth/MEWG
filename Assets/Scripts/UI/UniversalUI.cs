using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UniversalUI : MonoBehaviour
{
    public GameObject HintsUI;
    public CanvasGroup DisconnectMessage;
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

    public void ShowPlayerDisconnectMessage()
    {
        StartCoroutine(_ShowPlayerDisconnectMessage());
    }

    IEnumerator _ShowPlayerDisconnectMessage()
    {
        DisconnectMessage.alpha = 0;
        DisconnectMessage.gameObject.SetActive(true);

        while (DisconnectMessage.alpha < 1)
        {
            DisconnectMessage.alpha = Mathf.MoveTowards(DisconnectMessage.alpha, 1, Time.deltaTime * 3f);
            yield return null;
        }

        yield return new WaitForSeconds(4);

        while (DisconnectMessage.alpha > 0)
        {
            DisconnectMessage.alpha = Mathf.MoveTowards(DisconnectMessage.alpha, 0, Time.deltaTime * 4f);
            yield return null;
        }

        DisconnectMessage.gameObject.SetActive(false);
    }
}
