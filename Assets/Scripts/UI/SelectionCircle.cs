using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;
using UnityEngine.UI.Extensions;

public class SelectionCircle : MonoBehaviour
{
    public GameObject SelectableTextItem;
    public GameObject SelectedCircle;
    public GameObject SelectionLine;
    public string[] Letters;
    public Transform SelectionCircleContainer;
    public Transform SelectedCircleContainer;
    public Transform SelectionLineContainer;
    public float LetterRadius = 2f;

    public List<LetterConnection> LetterConnections = new List<LetterConnection>();

    List<string> CurrentlyFormingWord = new List<string>();

    private void Start()
    {
        StartCoroutine(RenderLetters());
    }

    IEnumerator RenderLetters()
    {
        // Delete the children
        foreach (Transform child in SelectionCircleContainer)
            Destroy(child.gameObject);

        yield return new WaitForSeconds(0.05f);

        // Render the selectable text item
        for (var i = 0; i < Letters.Length; i++)
        {
            var letter = Letters[i];
            var letterGO = Instantiate(SelectableTextItem, Vector3.zero, Quaternion.Euler(0, 0, 0), SelectionCircleContainer);
            letterGO.GetComponent<TMPro.TextMeshProUGUI>().text = letter.ToString();

            var letterRect = letterGO.GetComponent<RectTransform>();
            var angle = 2 * Mathf.PI * ((float)i / (float)Letters.Length);
            print(angle);
            letterRect.anchoredPosition = Vector3.right * Mathf.Cos(angle) * LetterRadius + Vector3.up * Mathf.Sin(angle) * LetterRadius;

            var pointerDown = new EventTrigger.Entry();
            pointerDown.eventID = EventTriggerType.PointerDown;
            pointerDown.callback.AddListener((e) =>
            {
                CurrentlyFormingWord.Add(letter);
                Instantiate(SelectedCircle, letterGO.transform.position, Quaternion.Euler(0, 0, 0), SelectedCircleContainer);
            });

            var pointerUp = new EventTrigger.Entry();
            pointerUp.eventID = EventTriggerType.PointerUp;
            pointerUp.callback.AddListener((e) =>
            {
                if (CurrentlyFormingWord.Count > 0)
                {
                    print(string.Join("", CurrentlyFormingWord.ToArray()));
                    // Reset the currently formed word
                    CurrentlyFormingWord.Clear();

                    foreach (Transform child in SelectedCircleContainer)
                        Destroy(child.gameObject);

                    foreach (Transform child in SelectionLineContainer)
                        Destroy(child.gameObject);
                }

                mouseLineRenderer = null;
            });

            var pointerEnter = new EventTrigger.Entry();
            pointerEnter.eventID = EventTriggerType.PointerEnter;
            pointerEnter.callback.AddListener((e) =>
            {
                if (CurrentlyFormingWord.Count > 0 && !CurrentlyFormingWord.Contains(letter))
                {
                    CurrentlyFormingWord.Add(letter);
                    Instantiate(SelectedCircle, letterGO.transform.position, Quaternion.Euler(0, 0, 0), SelectedCircleContainer);

                    // Render the connecting lines
                    var prevLetter = CurrentlyFormingWord[CurrentlyFormingWord.Count - 2];
                    var nowLetter = CurrentlyFormingWord[CurrentlyFormingWord.Count - 1];
                    var prevPos = GetPositionForLetter(prevLetter);
                    var nowPos = GetPositionForLetter(nowLetter);

                    var line = Instantiate(SelectionLine, prevPos, Quaternion.Euler(0, 0, 0), SelectionLineContainer).GetComponent<UILineRenderer>();
                    var points = new List<Vector2>();

                    points.Add(Vector3.zero);
                    points.Add(nowPos - prevPos);

                    line.Points = points.ToArray();
                }
            });

            letterGO.GetComponent<EventTrigger>().triggers.Add(pointerDown);
            letterGO.GetComponent<EventTrigger>().triggers.Add(pointerUp);
            letterGO.GetComponent<EventTrigger>().triggers.Add(pointerEnter);

            LetterConnections.Add(new LetterConnection
            {
                Letter = Letters[i].ToString(),
                Target = letterGO.transform
            });
        }
    }

    Vector2 GetPositionForLetter(string letter)
    {
        return LetterConnections.Where(x => x.Letter == letter).FirstOrDefault().Target.position;
    }

    UILineRenderer mouseLineRenderer = null;

    private void Update()
    {
        if (CurrentlyFormingWord.Count > 0)
        {
            var nowLetter = CurrentlyFormingWord[CurrentlyFormingWord.Count - 1];
            var nowPos = GetPositionForLetter(nowLetter);

            // Some word is forming right now
            if (mouseLineRenderer == null)
                mouseLineRenderer = Instantiate(SelectionLine, nowPos, Quaternion.Euler(0, 0, 0), SelectionLineContainer).GetComponent<UILineRenderer>();

            var points = new List<Vector2>();

            mouseLineRenderer.transform.position = nowPos;

            points.Add(Vector2.zero);
            points.Add((Vector2)Input.mousePosition - nowPos);

            mouseLineRenderer.Points = points.ToArray();
        }
    }

    public void RandomizeLetters()
    {
        
    }
}

public class LetterConnection
{
    public string Letter;
    public Transform Target;
}