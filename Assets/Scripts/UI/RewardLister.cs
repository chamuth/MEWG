using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class RewardLister : MonoBehaviour
{
    [HideInInspector()]
    public CanvasGroup[] Rewards;

    public void ProcessRewards()
    {
        // Load in the rewards
        var rewards = new List<CanvasGroup>();
        foreach (Transform child in transform)
            rewards.Add(child.gameObject.GetComponent<CanvasGroup>());
        Rewards = rewards.ToArray();

        StartCoroutine(PresentRewards());
    }

    IEnumerator PresentRewards()
    {
        foreach(var cg in Rewards)
        {
            cg.alpha = 0;
            cg.gameObject.transform.localScale = Vector3.zero;
        }

        int currentIndex = 0;

        while (currentIndex < Rewards.Length)
        {
            // Show the current reward
            while (Rewards[currentIndex].alpha < 1 || Rewards[currentIndex].gameObject.transform.localScale != Vector3.one)
            {
                Rewards[currentIndex].alpha = Mathf.MoveTowards(Rewards[currentIndex].alpha, 1, Time.deltaTime * 4f);
                Rewards[currentIndex].gameObject.transform.localScale = Vector3.MoveTowards(Rewards[currentIndex].gameObject.transform.localScale, Vector3.one, Time.deltaTime * 4f);
                yield return null;
            }

            // Hide it only if its not the last one
            if (currentIndex + 1 != Rewards.Length)
            {
                yield return new WaitForSeconds(4);

                // Hide the current reward
                while (Rewards[currentIndex].alpha > 0 || Rewards[currentIndex].gameObject.transform.localScale != Vector3.one * 2f)
                {
                    Rewards[currentIndex].alpha = Mathf.MoveTowards(Rewards[currentIndex].alpha, 0, Time.deltaTime * 5f);
                    Rewards[currentIndex].gameObject.transform.localScale = Vector3.MoveTowards(Rewards[currentIndex].gameObject.transform.localScale, Vector3.one * 2f, Time.deltaTime * 5f);
                    yield return null;
                }
            }

            currentIndex++;
        }
    }
}
