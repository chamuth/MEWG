using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;
using UnityEngine.UI.Extensions;
using System;

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

    public CanvasGroup FormingWordUI;
    public TMPro.TextMeshProUGUI FormingWordText;

    public List<LetterConnection> LetterConnections = new List<LetterConnection>();

    List<string> CurrentlyFormingWord = new List<string>();
    List<string> CurrentlyFormingWord_IDs = new List<string>();

    bool RenderedOnce = false;

    public void Render(string[] words, bool dev = false)
    {
        if (!RenderedOnce || dev)
        {
            #region Generate Letters for the Words List
            var _letters = new List<string>();
            var _wordLetters = new Dictionary<string, int>();

            foreach (var word in words)
            {
                foreach (var letter in word)
                {
                    var amount = word.Count(x => x.ToString() == letter.ToString());

                    if (_wordLetters.ContainsKey(letter.ToString()) && _wordLetters[letter.ToString()] < amount)
                        _wordLetters[letter.ToString()] = amount;

                    if (!_wordLetters.ContainsKey(letter.ToString()))
                        _wordLetters.Add(letter.ToString(), amount);
                }
            }

            foreach (var l in _wordLetters)
                for (var i = 0; i < l.Value; i++)
                    _letters.Add(l.Key);

            Letters = _letters.ToArray();
            #endregion

            StartCoroutine(RenderLetters());

            RenderedOnce = true;
        }
    }

    IEnumerator RenderLetters()
    {
        // Delete the children
        foreach (Transform child in SelectionCircleContainer)
            Destroy(child.gameObject);

        LetterConnections.Clear();

        yield return new WaitForSeconds(0.05f);

        // Render the selectable text item
        for (var i = 0; i < Letters.Length; i++)
        {
            var _id = Letters[i].ToString();

            // for multiple occurances of the same letter
            var occurancesCount = 0;

            if (i > 0)
                occurancesCount = Letters.Take(i).Count(x => x == Letters[i]);

            _id = _id + occurancesCount.ToString();
            
            var letter = Letters[i];
            var letterGO = Instantiate(SelectableTextItem, Vector3.zero, Quaternion.Euler(0, 0, 0), SelectionCircleContainer);
            letterGO.GetComponent<TMPro.TextMeshProUGUI>().text = letter.ToString();
            letterGO.GetComponent<LetterID>().ID = _id;

            var letterRect = letterGO.GetComponent<RectTransform>();
            var angle = 2 * Mathf.PI * ((float)i / (float)Letters.Length);

            letterRect.anchoredPosition = Vector3.right * Mathf.Cos(angle) * LetterRadius + Vector3.up * Mathf.Sin(angle) * LetterRadius;

            var pointerDown = new EventTrigger.Entry();
            pointerDown.eventID = EventTriggerType.PointerDown;
            pointerDown.callback.AddListener((e) =>
            {
                CurrentlyFormingWord.Add(letter);
                CurrentlyFormingWord_IDs.Add(_id);
                Instantiate(SelectedCircle, letterGO.transform.position, Quaternion.Euler(0, 0, 0), SelectedCircleContainer);
                SoundManager.Instance.SelectLetter(CurrentlyFormingWord.Count);
            });

            var pointerUp = new EventTrigger.Entry();
            pointerUp.eventID = EventTriggerType.PointerUp;
            pointerUp.callback.AddListener((e) =>
            {
                if (CurrentlyFormingWord.Count > 0)
                {
                    var word = string.Join("", CurrentlyFormingWord.ToArray());

                    var correct = Game.MatchWord(word);

                    // If the answer is incorrect play the shake animation
                    if (!correct)
                        FormingWordUI.gameObject.GetComponent<Animator>().Play("Shake");

                    // Reset the currently formed word
                    CurrentlyFormingWord.Clear();
                    CurrentlyFormingWord_IDs.Clear();

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
                if (CurrentlyFormingWord.Count > 0 && !CurrentlyFormingWord_IDs.Contains(_id))
                {
                    CurrentlyFormingWord.Add(letter);
                    CurrentlyFormingWord_IDs.Add(_id);

                    Instantiate(SelectedCircle, letterGO.transform.position, Quaternion.Euler(0, 0, 0), SelectedCircleContainer);

                    // Render the connecting lines
                    var prevLetter = CurrentlyFormingWord_IDs[CurrentlyFormingWord_IDs.Count - 2];
                    var nowLetter = CurrentlyFormingWord_IDs[CurrentlyFormingWord_IDs.Count - 1];
                    var prevPos = GetPositionForLetter(prevLetter);
                    var nowPos = GetPositionForLetter(nowLetter);

                    var line = Instantiate(SelectionLine, prevPos, Quaternion.Euler(0, 0, 0), SelectionLineContainer).GetComponent<UILineRenderer>();

                    line.transform.localScale = Vector3.one;

                    var points = new List<Vector2>();

                    points.Add(Vector3.zero);
                    points.Add((nowPos - prevPos));

                    line.Points = points.ToArray();

                    // Play the sound
                    SoundManager.Instance.SelectLetter(CurrentlyFormingWord.Count);
                }
            });

            letterGO.GetComponent<EventTrigger>().triggers.Add(pointerDown);
            letterGO.GetComponent<EventTrigger>().triggers.Add(pointerUp);
            letterGO.GetComponent<EventTrigger>().triggers.Add(pointerEnter);

            LetterConnections.Add(new LetterConnection
            {
                Letter = Letters[i].ToString(),
                Id = _id,
                Target = letterGO.transform
            });
        }

        yield return new WaitForSeconds(5f);

        StartCoroutine(FadeInLetters());
    }

    IEnumerator FadeInLetters()
    {
        var cg = SelectionCircleContainer.gameObject.GetComponent<CanvasGroup>();

        while (cg.alpha < 1)
        {
            cg.alpha += Time.deltaTime * 2f;
            yield return null;
        }
    }

    IEnumerator FadeOutLetters()
    {
        var cg = SelectionCircleContainer.gameObject.GetComponent<CanvasGroup>();

        while (cg.alpha > 0)
        {
            cg.alpha -= Time.deltaTime * 4f;
            yield return null;
        }
    }

    Vector2 GetPositionForLetter(string id)
    {
        return LetterConnections.Where(x => x.Id == id).FirstOrDefault().Target.position;
    }

    UILineRenderer mouseLineRenderer = null;

    private void Update()
    {
        if (CurrentlyFormingWord.Count > 0)
        {
            var nowLetter = CurrentlyFormingWord_IDs[CurrentlyFormingWord_IDs.Count - 1];
            var nowPos = GetPositionForLetter(nowLetter);

            // Some word is forming right now
            if (mouseLineRenderer == null)
                mouseLineRenderer = Instantiate(SelectionLine, nowPos, Quaternion.Euler(0, 0, 0), SelectionLineContainer).GetComponent<UILineRenderer>();

            var points = new List<Vector2>();

            mouseLineRenderer.transform.position = nowPos;
            points.Add(Vector2.zero);
            points.Add(((Vector2)Input.mousePosition - nowPos));

            mouseLineRenderer.Points = points.ToArray();
        }

        if (CurrentlyFormingWord.Count > 0)
        {
            FormingWordUI.alpha = Mathf.MoveTowards(FormingWordUI.alpha, 1, Time.deltaTime * 4f);
            FormingWordText.text = string.Join("", CurrentlyFormingWord.ToArray());
        }
        else
        {
            if (FormingWordUI.alpha > 0)
                FormingWordUI.alpha = Mathf.MoveTowards(FormingWordUI.alpha, 0, Time.deltaTime * 4f);
        }
    }

    public void ShowReactionButtons()
    {

    }

    public void RandomizeLetters()
    {
        StartCoroutine(FadeOutLetters());
        StartCoroutine(_RandomizeLetters());
    }

    IEnumerator _RandomizeLetters()
    {
        yield return new WaitForSeconds(0.3f);

        var rnd = new System.Random();
        Letters = Letters.OrderBy(x => rnd.Next()).ToArray();

        StartCoroutine(RenderLetters());

        yield return new WaitForSeconds(0.3f);

        StartCoroutine(FadeInLetters());
    }
}

public class LetterConnection
{
    public string Letter;
    public string Id;
    public Transform Target;
}