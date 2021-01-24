using Firebase.Auth;
using Firebase.Database;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;

public class NextRoundUI : MonoBehaviour
{
    public Button ReadyButton;
    public Button ReadiedButton;
    public Button FinishGame;

    [Header("Notifiers / Panels")]
    public GameObject WaitingForOpponent;
    public GameObject WaitingForPlayerInput;

    string enemyId = "";
    bool enemyReady = false;
    bool meReady = false;
    DatabaseReference myReadyReference, enemyReadyReference;

    void Start()
    {
        enemyId = Game.CurrentMatchData.players.First((x) => x != FirebaseAuth.DefaultInstance.CurrentUser.UserId);

        // Set the references
        myReadyReference = Game.MatchReference.Child("ready").Child(FirebaseAuth.DefaultInstance.CurrentUser.UserId);
        enemyReadyReference = Game.MatchReference.Child("ready").Child(enemyId);

        ReadyButton.onClick.AddListener(() =>
        {
            myReadyReference.SetValueAsync(true);
            ReadyButton.gameObject.SetActive(false);
            ReadiedButton.gameObject.SetActive(true);
            meReady = true;
        });

        ReadiedButton.onClick.AddListener(() =>
        {
            myReadyReference.SetValueAsync(false);
            ReadiedButton.gameObject.SetActive(false);
            ReadyButton.gameObject.SetActive(true);
            meReady = false;
        });

        FinishGame.onClick.AddListener(() =>
        {
            myReadyReference.SetValueAsync(false); // Set the player not ready
            Game.MatchReference.Child("disconnect").SetValueAsync(FirebaseAuth.DefaultInstance.CurrentUser.UserId); // Set the player disconnect
            SceneManager.LoadSceneAsync(1);
        });

        // on enemy ready changes
        enemyReadyReference.ValueChanged += EnemyReadyReference_ValueChanged;
    }

    bool loading = false;

    private void Update()
    {
        if (enemyReady && meReady && !loading)
        {
            // if both the enemy and the player are ready for the next round
            SceneManager.LoadSceneAsync(2);
            loading = true;
        }
    }

    /// <summary>
    /// On enemy player ready or not changed
    /// </summary>
    private void EnemyReadyReference_ValueChanged(object sender, ValueChangedEventArgs e)
    {
        if (e.Snapshot.Exists)
        {
            enemyReady = (bool)e.Snapshot.Value;

            WaitingForOpponent.SetActive(!enemyReady);
            WaitingForPlayerInput.SetActive(enemyReady);
        }
    }
}
