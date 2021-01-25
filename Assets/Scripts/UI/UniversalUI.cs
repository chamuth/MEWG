using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UniversalUI : MonoBehaviour
{
    public GameObject HintsUI;
    public CanvasGroup DisconnectMessage;
    public CanvasGroup NetworkMessage;
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
        StartCoroutine(_ShowMessage(DisconnectMessage));
    }

    public void ShowNetworkConnectionWarning()
    {
        StartCoroutine(_ShowMessage(NetworkMessage));
    }

    IEnumerator _ShowMessage(CanvasGroup cg)
    {
        cg.alpha = 0;
        cg.gameObject.SetActive(true);

        while (cg.alpha < 1)
        {
            cg.alpha = Mathf.MoveTowards(cg.alpha, 1, Time.deltaTime * 3f);
            yield return null;
        }

        yield return new WaitForSeconds(4);

        while (cg.alpha > 0)
        {
            cg.alpha = Mathf.MoveTowards(cg.alpha, 0, Time.deltaTime * 4f);
            yield return null;
        }

        cg.gameObject.SetActive(false);
    }
}
