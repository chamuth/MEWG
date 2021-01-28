using Firebase.Auth;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MiddleReactionButton : MonoBehaviour
{
    public CanvasGroup ReactionCircle;
    public CanvasGroup LetterCircle;

    bool reactionCircleShown = false;
    bool down = false;
    float timer = 0f;
    float reactionTimer = 0;

    [Header("Our")]
    public Animator OurReactionAnimator;
    public TMPro.TextMeshProUGUI OurReactionText;

    public TMPro.TextMeshProUGUI[] Reactions;

    [Header("Theirs")]
    public Animator TheirReactionAnimator;
    public TMPro.TextMeshProUGUI TheirReactionText;

    TMPro.TextMeshProUGUI myReactionText;

    [HideInInspector]
    public int LastUsedReactionIndex = 0;

    private Reaction[] previousReactions = new Reaction[] { };

    private void Start()
    {
        Game.OnMatchDataChanged += MatchDataChanged;
        myReactionText = GetComponent<TMPro.TextMeshProUGUI>();
    }

    void MatchDataChanged()
    {
        if (Game.CurrentMatchData != null)
        {
            if (Game.CurrentMatchData.reactions != null)
            {
                if (!Enumerable.SequenceEqual(Game.CurrentMatchData.reactions, previousReactions))
                {
                    // Reactions list updated
                    var latestReaction = Game.CurrentMatchData.reactions.Last();

                    if (latestReaction.uid == FirebaseAuth.DefaultInstance.CurrentUser.UserId)
                    {
                        OurReactionText.text = Reactions[latestReaction.index].text;
                        OurReactionAnimator.Play("Show");
                    }
                    else
                    {
                        TheirReactionText.text = Reactions[latestReaction.index].text;
                        TheirReactionAnimator.Play("Show");
                    }

                    previousReactions = Game.CurrentMatchData.reactions;
                }
            }
        }
    }

    private void OnDestroy()
    {
        try
        {
            Game.OnMatchDataChanged -= MatchDataChanged;
        }
        catch (Exception) { }
    }

    public void SendEmoji(int reactionIndex)
    {
        print("Sending reaction " + reactionIndex.ToString());

        if (previousReactions != null)
        {
            var t = new List<Reaction>(previousReactions);
            t.Add(new Reaction { index = reactionIndex, uid = FirebaseAuth.DefaultInstance.CurrentUser.UserId });
            Game.MatchReference.Child("reactions").SetRawJsonValueAsync(JsonConvert.SerializeObject(t.ToArray()));
        }
        else
        {
            var t = new Reaction[] {
                new Reaction { index = reactionIndex, uid = FirebaseAuth.DefaultInstance.CurrentUser.UserId }
            };
            
            Game.MatchReference.Child("reactions").SetRawJsonValueAsync(JsonConvert.SerializeObject(t));
        }

        LastUsedReactionIndex = reactionIndex;

        reactionTimer = 0;
    }

    public void MiddleReactionButtonDown()
    {
        down = true;
    }

    public void MiddleReactionButtonUp()
    {
        down = false;

        if (timer < 0.5f)
        {
            // Send the last used emoji
            SendEmoji(LastUsedReactionIndex);
        }
    }

    private void Update()
    {
        myReactionText.text = Reactions[LastUsedReactionIndex].text;

        // Hold down to import 
        if (down)
        {
            if (timer < 0.5f)
            {
                timer += Time.deltaTime;
            }
            else
            {
                if (!reactionCircleShown)
                {
                    // long tap, show the reaction circle 
                    reactionCircleShown = true;
                    StartCoroutine(FadeInReactionCircle());
                }
            }
        }
        else
        {
            timer = 0;
        }

        if (!down)
        {
            // Hide the reaction circle automatically
            if (reactionCircleShown)
            {
                if (reactionTimer < 3f)
                {
                    reactionTimer += Time.deltaTime;
                }
                else
                {
                    StartCoroutine(FadeOutReactionCircle());
                }
            }
            else
            {
                reactionTimer = 0;
            }
        }
    }

    IEnumerator FadeInReactionCircle()
    {
        ReactionCircle.interactable = true;
        ReactionCircle.blocksRaycasts = true;

        ReactionCircle.transform.localScale = Vector3.zero;

        while (ReactionCircle.alpha < 1 || LetterCircle.alpha > 0 || ReactionCircle.transform.localScale != Vector3.one)
        {
            ReactionCircle.alpha = Mathf.MoveTowards(ReactionCircle.alpha, 1, Time.deltaTime * 6f);
            LetterCircle.alpha = Mathf.MoveTowards(LetterCircle.alpha, 0, Time.deltaTime * 5f);
            ReactionCircle.transform.localScale = Vector3.MoveTowards(ReactionCircle.transform.localScale, Vector3.one, Time.deltaTime * 5f);
            yield return null;
        }

        reactionCircleShown = true;
    }

    IEnumerator FadeOutReactionCircle()
    {
        ReactionCircle.interactable = false;
        ReactionCircle.blocksRaycasts = false;

        while (ReactionCircle.alpha > 0 || LetterCircle.alpha < 1 || ReactionCircle.transform.localScale != Vector3.zero)
        {
            ReactionCircle.alpha = Mathf.MoveTowards(ReactionCircle.alpha, 0, Time.deltaTime * 6f);
            LetterCircle.alpha = Mathf.MoveTowards(LetterCircle.alpha, 1, Time.deltaTime * 5f);
            ReactionCircle.transform.localScale = Vector3.MoveTowards(ReactionCircle.transform.localScale, Vector3.zero, Time.deltaTime * 5f);

            yield return null;
        }

        reactionCircleShown = false;
    }
}
