using Firebase.Auth;
using Firebase.Database;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HintCountRenderer : MonoBehaviour
{
    TMPro.TextMeshProUGUI text;
    GameObject preloader;
    DatabaseReference hintNodeReference;

    void Start()
    {
        text = transform.Find("Hints Count").gameObject.GetComponent<TMPro.TextMeshProUGUI>();
        preloader = transform.Find("CircularPreloader").gameObject;

        hintNodeReference = FirebaseDatabase.DefaultInstance.RootReference.Child("user").Child(FirebaseAuth.DefaultInstance.CurrentUser.UserId).Child("hints");

        hintNodeReference.ValueChanged += HintNodeReference_ValueChanged; 
    }

    private void HintNodeReference_ValueChanged(object sender, ValueChangedEventArgs e)
    {
        // hide the preloader
        preloader.SetActive(false);

        var count = JsonConvert.DeserializeObject<HintNode>(e.Snapshot.GetRawJsonValue()).count;
        text.text = count.ToString();

        if (gameObject.activeInHierarchy)
            StartCoroutine(AnimateChange(text.gameObject)); 
    }

    private void OnDestroy()
    {
        hintNodeReference.ValueChanged -= HintNodeReference_ValueChanged;
    }

    IEnumerator AnimateChange(GameObject animatable)
    {
        while(Vector3.Magnitude(animatable.transform.localScale) < Vector3.Magnitude(Vector3.one * 1.25f))
        {
            animatable.transform.localScale = Vector3.MoveTowards(animatable.transform.localScale, Vector3.one * 1.25f, Time.deltaTime * 10f);
            yield return null;
        }

        while (Vector3.Magnitude(animatable.transform.localScale) > Vector3.Magnitude(Vector3.one))
        {
            animatable.transform.localScale = Vector3.MoveTowards(animatable.transform.localScale, Vector3.one, Time.deltaTime * 10f);
            yield return null;
        }
    }
}

[Serializable]
public class HintNode
{
    public int count;
}

