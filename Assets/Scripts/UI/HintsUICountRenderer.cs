using Firebase.Auth;
using Firebase.Database;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HintsUICountRenderer : MonoBehaviour
{
    TMPro.TextMeshProUGUI hintsText;
    DatabaseReference hintsReference;

    private void Start()
    {
        hintsText = GetComponent<TMPro.TextMeshProUGUI>();
        hintsReference = FirebaseDatabase.DefaultInstance.RootReference.Child("user").Child(FirebaseAuth.DefaultInstance.CurrentUser.UserId).Child("hints").Child("count");
        hintsReference.ValueChanged += HintsUICountRenderer_ValueChanged;
    }

    private void OnDestroy()
    {
        hintsReference.ValueChanged -= HintsUICountRenderer_ValueChanged;
    }

    private void HintsUICountRenderer_ValueChanged(object sender, ValueChangedEventArgs e)
    {
        int val = (int) e.Snapshot.Value;
        hintsText.text = string.Format("You currently have {0} hints", val);
    }
}
